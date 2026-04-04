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
