# 📄 Church Ministry Rostering System  
## Complete Requirements Document (Web + Mobile App)  
**Version 6.0 – FINAL CORRECTION**  
*Copy this entire document and paste into Word, Google Docs, or save as .txt/.md*

---

## 🎯 1. PURPOSE
To provide a **Web Portal** for Administrators to plan yearly ministry schedules and a **Mobile App** for Members to view tasks, receive instant alerts, and accept/reject assignments. The system must enforce qualification rules, prevent conflicts, manage past event statuses, and provide printable reports for physical notice boards.

> ⚠️ **Correction Note (v6.0):**  
> - ✅ **Restricted Tasks (ONLY 2):**  
>   1. Tuesday → Lead Bible Study → Requires `CanLeadBibleStudy`  
>   2. Sunday → Lead Preaching → Requires `CanLeadPreaching`  
> - ✅ **All other tasks** (including Tuesday Prayer Meeting & Sunday Prayer) = Open to ANY member  
> *(Previous versions had prayer restrictions incorrect — this document is now fully corrected)*

---

## 👥 2. USER ROLES & PLATFORMS

| Role | Platform | Key Responsibilities |
|------|----------|----------------------|
| **Administrator** | **Web App** (Desktop/Tablet) | • Create yearly calendar<br>• Assign tasks to qualified members<br>• Track Accept/Reject status<br>• **Print Weekly/Monthly Reports** for notice boards<br>• Manage member skills & profiles |
| **Member** | **Mobile App** (iOS/Android) | • Receive push notifications<br>• View personal schedule (**Weekly/Monthly toggle**)<br>• **Accept or Reject** tasks<br>• Update availability |

---

## 📋 3. TASK CATALOG (Recurring Events) ✅ FINAL CORRECTED

### Weekly Tasks
| Day | Task Name | Restricted? | Required Skill (Phase 1) |
|-----|-----------|-------------|--------------------------|
| Tuesday | Lead Bible Study | ✅ **Yes** | `CanLeadBibleStudy` |
| Tuesday | Lead Prayer Meeting | ❌ **No** | *(None – Any member)* |
| Sunday | Lead Preaching | ✅ **Yes** | `CanLeadPreaching` |
| Sunday | Lead Prayer | ❌ **No** | *(None – Any member)* |
| Sunday | Lead Announcements | ❌ No | *(None)* |

### Monthly Tasks
| When | Task Name | Restricted? | Required Skill |
|------|-----------|-------------|----------------|
| Last Friday | Lead Prayer | ❌ No | *(None)* |
| Last Saturday | Lead Prayer | ❌ No | *(None)* |

> 🔹 **Restricted Tasks (ONLY 2):**  
> 1. Tuesday → Lead Bible Study → Requires `CanLeadBibleStudy`  
> 2. Sunday → Lead Preaching → Requires `CanLeadPreaching`  
>  
> 🔹 **All other tasks** can be assigned to any active member.

---

## 🛠️ 4. SKILLS MANAGEMENT (Configurable)

### 4.1 Phase 1 Skills (Required at Launch) ✅ FINAL CORRECTED
The system must include these two skills by default:

| Skill Tag | Required For |
|-----------|-------------|
| `CanLeadBibleStudy` | Tuesday → Lead Bible Study |
| `CanLeadPreaching` | Sunday → Lead Preaching |

### 4.2 Future-Proofing Requirement (Critical)
**The Skills Module must be dynamic/configurable.** Administrators must be able to add, edit, or remove skills without needing code changes.

| Feature | Requirement |
|---------|-------------|
| **Admin Control** | Admin can create new skill tags (e.g., `CanLeadWorship`, `CanServeCommunion`) via the Web Portal. |
| **Task Linking** | Admin can link any task to any skill tag (or leave it unlinked for open tasks). |
| **Member Tagging** | Admin can assign multiple skills to a single member profile. |
| **Filtering** | When assigning a task, the member dropdown must automatically filter based on the linked skill. |

### 4.3 Future Skills to Consider (Phase 2+)
*Do not build these now, but ensure the system can support them later:*
- `CanLeadPrayerMeeting` (If prayer leadership becomes restricted later)
- `CanLeadWorship` (For worship leaders)
- `AvailableTuesdayEvening` (For availability filtering)
- `BackgroundCheckCleared` (For safety compliance)

---

## 🔄 5. WORKFLOW: REQUEST & CONFIRM

1.  **Admin Assigns (Web):** Admin picks a date/task and selects a member.  
    → *System Status: **"Pending Acceptance"**.*
2.  **Member Notified (Mobile):** Member receives a **Push Notification** immediately.
3.  **Member Responds (Mobile):** Member opens app and taps **✅ Accept** or **❌ Reject**.
    - **If Accept:** Status changes to **"Confirmed"**. Task is locked.
    - **If Reject:** Status changes to **"Vacant"**. Admin is notified.
