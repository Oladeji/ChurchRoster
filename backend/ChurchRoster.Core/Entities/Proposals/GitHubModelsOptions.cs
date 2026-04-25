namespace ChurchRoster.Core.Entities.Proposals;

/// <summary>
/// Bound from the "AIProvider" config section.
/// Set Provider = "Ollama" for local testing, "GitHub" for production.
/// </summary>
public class GitHubModelsOptions
{
    /// <summary>"GitHub" or "Ollama"</summary>
    public string Provider { get; set; } = "GitHub";

    // ── GitHub Models ──────────────────────────────────────────────────────────
    public string Endpoint { get; set; } = "https://models.inference.ai.azure.com";
    public string ModelName { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;

    // ── Ollama (local) ─────────────────────────────────────────────────────────
    /// <summary>Base URL of the local Ollama server, e.g. http://localhost:11434</summary>
    public string OllamaEndpoint { get; set; } = "http://localhost:11434";
    /// <summary>Model tag to use, e.g. "llama3.2", "mistral", "qwen2.5"</summary>
    public string OllamaModel { get; set; } = "llama3.2";
}
