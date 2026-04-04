# PDF Reports Feature - Week 5

## Overview
The Church Roster System includes comprehensive PDF report generation capabilities using QuestPDF library. Reports can be generated and downloaded directly from the web interface.

## Available Reports

### 1. Monthly Roster Report
**Endpoint:** `GET /api/reports/monthly-roster`
**Query Parameters:**
- `year` (required): Year (e.g., 2025)
- `month` (required): Month number (1-12)

**Description:**
Generates a comprehensive PDF showing all assignments for a specific month, organized by week. Each week section displays assignments in a table format with:
- Date and day of week
- Task name
- Assigned member
- Status (with color coding)

**Features:**
- Weekly organization
- Status summary (Pending, Accepted, Rejected, Confirmed, Completed)
- Total assignments count
- Professional formatting with headers and page numbers

---

### 2. Member Schedule Report
**Endpoint:** `GET /api/reports/member-schedule`
**Query Parameters:**
- `userId` (optional): User ID to generate report for (defaults to current logged-in user)
- `startDate` (optional): Start date (defaults to today)
- `endDate` (optional): End date (defaults to 1 month from start date)

**Description:**
Generates a personalized PDF schedule for a specific member showing all their assignments within the date range.

**Features:**
- Personal schedule view
- Assignments sorted by date
- Task details with status
- Summary statistics
- Member name prominently displayed

---

### 3. Task Assignment Report
**Endpoint:** `GET /api/reports/task-assignments`
**Query Parameters:**
- `startDate` (optional): Start date (defaults to today)
- `endDate` (optional): End date (defaults to 1 month from start date)

**Description:**
Generates a PDF report showing all assignments grouped by task type for a specific date range. Useful for understanding task distribution.

**Features:**
- Grouped by task name
- Shows all members assigned to each task
- Date-sorted assignments
- Landscape orientation for better readability

---

## Implementation Details

### Backend

**Technology:** QuestPDF v2024.12.3
**License:** Community (for non-commercial use)

**Service:** `ReportService`
**Location:** `ChurchRoster.Application/Services/ReportService.cs`

**Key Components:**
- PDF document generation with professional styling
- Color-coded status badges
- Responsive table layouts
- Page headers and footers
- Summary statistics

### Frontend

**Page:** `ReportsPage`
**Location:** `frontend/src/pages/ReportsPage.tsx`
**Route:** `/reports` (Admin only)

**Features:**
- Three separate report generation forms
- Date pickers for date range selection
- Month/Year selectors for monthly reports
- Loading states during PDF generation
- Automatic PDF download upon generation
- Informative descriptions for each report type

## Usage

### For Users (Frontend)

1. Navigate to **Reports** page (Admin access required)
2. Select the desired report type
3. Fill in the required parameters:
   - **Monthly Roster:** Select year and month
   - **Member Schedule:** Optionally enter user ID, select date range
   - **Task Assignments:** Select date range
4. Click "Generate PDF"
5. PDF will be automatically downloaded to your browser's download folder

### For Developers (API)

**Example: Generate Monthly Roster**
```bash
curl -X GET "https://localhost:7288/api/reports/monthly-roster?year=2025&month=4" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  --output monthly_roster.pdf
```

**Example: Generate Member Schedule (Current User)**
```bash
curl -X GET "https://localhost:7288/api/reports/member-schedule?startDate=2025-04-01&endDate=2025-04-30" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  --output my_schedule.pdf
```

**Example: Generate Task Assignments**
```bash
curl -X GET "https://localhost:7288/api/reports/task-assignments?startDate=2025-04-01&endDate=2025-05-31" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  --output task_assignments.pdf
```

## PDF Features

### Styling
- **Page Size:** A4 (Monthly/Member Schedule), A4 Landscape (Task Assignments)
- **Margins:** 2cm all around
- **Font Size:** 10pt body, larger for headers
- **Colors:** Blue for headers, color-coded status badges

### Status Badge Colors
- ✓ **Accepted:** Light green background
- ✓✓ **Confirmed:** Light blue background
- ⏳ **Pending:** Light yellow background
- ✗ **Rejected:** Light red background
- ✓ **Completed:** Light gray background
- ⌛ **Expired:** (if applicable)

### Headers Include
- Report title
- Date range or month/year
- Generation timestamp
- Page numbers

## Security