4.  **Admin Follow-up (Web):** Admin sees "Vacant" tasks on dashboard and reassigns to someone else.
5.  **Auto-Archive (System):** Once the event date passes, status updates automatically (see Business Rules).

---

## ⚙️ 6. BUSINESS RULES & LOGIC

| Rule ID | Description | Enforcement |
|---------|-------------|-------------|
| **BR-01** | **Qualification Check** | Restricted tasks only show members with the required skill tag:<br>• Tuesday Bible Study → `CanLeadBibleStudy`<br>• Sunday Preaching → `CanLeadPreaching` |
| **BR-02** | **No Double-Booking** | A member cannot have >1 task on the same calendar day. System blocks this. |
| **BR-03** | **Fairness Limit** | Admin sets a monthly limit (e.g., 4 tasks/member). System warns if exceeded (Admin can override). |
| **BR-04** | **Auto-Date Calculation** | System automatically identifies "Last Friday" and "Last Saturday" of every month. |
| **BR-05** | **Edit Rights** | Admin can edit, reassign, or delete any assignment at any time. |
| **BR-06** | **Holiday Logic** | No automatic holiday rules. Admin manually adjusts schedule for special dates. |
| **BR-07** | **Notification Trigger** | Push notification sends immediately upon assignment or status change. |
| **BR-08** | **Past Event Status** | **Automatic Update:** After the event date passes:<br>• If Accepted → Status becomes **"Completed"**<br>• If Pending/Vacant → Status becomes **"Expired"** |
| **BR-09** | **Printable Reports** | Web app must generate print-friendly PDFs for physical notice boards (Weekly/Monthly). |

---

## 📲 7. MOBILE APP FEATURES (Member)

| Feature | Description |
|---------|-------------|
| **Secure Login** | Email/Phone + Password (or Magic Link). |
| **Home Dashboard** | Shows next upcoming task + Pending Actions (Accept/Reject). |
| **Push Alerts** | Instant pop-up when a new task is assigned or schedule changes. |
| **Task Details** | Date, Time, Task Name, Location, Notes, Admin Contact. |
| **Action Buttons** | Large **✅ Accept** and **❌ Reject** buttons. |
| **Rejection Reason** | Optional text box when rejecting (e.g., "Out of town", "Sick"). |
| **Calendar View** | **Toggle Switch:** View tasks by **Week** or **Month**. |
| **Status Colors** | Visual indicators: 🟢 Confirmed, 🟡 Pending, 🔴 Rejected, ⚪ Completed/Expired. |
| **Profile Settings** | Update phone/email, mark "Unavailable" dates, manage notification preferences. |

---

## 🖥️ 8. WEB APP FEATURES (Admin)

| Feature | Description |
|---------|-------------|
| **Yearly Calendar** | View all tasks (Assigned, Pending, Vacant, Completed, Expired) in Month/Week view. |
| **Assignment Tool** | Click date → Select Task → Select Member → Send. |
| **Qualification Filter** | When assigning restricted tasks, dropdown shows ONLY qualified members:<br>• Bible Study → Shows `CanLeadBibleStudy` members<br>• Sunday Preaching → Shows `CanLeadPreaching` members |
| **Status Dashboard** | List view of all "Pending Acceptance" and "Vacant" tasks. |
| **Member Manager** | Add/Edit members, assign Skills, set Monthly Limits. |
| **Skill Manager** | Create/Edit/Delete skill tags (Configurable). |
| **Printable Reports** | **Generate PDF for Notice Boards:**<br>• Select Range: Weekly or Monthly<br>• Format: Clean, high-contrast, A4 friendly<br>• Content: Date, Task, Assigned Person (Only "Confirmed" shown by default) |
| **Reports** | Export digital PDF roster, view "Assignments per Member" report. |
| **Overrides** | Ability to bypass fairness warnings with a confirmation prompt. |

---

## 🗄️ 9. SIMPLE DATA MODEL (For Developers)

```plaintext
USERS
├─ user_id (PK)
├─ name
├─ email
├─ phone
├─ role (Admin/Member)
├─ monthly_limit (int, nullable)
├─ device_token (for push notifications)

SKILLS (Configurable Table)
├─ skill_id (PK)
├─ skill_name (e.g., "CanLeadBibleStudy", "CanLeadPreaching")
├─ description
├─ is_active (boolean)

USER_SKILLS
├─ user_id (FK → USERS)
├─ skill_id (FK → SKILLS)
├─ assigned_date

TASKS
├─ task_id (PK)
├─ task_name
├─ frequency (Weekly/Monthly)
├─ day_rule (e.g., "Tuesday", "Sunday", "Last Friday")
├─ required_skill_id (FK → SKILLS, nullable)  ← Links to skill if restricted
├─ is_restricted (boolean)

ASSIGNMENTS
├─ assignment_id (PK)
├─ task_id (FK → TASKS)
├─ user_id (FK → USERS)
├─ event_date
├─ status (Enum: Pending, Accepted, Rejected, Confirmed, Vacant, Completed, Expired)
├─ rejection_reason (text, nullable)
├─ is_override (boolean)
├─ assigned_by (FK → USERS)
├─ created_at, updated_at
```

