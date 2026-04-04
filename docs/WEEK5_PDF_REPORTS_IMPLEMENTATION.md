# 📄 PDF Report Generation Implementation
**Church Ministry Roster System - Week 5, Day 6**

---

## 📋 Overview

This guide implements printable PDF reports for:
- ✅ Monthly Ministry Roster (all assignments for a month)
- ✅ Individual Member Schedule
- ✅ Task Assignment Report (who's assigned to what)
- ✅ Professional church-branded PDF layout

**Technology:** QuestPDF (free, .NET library with commercial-friendly license)

---

## 🎯 Report Types

### 1. Monthly Roster Report
- Shows all assignments for selected month
- Grouped by week
- Includes task name, assigned member, date, status
- Useful for printing and posting on bulletin board

### 2. Member Schedule Report
- Shows all assignments for a specific member
- Date range filter
- Includes upcoming and past assignments
- Member can download their own schedule

### 3. Task Assignment Report
- Shows who's assigned to each task
- Date range filter
- Useful for coordinators to see coverage

---

## 🔧 Implementation

### **Part 1: Backend Implementation**

#### 1.1 Install QuestPDF Package

```bash
cd backend/ChurchRoster.Infrastructure
dotnet add package QuestPDF --version 2024.1.3
```

**⚠️ License Note:** QuestPDF is free for open-source projects. For commercial use, you may need a license ($99/year). See: https://www.questpdf.com/license/

#### 1.2 Create Report Service Interface

Create `backend/ChurchRoster.Application/Interfaces/IReportService.cs`:

```csharp
namespace ChurchRoster.Application.Interfaces;

public interface IReportService
{
    Task<byte[]> GenerateMonthlyRosterAsync(int year, int month);
    Task<byte[]> GenerateMemberScheduleAsync(int userId, DateTime startDate, DateTime endDate);
    Task<byte[]> GenerateTaskAssignmentReportAsync(DateTime startDate, DateTime endDate);
}
```

#### 1.3 Create Report DTOs

Create `backend/ChurchRoster.Application/DTOs/Reports/MonthlyRosterDto.cs`:

```csharp
namespace ChurchRoster.Application.DTOs.Reports;

public class MonthlyRosterDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public List<WeeklyAssignments> Weeks { get; set; } = new();
}

public class WeeklyAssignments
{
    public int WeekNumber { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<AssignmentReportItem> Assignments { get; set; } = new();
}

public class AssignmentReportItem
{
    public string TaskName { get; set; } = string.Empty;
    public string MemberName { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string DayOfWeek { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string TaskFrequency { get; set; } = string.Empty;
}
```

#### 1.4 Implement Report Service

Create `backend/ChurchRoster.Infrastructure/Services/ReportService.cs`:

```csharp
using ChurchRoster.Application.DTOs.Reports;
using ChurchRoster.Application.Interfaces;
using ChurchRoster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace ChurchRoster.Infrastructure.Services;

public class ReportService : IReportService
{
    private readonly AppDbContext _context;

    public ReportService(AppDbContext context)
    {
        _context = context;

        // Configure QuestPDF license
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateMonthlyRosterAsync(int year, int month)
    {
        // Get data
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var assignments = await _context.Assignments
            .Include(a => a.Task)
            .Include(a => a.User)
            .Where(a => a.EventDate >= startDate && a.EventDate <= endDate)
            .OrderBy(a => a.EventDate)
            .ThenBy(a => a.Task.TaskName)
            .Select(a => new AssignmentReportItem
            {
                TaskName = a.Task.TaskName,
                MemberName = a.User.Name,
                EventDate = a.EventDate,
                DayOfWeek = a.EventDate.DayOfWeek.ToString(),
                Status = a.Status,
                TaskFrequency = a.Task.Frequency
            })
            .ToListAsync();

        // Group by week
        var weeks = assignments
            .GroupBy(a => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                a.EventDate, 
                CalendarWeekRule.FirstDay, 
                DayOfWeek.Sunday))
            .Select(g => new WeeklyAssignments
            {
                WeekNumber = g.Key,
                StartDate = g.Min(a => a.EventDate),
                EndDate = g.Max(a => a.EventDate),
                Assignments = g.ToList()
            })
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
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Header()
                    .PaddingBottom(10)
                    .BorderBottom(1)
                    .BorderColor(Colors.Grey.Medium)
                    .Column(column =>
                    {
                        column.Item().AlignCenter().Text($"Church Ministry Roster")
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
                        foreach (var week in reportData.Weeks)
                        {
                            column.Item().PaddingBottom(15).Element(container =>
                            {
                                // Week header
                                container.Column(weekColumn =>
                                {
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
                                            columns.ConstantColumn(70); // Date
                                            columns.ConstantColumn(80); // Day
                                            columns.RelativeColumn(3);  // Task
                                            columns.RelativeColumn(2);  // Member
                                            columns.ConstantColumn(70); // Status
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

                                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(assignment.EventDate.ToString("MMM dd"));
                                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(assignment.DayOfWeek);
                                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(assignment.TaskName);
                                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(assignment.MemberName);
                                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Background(statusColor).Text(assignment.Status).FontSize(9);
                                        }
                                    });
                                });
                            });
                        }

                        // Summary section
                        column.Item().PaddingTop(15).BorderTop(1).BorderColor(Colors.Grey.Medium).PaddingTop(10).Row(row =>
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

        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateMemberScheduleAsync(int userId, DateTime startDate, DateTime endDate)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) throw new ArgumentException("User not found");

        var assignments = await _context.Assignments
            .Include(a => a.Task)
            .Where(a => a.UserId == userId && a.EventDate >= startDate && a.EventDate <= endDate)
            .OrderBy(a => a.EventDate)
            .Select(a => new AssignmentReportItem
            {
                TaskName = a.Task.TaskName,
                MemberName = user.Name,
                EventDate = a.EventDate,
                DayOfWeek = a.EventDate.DayOfWeek.ToString(),
                Status = a.Status,
                TaskFrequency = a.Task.Frequency
            })
            .ToListAsync();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Header()
                    .Column(column =>
                    {
                        column.Item().AlignCenter().Text("Ministry Assignment Schedule")
                            .FontSize(20).Bold().FontColor(Colors.Blue.Darken3);
                        column.Item().AlignCenter().Text(user.Name)
                            .FontSize(16).SemiBold();
                        column.Item().AlignCenter().Text($"{startDate:MMM dd, yyyy} - {endDate:MMM dd, yyyy}")
                            .FontSize(12);
                    });

                page.Content()
                    .PaddingVertical(15)
                    .Table(table =>
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

                        foreach (var assignment in assignments)
                        {
                            table.Cell().BorderBottom(0.5f).Padding(5).Text(assignment.EventDate.ToString("MMM dd, yyyy"));
                            table.Cell().BorderBottom(0.5f).Padding(5).Text(assignment.DayOfWeek);
                            table.Cell().BorderBottom(0.5f).Padding(5).Text(assignment.TaskName);
                            table.Cell().BorderBottom(0.5f).Padding(5).Text(assignment.Status);
                        }
                    });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                });
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateTaskAssignmentReportAsync(DateTime startDate, DateTime endDate)
    {
        var taskAssignments = await _context.Assignments
            .Include(a => a.Task)
            .Include(a => a.User)
            .Where(a => a.EventDate >= startDate && a.EventDate <= endDate)
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
            .ToListAsync();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape()); // Landscape for wider table
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);

                page.Header().Column(column =>
                {
                    column.Item().AlignCenter().Text("Task Assignment Report")
                        .FontSize(20).Bold();
                    column.Item().AlignCenter().Text($"{startDate:MMM dd, yyyy} - {endDate:MMM dd, yyyy}")
                        .FontSize(12);
                });

                page.Content().PaddingVertical(15).Column(column =>
                {
                    foreach (var task in taskAssignments)
                    {
                        column.Item().PaddingBottom(10).Element(container =>
                        {
                            container.Column(taskColumn =>
                            {
                                taskColumn.Item().Background(Colors.Blue.Lighten4).Padding(5).Text(task.TaskName).SemiBold();
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
                                        header.Cell().Padding(5).Text("Date");
                                        header.Cell().Padding(5).Text("Member");
                                        header.Cell().Padding(5).Text("Status");
                                    });

                                    foreach (var assignment in task.Assignments)
                                    {
                                        table.Cell().Padding(5).Text(assignment.EventDate.ToString("MMM dd"));
                                        table.Cell().Padding(5).Text(assignment.MemberName);
                                        table.Cell().Padding(5).Text(assignment.Status);
                                    }
                                });
                            });
                        });
                    }
                });
            });
        });

        return document.GeneratePdf();
    }
}
```

#### 1.5 Register Service

In `backend/ChurchRoster.Api/Program.cs`:

```csharp
builder.Services.AddScoped<IReportService, ReportService>();
```

#### 1.6 Create Report Endpoints

Create `backend/ChurchRoster.Api/Endpoints/V1/ReportEndpoints.cs`:

```csharp
using ChurchRoster.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChurchRoster.Api.Endpoints.V1;

public static class ReportEndpoints
{
    public static void MapReportEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/reports").WithTags("Reports");

        group.MapGet("/monthly-roster", GetMonthlyRoster)
            .RequireAuthorization()
            .WithName("GetMonthlyRoster");

        group.MapGet("/member-schedule", GetMemberSchedule)
            .RequireAuthorization()
            .WithName("GetMemberSchedule");

        group.MapGet("/task-assignments", GetTaskAssignments)
            .RequireAuthorization()
            .WithName("GetTaskAssignments");
    }

    private static async Task<IResult> GetMonthlyRoster(
        [FromQuery] int year,
        [FromQuery] int month,
        IReportService reportService)
    {
        try
        {
            var pdfBytes = await reportService.GenerateMonthlyRosterAsync(year, month);
            return Results.File(pdfBytes, "application/pdf", $"Monthly_Roster_{year}_{month:D2}.pdf");
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to generate report: {ex.Message}");
        }
    }

    private static async Task<IResult> GetMemberSchedule(
        [FromQuery] int? userId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        HttpContext httpContext,
        IReportService reportService)
    {
        try
        {
            // If userId not provided, use current user
            var targetUserId = userId ?? int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var pdfBytes = await reportService.GenerateMemberScheduleAsync(targetUserId, startDate, endDate);
            return Results.File(pdfBytes, "application/pdf", $"Member_Schedule_{targetUserId}_{startDate:yyyyMMdd}.pdf");
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to generate report: {ex.Message}");
        }
    }

    private static async Task<IResult> GetTaskAssignments(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        IReportService reportService)
    {
        try
        {
            var pdfBytes = await reportService.GenerateTaskAssignmentReportAsync(startDate, endDate);
            return Results.File(pdfBytes, "application/pdf", $"Task_Assignments_{startDate:yyyyMMdd}.pdf");
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to generate report: {ex.Message}");
        }
    }
}
```

Register in `Program.cs`:

```csharp
app.MapReportEndpoints();
```

---

### **Part 2: Frontend Implementation**

#### 2.1 Create Report Service

Create `frontend/src/services/report.service.ts`:

```typescript
import apiService from './api.service';

class ReportService {
  async downloadMonthlyRoster(year: number, month: number): Promise<void> {
    try {
      const response = await fetch(
        `${import.meta.env.VITE_API_URL}/reports/monthly-roster?year=${year}&month=${month}`,
        {
          headers: {
            'Authorization': `Bearer ${localStorage.getItem('token')}`
          }
        }
      );

      if (!response.ok) throw new Error('Failed to download report');

      const blob = await response.blob();
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `Monthly_Roster_${year}_${month.toString().padStart(2, '0')}.pdf`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
    } catch (error) {
      console.error('Error downloading report:', error);
      throw error;
    }
  }

  async downloadMemberSchedule(userId: number | null, startDate: Date, endDate: Date): Promise<void> {
    try {
      const userParam = userId ? `&userId=${userId}` : '';
      const response = await fetch(
        `${import.meta.env.VITE_API_URL}/reports/member-schedule?startDate=${startDate.toISOString()}&endDate=${endDate.toISOString()}${userParam}`,
        {
          headers: {
            'Authorization': `Bearer ${localStorage.getItem('token')}`
          }
        }
      );

      if (!response.ok) throw new Error('Failed to download report');

      const blob = await response.blob();
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `Member_Schedule_${startDate.toISOString().split('T')[0]}.pdf`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
    } catch (error) {
      console.error('Error downloading report:', error);
      throw error;
    }
  }

  async downloadTaskAssignments(startDate: Date, endDate: Date): Promise<void> {
    try {
      const response = await fetch(
        `${import.meta.env.VITE_API_URL}/reports/task-assignments?startDate=${startDate.toISOString()}&endDate=${endDate.toISOString()}`,
        {
          headers: {
            'Authorization': `Bearer ${localStorage.getItem('token')}`
          }
        }
      );

      if (!response.ok) throw new Error('Failed to download report');

      const blob = await response.blob();
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `Task_Assignments_${startDate.toISOString().split('T')[0]}.pdf`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
    } catch (error) {
      console.error('Error downloading report:', error);
      throw error;
    }
  }
}

export default new ReportService();
```

#### 2.2 Create Reports Page

Create `frontend/src/pages/ReportsPage.tsx`:

```typescript
import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import reportService from '../services/report.service';
import { DocumentTextIcon, CalendarIcon, ClipboardDocumentListIcon, ArrowDownTrayIcon, ChevronLeftIcon } from '@heroicons/react/24/solid';

const ReportsPage: React.FC = () => {
  const navigate = useNavigate();
  const [loading, setLoading] = useState<string | null>(null);

  // Monthly Roster state
  const currentDate = new Date();
  const [rosterYear, setRosterYear] = useState(currentDate.getFullYear());
  const [rosterMonth, setRosterMonth] = useState(currentDate.getMonth() + 1);

  // Member Schedule state
  const [scheduleStartDate, setScheduleStartDate] = useState(new Date().toISOString().split('T')[0]);
  const [scheduleEndDate, setScheduleEndDate] = useState(
    new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0]
  );

  // Task Assignments state
  const [taskStartDate, setTaskStartDate] = useState(new Date().toISOString().split('T')[0]);
  const [taskEndDate, setTaskEndDate] = useState(
    new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0]
  );

  const handleDownloadMonthlyRoster = async () => {
    setLoading('monthly');
    try {
      await reportService.downloadMonthlyRoster(rosterYear, rosterMonth);
    } catch (error) {
      alert('Failed to download monthly roster');
    } finally {
      setLoading(null);
    }
  };

  const handleDownloadMemberSchedule = async () => {
    setLoading('member');
    try {
      await reportService.downloadMemberSchedule(
        null,
        new Date(scheduleStartDate),
        new Date(scheduleEndDate)
      );
    } catch (error) {
      alert('Failed to download member schedule');
    } finally {
      setLoading(null);
    }
  };

  const handleDownloadTaskAssignments = async () => {
    setLoading('task');
    try {
      await reportService.downloadTaskAssignments(
        new Date(taskStartDate),
        new Date(taskEndDate)
      );
    } catch (error) {
      alert('Failed to download task assignments');
    } finally {
      setLoading(null);
    }
  };

  const containerStyle: React.CSSProperties = {
    maxWidth: '1280px',
    margin: '0 auto',
    padding: '32px'
  };

  const headerStyle: React.CSSProperties = {
    display: 'flex',
    alignItems: 'center',
    gap: '16px',
    marginBottom: '32px'
  };

  const cardStyle: React.CSSProperties = {
    background: 'white',
    borderRadius: '8px',
    padding: '24px',
    boxShadow: '0 1px 3px rgba(0,0,0,0.1)',
    marginBottom: '24px'
  };

  const gridStyle: React.CSSProperties = {
    display: 'grid',
    gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))',
    gap: '24px'
  };

  return (
    <div style={containerStyle}>
      <div style={headerStyle}>
        <button
          onClick={() => navigate('/dashboard')}
          style={{
            background: 'white',
            border: '1px solid #d1d5db',
            borderRadius: '8px',
            padding: '8px 12px',
            cursor: 'pointer',
            display: 'flex',
            alignItems: 'center',
            gap: '8px'
          }}
        >
          <ChevronLeftIcon style={{ width: '20px', height: '20px' }} />
          Dashboard
        </button>
        <DocumentTextIcon style={{ width: '32px', height: '32px', color: '#7c3aed' }} />
        <h1 style={{ fontSize: '30px', fontWeight: 'bold', color: '#1f2937', margin: 0 }}>
          Reports
        </h1>
      </div>

      <div style={gridStyle}>
        {/* Monthly Roster Report */}
        <div style={cardStyle}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '12px', marginBottom: '16px' }}>
            <CalendarIcon style={{ width: '24px', height: '24px', color: '#3b82f6' }} />
            <h3 style={{ fontSize: '18px', fontWeight: '600', margin: 0 }}>Monthly Roster</h3>
          </div>
          <p style={{ color: '#6b7280', marginBottom: '16px' }}>
            Download complete roster for a specific month
          </p>
          <div style={{ marginBottom: '16px' }}>
            <label style={{ display: 'block', marginBottom: '8px', fontWeight: '500' }}>
              Select Month:
            </label>
            <div style={{ display: 'flex', gap: '8px' }}>
              <select
                value={rosterMonth}
                onChange={(e) => setRosterMonth(parseInt(e.target.value))}
                style={{
                  flex: 1,
                  padding: '8px',
                  border: '1px solid #d1d5db',
                  borderRadius: '6px'
                }}
              >
                {Array.from({ length: 12 }, (_, i) => (
                  <option key={i + 1} value={i + 1}>
                    {new Date(2000, i).toLocaleString('default', { month: 'long' })}
                  </option>
                ))}
              </select>
              <select
                value={rosterYear}
                onChange={(e) => setRosterYear(parseInt(e.target.value))}
                style={{
                  padding: '8px',
                  border: '1px solid #d1d5db',
                  borderRadius: '6px'
                }}
              >
                {Array.from({ length: 3 }, (_, i) => (
                  <option key={i} value={currentDate.getFullYear() + i}>
                    {currentDate.getFullYear() + i}
                  </option>
                ))}
              </select>
            </div>
          </div>
          <button
            onClick={handleDownloadMonthlyRoster}
            disabled={loading === 'monthly'}
            style={{
              width: '100%',
              background: '#3b82f6',
              color: 'white',
              border: 'none',
              borderRadius: '6px',
              padding: '10px',
              cursor: loading === 'monthly' ? 'not-allowed' : 'pointer',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              gap: '8px',
              opacity: loading === 'monthly' ? 0.7 : 1
            }}
          >
            <ArrowDownTrayIcon style={{ width: '20px', height: '20px' }} />
            {loading === 'monthly' ? 'Generating...' : 'Download PDF'}
          </button>
        </div>

        {/* Member Schedule Report */}
        <div style={cardStyle}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '12px', marginBottom: '16px' }}>
            <ClipboardDocumentListIcon style={{ width: '24px', height: '24px', color: '#10b981' }} />
            <h3 style={{ fontSize: '18px', fontWeight: '600', margin: 0 }}>My Schedule</h3>
          </div>
          <p style={{ color: '#6b7280', marginBottom: '16px' }}>
            Download your personal assignment schedule
          </p>
          <div style={{ marginBottom: '16px' }}>
            <label style={{ display: 'block', marginBottom: '8px', fontWeight: '500' }}>
              Date Range:
            </label>
            <div style={{ display: 'flex', flexDirection: 'column', gap: '8px' }}>
              <input
                type="date"
                value={scheduleStartDate}
                onChange={(e) => setScheduleStartDate(e.target.value)}
                style={{
                  padding: '8px',
                  border: '1px solid #d1d5db',
                  borderRadius: '6px'
                }}
              />
              <input
                type="date"
                value={scheduleEndDate}
                onChange={(e) => setScheduleEndDate(e.target.value)}
                style={{
                  padding: '8px',
                  border: '1px solid #d1d5db',
                  borderRadius: '6px'
                }}
              />
            </div>
          </div>
          <button
            onClick={handleDownloadMemberSchedule}
            disabled={loading === 'member'}
            style={{
              width: '100%',
              background: '#10b981',
              color: 'white',
              border: 'none',
              borderRadius: '6px',
              padding: '10px',
              cursor: loading === 'member' ? 'not-allowed' : 'pointer',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              gap: '8px',
              opacity: loading === 'member' ? 0.7 : 1
            }}
          >
            <ArrowDownTrayIcon style={{ width: '20px', height: '20px' }} />
            {loading === 'member' ? 'Generating...' : 'Download PDF'}
          </button>
        </div>

        {/* Task Assignments Report */}
        <div style={cardStyle}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '12px', marginBottom: '16px' }}>
            <DocumentTextIcon style={{ width: '24px', height: '24px', color: '#f59e0b' }} />
            <h3 style={{ fontSize: '18px', fontWeight: '600', margin: 0 }}>Task Assignments</h3>
          </div>
          <p style={{ color: '#6b7280', marginBottom: '16px' }}>
            View all task assignments by date range
          </p>
          <div style={{ marginBottom: '16px' }}>
            <label style={{ display: 'block', marginBottom: '8px', fontWeight: '500' }}>
              Date Range:
            </label>
            <div style={{ display: 'flex', flexDirection: 'column', gap: '8px' }}>
              <input
                type="date"
                value={taskStartDate}
                onChange={(e) => setTaskStartDate(e.target.value)}
                style={{
                  padding: '8px',
                  border: '1px solid #d1d5db',
                  borderRadius: '6px'
                }}
              />
              <input
                type="date"
                value={taskEndDate}
                onChange={(e) => setTaskEndDate(e.target.value)}
                style={{
                  padding: '8px',
                  border: '1px solid #d1d5db',
                  borderRadius: '6px'
                }}
              />
            </div>
          </div>
          <button
            onClick={handleDownloadTaskAssignments}
            disabled={loading === 'task'}
            style={{
              width: '100%',
              background: '#f59e0b',
              color: 'white',
              border: 'none',
              borderRadius: '6px',
              padding: '10px',
              cursor: loading === 'task' ? 'not-allowed' : 'pointer',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              gap: '8px',
              opacity: loading === 'task' ? 0.7 : 1
            }}
          >
            <ArrowDownTrayIcon style={{ width: '20px', height: '20px' }} />
            {loading === 'task' ? 'Generating...' : 'Download PDF'}
          </button>
        </div>
      </div>
    </div>
  );
};

export default ReportsPage;
```

#### 2.3 Add Route

In `frontend/src/App.tsx`:

```typescript
import ReportsPage from './pages/ReportsPage';

// Add route
<Route path="/reports" element={
  <ProtectedRoute>
    <ReportsPage />
  </ProtectedRoute>
} />
```

#### 2.4 Update Dashboard

Add Reports card to `Dashboard.tsx`:

```typescript
<div onClick={() => navigate('/reports')} style={cardStyle}>
  <DocumentTextIcon style={{ width: '48px', height: '48px', color: '#f59e0b' }} />
  <h3>Reports</h3>
  <p>Download PDF reports</p>
</div>
```

---

## 🧪 Testing

1. **Start backend:**
   ```bash
   dotnet run
   ```

2. **Start frontend:**
   ```bash
   npm run dev
   ```

3. **Test reports:**
   - Navigate to Reports page
   - Download monthly roster
   - Verify PDF opens correctly
   - Check formatting and data

---

## 🚀 Deployment

QuestPDF works seamlessly on Linux (Render) with no additional dependencies.

---

**Next Steps:** Week 5, Day 7 - Testing & Bug Fixes
