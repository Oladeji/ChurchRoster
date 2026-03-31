# API Testing Guide - Complete Workflow

## Quick Start
1. Start API: `dotnet run --project backend/ChurchRoster.Api`
2. Open Scalar UI: `https://localhost:7288/scalar/v1`
3. Follow this testing sequence:

---

## Test Sequence (Copy & Paste into Scalar)

### 1. Authentication ✅ (Already Working)
```json
POST /api/auth/login
{
  "email": "admin@church.com",
  "password": "Admin@123"
}
```
**Copy the token from response for later use**

---

### 2. Create New Members

**Member 1 - John (No Skills)**
```json
POST /api/members
{
  "name": "John Doe",
  "email": "john.doe@church.com",
  "password": "SecurePass123",
  "phone": "+1234567890",
  "role": "Member",
  "monthlyLimit": 4
}
```

**Member 2 - Jane (Will add skills later)**
```json
POST /api/members
{
  "name": "Jane Smith",
  "email": "jane.smith@church.com",
  "password": "SecurePass123",
  "phone": "+9876543210",
  "role": "Member",
  "monthlyLimit": 3
}
```

**Member 3 - Pastor Mike**
```json
POST /api/members
{
  "name": "Pastor Mike",
  "email": "pastor.mike@church.com",
  "password": "SecurePass123",
  "phone": "+5555555555",
  "role": "Member",
  "monthlyLimit": 6
}
```

---

### 3. Get All Members
```json
GET /api/members
```
**Note the UserId for each member (likely 2, 3, 4)**

---

### 4. Get All Skills
```json
GET /api/skills
```
**You should see 7 skills already seeded**

---

### 5. Assign Skills to Members

**Give Jane "CanLeadBibleStudy" (SkillId: 1)**
```json
POST /api/skills/assign
{
  "userId": 3,
  "skillId": 1
}
```

**Give Pastor Mike "CanLeadPreaching" (SkillId: 2)**
```json
POST /api/skills/assign
{
  "userId": 4,
  "skillId": 2
}
```

**Give Pastor Mike "CanLeadPrayer" (SkillId: 3)**
```json
POST /api/skills/assign
{
  "userId": 4,
  "skillId": 3
}
```

---

### 6. View Member Skills

**Get Jane's Skills**
```json
GET /api/skills/user/3
```

**Get Pastor Mike's Skills**
```json
GET /api/skills/user/4
```

---

### 7. Get All Tasks
```json
GET /api/tasks
```
**You should see 8 tasks already seeded**

---

### 8. Test Assignment Validation

**❌ Test 1: Assign Restricted Task to Unqualified User (SHOULD FAIL)**
```json
POST /api/assignments/validate
{
  "taskId": 1,
  "userId": 2,
  "eventDate": "2026-04-08",
  "isOverride": false
}
```
**Expected: Error - "User does not have the required skill: CanLeadBibleStudy"**

---

**✅ Test 2: Assign Restricted Task to Qualified User (SHOULD PASS)**
```json
POST /api/assignments/validate
{
  "taskId": 1,
  "userId": 3,
  "eventDate": "2026-04-08",
  "isOverride": false
}
```
**Expected: IsValid = true, no errors**

---

**✅ Test 3: Assign Non-Restricted Task to Any User (SHOULD PASS)**
```json
POST /api/assignments/validate
{
  "taskId": 2,
  "userId": 2,
  "eventDate": "2026-04-08",
  "isOverride": false
}
```
**Expected: IsValid = true, no errors**

---

### 9. Create Valid Assignments

**Assignment 1: Jane leads Bible Study on April 8**
```json
POST /api/assignments
{
  "taskId": 1,
  "userId": 3,
  "eventDate": "2026-04-08",
  "isOverride": false
}
```

**Assignment 2: Pastor Mike leads Preaching on April 6**
```json
POST /api/assignments
{
  "taskId": 3,
  "userId": 4,
  "eventDate": "2026-04-06",
  "isOverride": false
}
```

**Assignment 3: John leads Announcements on April 6**
```json
POST /api/assignments
{
  "taskId": 5,
  "userId": 2,
  "eventDate": "2026-04-06",
  "isOverride": false
}
```

---

### 10. Test Conflict Detection

**❌ Try to assign Jane to another task on April 8 (SHOULD FAIL)**
```json
POST /api/assignments
{
  "taskId": 2,
  "userId": 3,
  "eventDate": "2026-04-08",
  "isOverride": false
}
```
**Expected: Error - "User already has an assignment on 2026-04-08"**

---

**✅ Override the conflict**
```json
POST /api/assignments
{
  "taskId": 2,
  "userId": 3,
  "eventDate": "2026-04-08",
  "isOverride": true
}
```
**Expected: Success (IsOverride = true)**

---

### 11. Test Monthly Limit Warning

**Create 3 more assignments for Jane in April**
```json
POST /api/assignments
{
  "taskId": 2,
  "userId": 3,
  "eventDate": "2026-04-15",
  "isOverride": false
}
```

```json
POST /api/assignments
{
  "taskId": 2,
  "userId": 3,
  "eventDate": "2026-04-22",
  "isOverride": false
}
```

**Now Jane has 4 assignments (2 on Apr 8, 1 on Apr 15, 1 on Apr 22)**