---

## ✅ 10. MUST-HAVE CHECKLIST (Release 1.0)

### Web (Admin)
- [ ] Calendar shows full year with recurring task slots
- [ ] Admin can assign people (Status starts as "Pending")
- [ ] **Skills are configurable** (Admin can add new skills without code)
- [ ] **Restricted tasks filter correctly:**
  - [ ] Tuesday Bible Study → Only `CanLeadBibleStudy` members
  - [ ] Sunday Preaching → Only `CanLeadPreaching` members
- [ ] Tuesday Prayer Meeting & Sunday Prayer → Shows ALL members (not restricted)
- [ ] Dashboard shows "Pending" and "Vacant" tasks clearly
- [ ] Admin can reassign if member rejects
- [ ] **Printable Report:** Generate Weekly/Monthly PDF for notice board (A4 format)
- [ ] **Past Events:** Status auto-updates to "Completed" or "Expired" after date passes

### Mobile (Member)
- [ ] User can log in securely
- [ ] **Push Notification** arrives when task is assigned
- [ ] User can view task details
- [ ] User can tap **Accept** or **Reject**
- [ ] **Calendar View:** Toggle between Weekly and Monthly view
- [ ] User sees only their **Confirmed** tasks on their calendar

### System
- [ ] System blocks assigning same person twice on one day
- [ ] System auto-finds Last Friday/Saturday each month
- [ ] Push notifications work on iOS and Android
- [ ] Fairness limit warning works (with override option)
- [ ] Daily job runs to update past event statuses

---

## ❓ 11. FREQUENT QUESTIONS

**Q: Which tasks require special qualification?**  
A: Only TWO tasks are restricted:  
1️⃣ Tuesday → Lead Bible Study → Requires `CanLeadBibleStudy`  
2️⃣ Sunday → Lead Preaching → Requires `CanLeadPreaching`  
*All other tasks (including Tuesday Prayer Meeting, Sunday Prayer, Announcements, and Monthly Prayer) can be done by any member.*

**Q: What happens if someone rejects a task?**  
A: The task status becomes "Vacant". The Admin gets a notification and must assign someone else.

**Q: What happens to tasks after the date passes?**  
A: The system automatically updates them. If someone accepted, it marks as **"Completed"**. If no one accepted, it marks as **"Expired"**. This keeps the active calendar clean.

**Q: Can we print the schedule for the church notice board?**  
A: Yes. The Admin Web App has a "Print Report" feature. You can select "Monthly" or "Weekly", generate a PDF, and print it on A4 paper to paste on the board.

**Q: Can members see their schedule by week?**  
A: Yes. The Mobile App has a toggle switch to view tasks by **Week** or **Month**.

**Q: Can we add more skills later?**  
A: Yes. The system is built to allow Admins to create new skill tags (e.g., `CanLeadWorship`) without calling the developer.

---

## 🚀 12. NEXT STEPS FOR YOU

1.  ✅ **Review** this corrected document (v6.0) and confirm the qualification rules now match your church policy.
2.  ✅ **List Members** and mark who has:
    - `CanLeadBibleStudy` (for Tuesday Bible Study)
    - `CanLeadPreaching` (for Sunday Preaching)
3.  ✅ **Note** which members have smartphones (for Mobile App) vs. need manual calling.
4.  ✅ **Share with Developer**: Ask for a **Web Admin Panel** + **Mobile App (iOS/Android)**.
5.  ✅ **Budget Note**: Building a native mobile app costs more. Ask your developer if a **PWA (Progressive Web App)** can meet the push notification needs to save cost.

---

## 📥 13. HOW TO SAVE THIS DOCUMENT

**Option 1: Google Docs / Microsoft Word**
1.  Select all text above (Ctrl+A or Cmd+A)
2.  Copy (Ctrl+C or Cmd+C)
3.  Open Google Docs or Word
4.  Paste (Ctrl+V or Cmd+V)
5.  File → Download → PDF or .docx

**Option 2: Save as Text File**
1.  Select all text above
2.  Copy
3.  Open Notepad (Windows) or TextEdit (Mac)
4.  Paste
5.  File → Save As → `Church_Rostering_Requirements_v6_FINAL.txt`

**Option 3: Save as Markdown (for tech teams)**
1.  Copy all text
2.  Paste into a new file named `requirements_v6.md`
3.  Open with any code editor or Markdown viewer

---

> 🙏 **You're all set!**  
> This version (v6.0 – FINAL CORRECTION) now accurately reflects:  
> ✅ **Tuesday Bible Study** = Restricted (`CanLeadBibleStudy`)  
> ✅ **Sunday Preaching** = Restricted (`CanLeadPreaching`)  
> ✅ **All other tasks** = Open to ANY member  
> ✅ Plus: Mobile App, Accept/Reject Workflow, Configurable Skills, Past Event Automation, Printable Reports, and Weekly/Monthly Views.  
>  