- All report endpoints require authentication
- JWT token must be provided in Authorization header
- Member Schedule endpoint:
  - Without `userId` parameter: Returns current user's schedule
  - With `userId` parameter: Requires admin privileges (enforced by endpoint protection)

## Database Queries

Reports use optimized queries with:
- `.Include()` for navigation properties (Task, User, etc.)
- Efficient filtering by date ranges
- Proper indexing on EventDate column
- Lazy loading disabled for better performance

## Error Handling

### Backend
- Validates year (2020-2100) and month (1-12) inputs
- Returns `400 Bad Request` for invalid parameters
- Returns `404 Not Found` if user doesn't exist
- Returns `500 Internal Server Error` with problem details on exceptions

### Frontend
- Shows loading state during generation
- Displays alert on error
- Disables buttons during PDF generation
- Handles download blob creation and cleanup

## Future Enhancements

Potential improvements for future versions:
1. Custom date range for monthly roster (multiple months)
2. Filter by assignment status
3. Export to Excel/CSV
4. Email reports directly to members
5. Scheduled report generation
6. Custom report templates
7. Charts and visualizations
8. Summary statistics dashboard

## Testing

### Manual Testing Steps

1. **Monthly Roster:**
   - Select current month and year
   - Click "Generate PDF"
   - Verify PDF downloads
   - Open PDF and check:
     - Correct month/year in title
     - All assignments present
     - Proper week grouping
     - Status colors correct

2. **Member Schedule:**
   - Leave User ID empty (test current user)
   - Select 30-day date range
   - Generate and verify personal schedule
   - Test with specific User ID (admin only)

3. **Task Assignments:**
   - Select 30-day date range
   - Generate and verify task grouping
   - Check landscape orientation

### Test Cases

```typescript
describe('Reports API', () => {
  test('GET /api/reports/monthly-roster returns PDF', async () => {
    const response = await fetch(
      'https://localhost:7288/api/reports/monthly-roster?year=2025&month=4',
      { headers: { Authorization: `Bearer ${token}` } }
    );
    expect(response.status).toBe(200);
    expect(response.headers.get('content-type')).toBe('application/pdf');
  });

  test('Returns 400 for invalid month', async () => {
    const response = await fetch(
      'https://localhost:7288/api/reports/monthly-roster?year=2025&month=13',
      { headers: { Authorization: `Bearer ${token}` } }
    );
    expect(response.status).toBe(400);
  });
});
```

## Performance Considerations

- **Database Query Optimization:** Use `.AsNoTracking()` for read-only queries
- **Pagination:** Large reports (>100 assignments) automatically paginated
- **Caching:** Consider implementing report caching for frequently accessed periods
- **Background Processing:** For very large reports, consider queuing mechanism

## Troubleshooting

### PDF Not Downloading
1. Check browser's popup blocker settings
2. Verify JWT token is valid and not expired
3. Check browser console for errors
4. Ensure backend service is running

### Empty PDF or Missing Data
1. Verify date range contains assignments
2. Check database for assignments in selected period
3. Review backend logs for query errors

### PDF Formatting Issues
1. Update QuestPDF package to latest version
2. Check for null reference exceptions in report service
3. Verify all navigation properties are properly included

## File Structure

```
backend/
  ChurchRoster.Application/
    Services/
      ReportService.cs              # Main report generation logic
      PdfReportService.cs           # Alternative PDF service
    Interfaces/
      IReportService.cs             # Report service interface
    DTOs/Reports/
      MonthlyRosterDto.cs          # Monthly report DTOs
      RosterReportData.cs          # Roster report DTOs
      RosterReportRequest.cs       # Report request DTOs

  ChurchRoster.Api/
    Endpoints/V1/
      ReportEndpoints.cs            # API endpoints for reports

frontend/
  src/
    pages/
      ReportsPage.tsx               # Reports UI page
```

## Dependencies

### Backend
```xml
<PackageReference Include="QuestPDF" Version="2024.12.3" />
```

### Frontend
```json
{
  "@heroicons/react": "^2.0.0"  // For icons
}
```

## License Compliance

**QuestPDF Community License:**
- ✅ Free for non-commercial use
- ✅ Free for businesses with revenue < $1M USD
- ⚠️ Commercial license required for larger businesses

Set license in code:
```csharp
QuestPDF.Settings.License = LicenseType.Community;
```

---

**Created:** Week 5
**Last Updated:** Week 5
**Status:** ✅ Completed and Tested
