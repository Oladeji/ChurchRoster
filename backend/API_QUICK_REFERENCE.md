# Church Roster API - Quick Reference

## Base URL
`https://localhost:7288` (Development)

## API Documentation
`https://localhost:7288/scalar/v1`

---

## 🔐 Authentication

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/login` | Login and get JWT token |
| POST | `/api/auth/register` | Register new user |

---

## 👥 Members (8 endpoints)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/members` | Get all members |
| GET | `/api/members/{id}` | Get member by ID |
| GET | `/api/members/role/{role}` | Get members by role |
| GET | `/api/members/active` | Get active members |
| POST | `/api/members` | Create new member |
| PUT | `/api/members/{id}` | Update member |
| DELETE | `/api/members/{id}` | Soft delete member |
| PUT | `/api/members/{id}/password` | Update password |

---

## 🎯 Skills (9 endpoints)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/skills` | Get all skills |
| GET | `/api/skills/{id}` | Get skill by ID |
| POST | `/api/skills` | Create new skill |
| PUT | `/api/skills/{id}` | Update skill |
| DELETE | `/api/skills/{id}` | Soft delete skill |
| POST | `/api/skills/assign` | Assign skill to user |
| DELETE | `/api/skills/assign/{userId}/{skillId}` | Remove skill from user |
| GET | `/api/skills/user/{userId}` | Get user's skills |
| GET | `/api/skills/{skillId}/users` | Get users with skill |

---

## 📋 Tasks (8 endpoints)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/tasks` | Get all tasks |
| GET | `/api/tasks/{id}` | Get task by ID |
| GET | `/api/tasks/frequency/{frequency}` | Get by frequency |
| GET | `/api/tasks/restricted` | Get restricted tasks |
| GET | `/api/tasks/active` | Get active tasks |
| POST | `/api/tasks` | Create new task |
| PUT | `/api/tasks/{id}` | Update task |
| DELETE | `/api/tasks/{id}` | Soft delete task |

---

## 📅 Assignments (9 endpoints)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/assignments` | Get all assignments |
| GET | `/api/assignments/{id}` | Get assignment by ID |
| GET | `/api/assignments/user/{userId}` | Get user's assignments |
| GET | `/api/assignments/date/{date}` | Get by date |
| GET | `/api/assignments/status/{status}` | Get by status |
| POST | `/api/assignments` | Create assignment |
| POST | `/api/assignments/validate` | Validate without creating |
| PUT | `/api/assignments/{id}/status` | Update status |
| DELETE | `/api/assignments/{id}` | Delete assignment |

---

## 📊 Business Rules

| Rule | Description | Can Override? |
|------|-------------|---------------|
| **Qualification** | Restricted tasks require skill | ✅ Yes |
| **Conflict** | No double-booking same day | ✅ Yes |
| **Monthly Limit** | Warns when limit exceeded | N/A (Warning only) |
| **Past Date** | Cannot assign to past | ❌ No |

---

## 🎨 Common Request Bodies

### Login
```json
{
  "email": "admin@church.com",
  "password": "Admin@123"
}
```

### Create Member
```json
{
  "name": "John Doe",
  "email": "john@church.com",
  "password": "SecurePass123",
  "phone": "+1234567890",
  "role": "Member",
  "monthlyLimit": 4
}
```

### Assign Skill
```json
{
  "userId": 2,
  "skillId": 1
}
```

### Create Assignment
```json
{
  "taskId": 1,
  "userId": 2,
  "eventDate": "2026-04-08",
  "isOverride": false
}
```

### Update Status
```json
{
  "status": "Accepted",
  "rejectionReason": null
}
```

---

## 📝 Valid Status Values

- `Pending` - Initial state
- `Accepted` - Member accepted
- `Rejected` - Member rejected
- `Confirmed` - Admin confirmed
- `Completed` - Task completed
- `Expired` - Past event date

---

## 🔑 Default Credentials

**Admin User**
- Email: `admin@church.com`
- Password: `Admin@123`
- Role: Admin

---

## 🗄️ Seeded Data

**Skills (7)**
1. CanLeadBibleStudy
2. CanLeadPreaching
3. CanLeadPrayer
4. CanMakeAnnouncements
5. CanLeadWorship
6. CanOperateSound
7. CanManageChildren

**Tasks (8)**
1. Lead Bible Study (Weekly, Tuesday, Restricted)
2. Lead Prayer Meeting (Weekly, Tuesday)
3. Lead Preaching (Weekly, Sunday, Restricted)
4. Lead Opening Prayer (Weekly, Sunday)
5. Lead Announcements (Weekly, Sunday)
6. Lead Closing Prayer (Weekly, Sunday)
7. Lead All-Night Prayer (Monthly, Last Friday)
8. Lead Vigil Prayer (Monthly, Last Saturday)

---

## 🚀 Quick Test Commands

```bash
# Start API
dotnet run --project backend/ChurchRoster.Api

# Build
dotnet build

# Run tests (when created)
dotnet test

# Create migration
cd backend/ChurchRoster.Infrastructure
dotnet ef migrations add MigrationName --startup-project ../ChurchRoster.Api

# Apply migration
dotnet ef database update --startup-project ../ChurchRoster.Api
```

---

## 📖 Documentation Files

- `WEEK2_COMPLETE.md` - Full feature documentation
- `API_TESTING_GUIDE.md` - Step-by-step testing
- `WEEK2_SUMMARY.md` - Implementation summary
- `AUTH_ENDPOINTS.md` - Authentication docs

---

*Quick Reference v1.0 - Week 2 Complete*
