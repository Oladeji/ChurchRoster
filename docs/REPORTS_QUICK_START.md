# Quick Start: PDF Reports

## Generate Reports via Frontend

### 1. Monthly Roster
1. Login as Admin
2. Navigate to **Reports** page
3. Select **Year** and **Month** from dropdowns
4. Click **"Generate PDF"**
5. PDF downloads automatically: `Monthly_Roster_YYYY_MM.pdf`

**Use Case:** Print and distribute monthly schedules to all members

---

### 2. Member Schedule
1. Navigate to **Reports** page
2. **(Optional)** Enter User ID (leave empty for your own schedule)
3. Select **Start Date** and **End Date**
4. Click **"Generate PDF"**
5. PDF downloads automatically: `Member_Schedule_YYYY-MM-DD.pdf`

**Use Case:** Give members their personal assignment calendars

---

### 3. Task Assignments
1. Navigate to **Reports** page
2. Select **Start Date** and **End Date**
3. Click **"Generate PDF"**
4. PDF downloads automatically: `Task_Assignments_YYYY-MM-DD.pdf`

**Use Case:** See which members are assigned to each task type

---

## Generate Reports via API (cURL)

### Monthly Roster
```bash
curl -X GET "https://localhost:7288/api/reports/monthly-roster?year=2025&month=4" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  --output roster.pdf
```

### Member Schedule (Current User)
```bash
curl -X GET "https://localhost:7288/api/reports/member-schedule?startDate=2025-04-01&endDate=2025-04-30" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  --output schedule.pdf
```

### Member Schedule (Specific User)
```bash
curl -X GET "https://localhost:7288/api/reports/member-schedule?userId=5&startDate=2025-04-01&endDate=2025-04-30" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  --output schedule.pdf
```

### Task Assignments
```bash
curl -X GET "https://localhost:7288/api/reports/task-assignments?startDate=2025-04-01&endDate=2025-05-31" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  --output tasks.pdf
```

---

## Common Issues

### PDF Not Downloading?
- ✅ Check browser's popup blocker
- ✅ Verify you're logged in as Admin
- ✅ Check browser console for errors

### Empty PDF?
- ✅ Verify assignments exist for selected date range
- ✅ Check database has data

### Backend Error?
- ✅ Ensure backend is running: `cd backend/ChurchRoster.Api && dotnet run`
- ✅ Check backend console for errors

---

## Quick Test

**Test Monthly Report:**
1. Login as Admin (admin@church.com / Admin@123)
2. Go to `/reports`
3. Keep default year/month
4. Click "Generate PDF" for Monthly Roster
5. Should download immediately

✅ **That's it!** You now have printable rosters.