**Accept them all:**
```json
PUT /api/assignments/1/status
{
  "status": "Accepted",
  "rejectionReason": null
}
```
Repeat for assignment IDs 2, 3, 4

**Try to assign 5th task (exceeds limit of 3)**
```json
POST /api/assignments
{
  "taskId": 2,
  "userId": 3,
  "eventDate": "2026-04-29",
  "isOverride": false
}
```
**Expected: Success WITH WARNING - "User has reached their monthly limit of 3 assignments"**

---

### 12. Test Past Date Validation

**❌ Try to assign task to past date (SHOULD FAIL)**
```json
POST /api/assignments
{
  "taskId": 2,
  "userId": 2,
  "eventDate": "2026-03-01",
  "isOverride": false
}
```
**Expected: Error - "Cannot assign tasks to past dates"**

**❌ Even override won't work**
```json
POST /api/assignments
{
  "taskId": 2,
  "userId": 2,
  "eventDate": "2026-03-01",
  "isOverride": true
}
```
**Expected: Error - "Cannot assign tasks to past dates"**

---

### 13. View Assignments

**Get all assignments**
```json
GET /api/assignments
```

**Get Jane's assignments**
```json
GET /api/assignments/user/3
```

**Get assignments for April 6, 2026**
```json
GET /api/assignments/date/2026-04-06
```

**Get pending assignments**
```json
GET /api/assignments/status/Pending
```

**Get accepted assignments**
```json
GET /api/assignments/status/Accepted
```

---

### 14. Update Assignment Status

**Member accepts assignment**
```json
PUT /api/assignments/1/status
{
  "status": "Accepted",
  "rejectionReason": null
}
```

**Member rejects assignment**
```json
PUT /api/assignments/2/status
{
  "status": "Rejected",
  "rejectionReason": "Family emergency - unable to attend"
}
```

**Admin confirms assignment**
```json
PUT /api/assignments/1/status
{
  "status": "Confirmed",
  "rejectionReason": null
}
```

---

### 15. Test Member Updates

**Update John's monthly limit**
```json
PUT /api/members/2
{
  "name": "John Doe",
  "phone": "+1234567890",
  "role": "Member",
  "monthlyLimit": 5,
  "isActive": true
}
```

**Deactivate a member**
```json
DELETE /api/members/2
```
**This soft deletes (IsActive = false)**

**Verify John is inactive**
```json
GET /api/members/active
```
**John should NOT appear in the list**

---

### 16. Test Skill Management

**Create a new skill**
```json
POST /api/skills
{
  "skillName": "CanPlayInstrument",
  "description": "Can play musical instruments"
}
```

**Update a skill**
```json
PUT /api/skills/8
{
  "skillName": "CanPlayInstrument",
  "description": "Can play musical instruments during worship",
  "isActive": true
}
```

**Get users with a specific skill**
```json
GET /api/skills/1/users
```

---

### 17. Test Task Management

**Create a new task**
```json
POST /api/tasks
{
  "taskName": "Lead Youth Service",
  "frequency": "Weekly",
  "dayRule": "Friday",
  "requiredSkillId": null,
  "isRestricted": false
}
```

**Get weekly tasks**
```json
GET /api/tasks/frequency/Weekly
```

**Get monthly tasks**
```json
GET /api/tasks/frequency/Monthly
```

**Get restricted tasks only**
```json
GET /api/tasks/restricted
```

---

## Expected Results Summary

| Test | Expected Result |
|------|----------------|
| Login with admin@church.com | ✅ Success, returns JWT token |
| Create 3 members | ✅ Success, returns member details |
| Assign skills to members | ✅ Success, users gain skills |
| Assign restricted task to unqualified user | ❌ Error: Missing required skill |
| Assign restricted task to qualified user | ✅ Success |
| Assign 2nd task same day | ❌ Error: Conflict detected |
| Override conflict | ✅ Success with IsOverride |
| Exceed monthly limit | ✅ Success WITH WARNING |
| Assign to past date | ❌ Error (even with override) |
| Member accept/reject assignment | ✅ Status updated |
| Soft delete member | ✅ Member deactivated |

---

## Quick Verification Checklist

After running all tests, verify:
- [ ] Can create members with password validation
- [ ] Can assign skills to members
- [ ] Restricted tasks block unqualified users
- [ ] Cannot assign same user twice on same day (unless override)
- [ ] Monthly limit shows warning (but doesn't block)
- [ ] Cannot assign to past dates
- [ ] Members can accept/reject assignments
- [ ] Soft delete works (member becomes inactive)
- [ ] All GET endpoints return data
- [ ] Swagger/Scalar UI documents all 34 endpoints

---

## Troubleshooting

**If you get 500 errors:**
- Check connection string in `appsettings.Development.json`
- Ensure database is running (Supabase)
- Check Output window for detailed error

**If validation doesn't work:**
- Verify user IDs and skill IDs match your data
- Check task `RequiredSkillId` and `IsRestricted` values
- Use `/api/assignments/validate` to debug

**If assignments fail:**
- Ensure event date is in the future
- Check user has required skills for restricted tasks
- Verify no existing assignment on same date

---

*Happy Testing! 🎉*
