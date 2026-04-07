using ChurchRoster.Application.DTOs.Reports;
using ChurchRoster.Application.Interfaces;
using ChurchRoster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace ChurchRoster.Application.Services;

public class ReportService : IReportService
{
    private readonly AppDbContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<ReportService> _logger;

    public ReportService(AppDbContext context, ITenantContext tenantContext, ILogger<ReportService> logger)
    {
        _context = context;
        _tenantContext = tenantContext;
        _logger = logger;

        // Configure QuestPDF license (Community for open-source/non-commercial)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateMonthlyRosterAsync(int year, int month)
    {
        try
        {
            if (!_tenantContext.TenantId.HasValue)
                throw new InvalidOperationException("Tenant context is required to generate reports");

            // Get data - PostgreSQL requires UTC DateTimes
            var startDate = DateTime.SpecifyKind(new DateTime(year, month, 1), DateTimeKind.Utc);
            var endDate = DateTime.SpecifyKind(startDate.AddMonths(1).AddDays(-1), DateTimeKind.Utc);

            var assignments = await _context.Assignments
                .Include(a => a.Task)
                .Include(a => a.User)
                .Where(a => a.EventDate >= startDate && a.EventDate <= endDate)
                .OrderBy(a => a.EventDate)
                .ThenBy(a => a.Task.TaskName)
                .ToListAsync();

            // Project to DTO in memory to properly convert DayOfWeek enum to string
            var assignmentItems = assignments.Select(a => new AssignmentReportItem
            {
                TaskName = a.Task.TaskName,
                MemberName = a.User.Name,
                EventDate = a.EventDate,
                DayOfWeek = a.EventDate.DayOfWeek.ToString(),
                Status = a.Status,
                TaskFrequency = a.Task.Frequency
            }).ToList();

            // Group by week
            var calendar = CultureInfo.CurrentCulture.Calendar;
            var weeks = assignmentItems
                .GroupBy(a => calendar.GetWeekOfYear(
                    a.EventDate,
                    CalendarWeekRule.FirstDay,
                    DayOfWeek.Sunday))
                .Select(g => new WeeklyAssignments
                {
                    WeekNumber = g.Key,
                    StartDate = g.Min(a => a.EventDate),
                    EndDate = g.Max(a => a.EventDate),
                    Assignments = g.OrderBy(a => a.EventDate).ThenBy(a => a.TaskName).ToList()
                })
                .OrderBy(w => w.StartDate)
                .ToList();

            var reportData = new MonthlyRosterDto
            {
                Year = year,
                Month = month,
                MonthName = startDate.ToString("MMMM yyyy"),
                Weeks = weeks
            };

            // Generate PDF
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .PaddingBottom(10)
                        .BorderBottom(1)
                        .BorderColor(Colors.Grey.Medium)
                        .Column(column =>
                        {
                            column.Item().AlignCenter().Text("Church Ministry Roster")
                                .FontSize(20)
                                .Bold()
                                .FontColor(Colors.Blue.Darken3);

                            column.Item().AlignCenter().Text(reportData.MonthName)
                                .FontSize(16)
                                .SemiBold();

                            column.Item().AlignCenter().Text($"Generated: {DateTime.Now:MMM dd, yyyy hh:mm tt}")
                                .FontSize(8)
                                .FontColor(Colors.Grey.Darken1);
                        });

                    page.Content()
                        .PaddingVertical(15)
                        .Column(column =>
                        {
                            if (!reportData.Weeks.Any())
                            {
                                column.Item().AlignCenter().PaddingTop(50).Text("No assignments for this month")
                                    .FontSize(14)
                                    .FontColor(Colors.Grey.Darken1);
                                return;
                            }

                            foreach (var week in reportData.Weeks)
                            {
                                column.Item().PaddingBottom(15).Element(container =>
                                {
                                    container.Column(weekColumn =>
                                    {
                                        // Week header
                                        weekColumn.Item()
                                            .Background(Colors.Blue.Lighten4)
                                            .Padding(8)
                                            .Text($"Week {week.WeekNumber}: {week.StartDate:MMM dd} - {week.EndDate:MMM dd}")
                                            .FontSize(12)
                                            .SemiBold();

                                        // Assignments table
                                        weekColumn.Item().Table(table =>
                                        {
                                            table.ColumnsDefinition(columns =>
                                            {
                                                columns.ConstantColumn(70);  // Date
                                                columns.ConstantColumn(80);  // Day
                                                columns.RelativeColumn(3);   // Task
                                                columns.RelativeColumn(2);   // Member
                                                columns.ConstantColumn(70);  // Status
                                            });

                                            // Table header
                                            table.Header(header =>
                                            {
                                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Date").SemiBold();
                                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Day").SemiBold();
                                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Task").SemiBold();
                                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Assigned To").SemiBold();
                                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Status").SemiBold();
                                            });

                                            // Table rows
                                            foreach (var assignment in week.Assignments)
                                            {
                                                var statusColor = assignment.Status switch
                                                {
                                                    "Accepted" => Colors.Green.Lighten3,
                                                    "Rejected" => Colors.Red.Lighten3,
                                                    "Confirmed" => Colors.Blue.Lighten3,
                                                    "Completed" => Colors.Grey.Lighten2,
                                                    _ => Colors.Yellow.Lighten3
                                                };

                                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                                    .Text(assignment.EventDate.ToString("MMM dd"));
                                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                                    .Text(assignment.DayOfWeek);
                                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                                    .Text(assignment.TaskName);
                                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                                    .Text(assignment.MemberName);
                                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                                    .Background(statusColor)
                                                    .Text(assignment.Status)
                                                    .FontSize(9);
                                            }
                                        });
                                    });
                                });
                            }

                            // Summary section
                            column.Item().PaddingTop(15).BorderTop(1).BorderColor(Colors.Grey.Medium).PaddingTop(10)
                                .Row(row =>
                                {
                                    row.RelativeItem().Column(col =>
                                    {
                                        col.Item().Text($"Total Assignments: {assignments.Count}").SemiBold();
                                        col.Item().Text($"Pending: {assignments.Count(a => a.Status == "Pending")}");
                                        col.Item().Text($"Accepted: {assignments.Count(a => a.Status == "Accepted")}");
                                    });

                                    row.RelativeItem().Column(col =>
                                    {
                                        col.Item().Text($"Rejected: {assignments.Count(a => a.Status == "Rejected")}");
                                        col.Item().Text($"Confirmed: {assignments.Count(a => a.Status == "Confirmed")}");
                                        col.Item().Text($"Completed: {assignments.Count(a => a.Status == "Completed")}");
                                    });
                                });
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("Page ");
                            text.CurrentPageNumber();
                            text.Span(" of ");
                            text.TotalPages();
                        });
                });
            });

            _logger.LogInformation("Generated monthly roster for {Year}-{Month}", year, month);
            return document.GeneratePdf();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate monthly roster for {Year}-{Month}", year, month);
            throw;
        }
    }

    public async Task<byte[]> GenerateMemberScheduleAsync(int userId, DateTime startDate, DateTime endDate)
    {
        try
        {
            if (!_tenantContext.TenantId.HasValue)
                throw new InvalidOperationException("Tenant context is required to generate reports");

            // Ensure UTC DateTimes for PostgreSQL
            var utcStartDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var utcEndDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                throw new ArgumentException($"User with ID {userId} not found");
            }

            var assignments = await _context.Assignments
                .Include(a => a.Task)
                .Where(a => a.UserId == userId && a.EventDate >= utcStartDate && a.EventDate <= utcEndDate)
                .OrderBy(a => a.EventDate)
                .ToListAsync();

            // Project to DTO in memory to properly convert DayOfWeek enum to string
            var assignmentItems = assignments.Select(a => new AssignmentReportItem
            {
                TaskName = a.Task.TaskName,
                MemberName = user.Name,
                EventDate = a.EventDate,
                DayOfWeek = a.EventDate.DayOfWeek.ToString(),
                Status = a.Status,
                TaskFrequency = a.Task.Frequency
            }).ToList();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .PaddingBottom(10)
                        .BorderBottom(1)
                        .BorderColor(Colors.Grey.Medium)
                        .Column(column =>
                        {
                            column.Item().AlignCenter().Text("Ministry Assignment Schedule")
                                .FontSize(20)
                                .Bold()
                                .FontColor(Colors.Blue.Darken3);

                            column.Item().AlignCenter().Text(user.Name)
                                .FontSize(16)
                                .SemiBold();

                            column.Item().AlignCenter().Text($"{startDate:MMM dd, yyyy} - {endDate:MMM dd, yyyy}")
                                .FontSize(12);

                            column.Item().AlignCenter().Text($"Generated: {DateTime.Now:MMM dd, yyyy hh:mm tt}")
                                .FontSize(8)
                                .FontColor(Colors.Grey.Darken1);
                        });

                    page.Content()
                        .PaddingVertical(15)
                        .Column(column =>
                        {
                            if (!assignmentItems.Any())
                            {
                                column.Item().AlignCenter().PaddingTop(50).Text("No assignments found for this period")
                                    .FontSize(14)
                                    .FontColor(Colors.Grey.Darken1);
                                return;
                            }

                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(100); // Date
                                    columns.ConstantColumn(80);  // Day
                                    columns.RelativeColumn();     // Task
                                    columns.ConstantColumn(80);  // Status
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Date").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Day").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Task").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Status").SemiBold();
                                });

                                foreach (var assignment in assignmentItems)
                                {
                                    var statusColor = assignment.Status switch
                                    {
                                        "Accepted" => Colors.Green.Lighten3,
                                        "Rejected" => Colors.Red.Lighten3,
                                        "Confirmed" => Colors.Blue.Lighten3,
                                        "Completed" => Colors.Grey.Lighten2,
                                        _ => Colors.Yellow.Lighten3
                                    };

                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                        .Text(assignment.EventDate.ToString("MMM dd, yyyy"));
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                        .Text(assignment.DayOfWeek);
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                        .Text(assignment.TaskName);
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                        .Background(statusColor)
                                        .Text(assignment.Status);
                                }
                            });

                            // Summary
                            column.Item().PaddingTop(15).BorderTop(1).BorderColor(Colors.Grey.Medium).PaddingTop(10)
                                .Row(row =>
                                {
                                    row.RelativeItem().Text($"Total Assignments: {assignmentItems.Count}").SemiBold();
                                    row.RelativeItem().AlignRight().Text($"Pending: {assignmentItems.Count(a => a.Status == "Pending")} | " +
                                        $"Accepted: {assignmentItems.Count(a => a.Status == "Accepted")} | " +
                                        $"Completed: {assignmentItems.Count(a => a.Status == "Completed")}");
                                });
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("Page ");
                            text.CurrentPageNumber();
                        });
                });
            });

            _logger.LogInformation("Generated member schedule for user {UserId}", userId);
            return document.GeneratePdf();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate member schedule for user {UserId}", userId);
            throw;
        }
    }

    public async Task<byte[]> GenerateTaskAssignmentReportAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            if (!_tenantContext.TenantId.HasValue)
                throw new InvalidOperationException("Tenant context is required to generate reports");

            // Ensure UTC DateTimes for PostgreSQL
            var utcStartDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var utcEndDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

            var taskAssignments = await _context.Assignments
                .Include(a => a.Task)
                .Include(a => a.User)
                .Where(a => a.EventDate >= utcStartDate && a.EventDate <= utcEndDate)
                .GroupBy(a => a.Task.TaskName)
                .Select(g => new
                {
                    TaskName = g.Key,
                    Assignments = g.OrderBy(a => a.EventDate).Select(a => new
                    {
                        a.EventDate,
                        MemberName = a.User.Name,
                        a.Status
                    }).ToList()
                })
                .OrderBy(t => t.TaskName)
                .ToListAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .PaddingBottom(10)
                        .BorderBottom(1)
                        .BorderColor(Colors.Grey.Medium)
                        .Column(column =>
                        {
                            column.Item().AlignCenter().Text("Task Assignment Report")
                                .FontSize(20)
                                .Bold()
                                .FontColor(Colors.Blue.Darken3);

                            column.Item().AlignCenter().Text($"{startDate:MMM dd, yyyy} - {endDate:MMM dd, yyyy}")
                                .FontSize(12);

                            column.Item().AlignCenter().Text($"Generated: {DateTime.Now:MMM dd, yyyy hh:mm tt}")
                                .FontSize(8)
                                .FontColor(Colors.Grey.Darken1);
                        });

                    page.Content().PaddingVertical(15).Column(column =>
                    {
                        if (!taskAssignments.Any())
                        {
                            column.Item().AlignCenter().PaddingTop(50).Text("No assignments found for this period")
                                .FontSize(14)
                                .FontColor(Colors.Grey.Darken1);
                            return;
                        }

                        foreach (var task in taskAssignments)
                        {
                            column.Item().PaddingBottom(10).Element(container =>
                            {
                                container.Column(taskColumn =>
                                {
                                    taskColumn.Item().Background(Colors.Blue.Lighten4).Padding(5)
                                        .Text(task.TaskName).SemiBold();

                                    taskColumn.Item().Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.ConstantColumn(100);
                                            columns.RelativeColumn();
                                            columns.ConstantColumn(80);
                                        });

                                        table.Header(header =>
                                        {
                                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Date");
                                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Member");
                                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Status");
                                        });

                                        foreach (var assignment in task.Assignments)
                                        {
                                            var statusColor = assignment.Status switch
                                            {
                                                "Accepted" => Colors.Green.Lighten3,
                                                "Rejected" => Colors.Red.Lighten3,
                                                "Confirmed" => Colors.Blue.Lighten3,
                                                "Completed" => Colors.Grey.Lighten2,
                                                _ => Colors.Yellow.Lighten3
                                            };

                                            table.Cell().BorderBottom(0.5f).Padding(5)
                                                .Text(assignment.EventDate.ToString("MMM dd"));
                                            table.Cell().BorderBottom(0.5f).Padding(5)
                                                .Text(assignment.MemberName);
                                            table.Cell().BorderBottom(0.5f).Padding(5)
                                                .Background(statusColor)
                                                .Text(assignment.Status);
                                        }
                                    });
                                });
                            });
                        }
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("Page ");
                            text.CurrentPageNumber();
                            text.Span(" of ");
                            text.TotalPages();
                        });
                });
            });

            _logger.LogInformation("Generated task assignment report");
            return document.GeneratePdf();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate task assignment report");
            throw;
        }
    }
}
