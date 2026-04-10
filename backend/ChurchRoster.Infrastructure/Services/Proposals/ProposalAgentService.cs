using System.ClientModel;
using ChurchRoster.Core.Entities.Proposals;
using ChurchRoster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;

// Alias the OpenAI SDK types explicitly to avoid ambiguity with Microsoft.Extensions.AI
using OAIChatClient = OpenAI.Chat.ChatClient;

namespace ChurchRoster.Infrastructure.Services.Proposals;

/// <summary>
/// Runs the Microsoft.Extensions.AI agent loop against GitHub Models.
/// Called by ProposalGenerationJob (BackgroundService) — never directly from HTTP.
/// </summary>
public class ProposalAgentService
{
    private readonly IProposalAgentTools _tools;
    private readonly AppDbContext _db;
    private readonly GitHubModelsOptions _options;
    private readonly ILogger<ProposalAgentService> _logger;

    public ProposalAgentService(
        IProposalAgentTools tools,
        AppDbContext db,
        IOptions<GitHubModelsOptions> options,
        ILogger<ProposalAgentService> logger)
    {
        _tools = tools;
        _db = db;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Runs the full agent loop for a given proposal.
    /// Sets Status = Draft on success, or logs and re-throws on failure.
    /// </summary>
    public async Task GenerateAsync(int proposalId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Agent generation starting for ProposalId={ProposalId}", proposalId);

        var proposal = await _db.RosterProposals
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.ProposalId == proposalId, cancellationToken);

        if (proposal is null)
        {
            _logger.LogError("ProposalId={ProposalId} not found — aborting generation", proposalId);
            return;
        }

        try
        {
            // Build IChatClient pointing at GitHub Models endpoint
            var client = BuildChatClient();

            // Register all 6 tool functions via reflection on the tools instance
            var tools = new List<AITool>
            {
                AIFunctionFactory.Create(_tools.GetRecurringTasksAsync,        "GetRecurringTasksAsync"),
                AIFunctionFactory.Create(_tools.GetQualifiedMembersAsync,      "GetQualifiedMembersAsync"),
                AIFunctionFactory.Create(_tools.GetMemberAssignmentCountAsync, "GetMemberAssignmentCountAsync"),
                AIFunctionFactory.Create(_tools.GetExistingAssignmentsAsync,   "GetExistingAssignmentsAsync"),
                AIFunctionFactory.Create(_tools.CreateProposalItemAsync,       "CreateProposalItemAsync"),
                AIFunctionFactory.Create(_tools.LogSkippedSlotAsync,           "LogSkippedSlotAsync"),
            };

            var chatOptions = new ChatOptions
            {
                Tools = tools,
                ToolMode = ChatToolMode.Auto,
                MaxOutputTokens = 4096,
            };

            var messages = new List<Microsoft.Extensions.AI.ChatMessage>
            {
                new(Microsoft.Extensions.AI.ChatRole.System, BuildSystemPrompt(proposal)),
                new(Microsoft.Extensions.AI.ChatRole.User,
                    $"Generate a complete roster proposal named \"{proposal.Name}\" " +
                    $"for tenantId={proposal.TenantId} " +
                    $"covering {proposal.DateRangeStart:yyyy-MM-dd} to {proposal.DateRangeEnd:yyyy-MM-dd}. " +
                    $"The proposalId to write items into is {proposal.ProposalId}. " +
                    $"When you are finished, reply with only: GENERATION_COMPLETE"),
            };

            // Agent loop — keep calling the model until it stops invoking tools
            int maxRounds = 50; // safety cap — prevents infinite loops
            int round = 0;

            while (round++ < maxRounds)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var response = await client.GetResponseAsync(
                    (IEnumerable<Microsoft.Extensions.AI.ChatMessage>)messages, chatOptions, cancellationToken);

                // Add assistant turn to history
                messages.Add(new Microsoft.Extensions.AI.ChatMessage(Microsoft.Extensions.AI.ChatRole.Assistant, response.Text));

                _logger.LogDebug("Agent round {Round}: FinishReason={Finish}, Text={Text}",
                    round, response.FinishReason, response.Text?.Substring(0, Math.Min(120, response.Text?.Length ?? 0)));

                // Stop when the model signals completion or stops calling tools
                if (response.FinishReason == Microsoft.Extensions.AI.ChatFinishReason.Stop)
                    break;
            }

            if (round >= maxRounds)
                _logger.LogWarning("ProposalId={ProposalId} hit max agent rounds ({Max}) — saving partial draft", proposalId, maxRounds);

            // Mark proposal as Draft
            proposal.Status = ProposalStatus.Draft;
            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Agent generation complete for ProposalId={ProposalId}", proposalId);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Agent generation cancelled for ProposalId={ProposalId}", proposalId);
            proposal.Status = ProposalStatus.Archived; // mark as dead so UI doesn't spin forever
            await _db.SaveChangesAsync(CancellationToken.None);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Agent generation failed for ProposalId={ProposalId}", proposalId);
            proposal.Status = ProposalStatus.Archived;
            await _db.SaveChangesAsync(CancellationToken.None);
            throw;
        }
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private IChatClient BuildChatClient()
    {
        if (string.IsNullOrWhiteSpace(_options.Token))
            throw new InvalidOperationException(
                "GitHubModels:Token is not configured. " +
                "Add it to appsettings.Development.json or set the GitHubModels__Token environment variable.");

        if (string.IsNullOrWhiteSpace(_options.ModelName))
            throw new InvalidOperationException(
                "GitHubModels:ModelName is not configured. " +
                "Add it to appsettings.Development.json or set the GitHubModels__ModelName environment variable.");

        var endpoint = string.IsNullOrWhiteSpace(_options.Endpoint)
            ? "https://models.inference.ai.azure.com"
            : _options.Endpoint;

        // OpenAI SDK supports custom endpoints via OpenAIClientOptions
        var openAiOptions = new OpenAIClientOptions
        {
            Endpoint = new Uri(endpoint)
        };

        var credential = new ApiKeyCredential(_options.Token);
        var openAiClient = new OpenAIClient(credential, openAiOptions);
        OAIChatClient chatClient = openAiClient.GetChatClient(_options.ModelName);

        // Wrap with Microsoft.Extensions.AI pipeline:
        // UseFunctionInvocation() automatically handles tool-call round-trips
        return new Microsoft.Extensions.AI.ChatClientBuilder(chatClient.AsIChatClient())
            .UseFunctionInvocation()
            .Build();
    }

    private static string BuildSystemPrompt(RosterProposal proposal) => $"""
        You are an expert church ministry scheduler. Your job is to generate a complete
        roster proposal for a church by calling the provided tool functions.

        ## Your workflow
        1. Call GetRecurringTasksAsync to retrieve all tasks and their rules.
        2. For each task, determine which dates in the range it falls on:
           - "Weekly" tasks with DayRule like "Tuesday" or "Sunday" recur every week.
           - "Monthly" tasks with DayRule like "Last Friday" or "Last Saturday"
             occur once on the last occurrence of that weekday in each month.
        3. For each task+date slot:
           a. Call GetQualifiedMembersAsync to get eligible members (pass taskId=0 for
              unrestricted tasks to get all active members).
           b. Call GetExistingAssignmentsAsync to check for existing live conflicts.
           c. Call GetMemberAssignmentCountAsync to pick the member with the fewest
              assignments that month (fairness-first distribution).
           d. If a qualified member is available: call CreateProposalItemAsync.
           e. If no member is available: call LogSkippedSlotAsync with a clear reason.
        4. When ALL task+date slots in the range have been processed, reply with only:
           GENERATION_COMPLETE

        ## Rules you must follow
        - Never assign the same member to more than one task on the same date.
        - Respect each member's MonthlyLimit (if set). Do not exceed it.
        - Only assign members returned by GetQualifiedMembersAsync — never invent user IDs.
        - Restricted tasks (IsRestricted=true) MUST use members from GetQualifiedMembersAsync
          with the correct taskId — never pass taskId=0 for restricted tasks.
        - If a live assignment already exists for the same taskId+date (from
          GetExistingAssignmentsAsync), skip that slot and log it.
        - Date range: {proposal.DateRangeStart:yyyy-MM-dd} to {proposal.DateRangeEnd:yyyy-MM-dd}
        - TenantId: {proposal.TenantId}
        - ProposalId: {proposal.ProposalId}

        ## Output format
        Do not produce any markdown, JSON, or explanatory text.
        Only make tool calls, and when done reply: GENERATION_COMPLETE
        """;
}
