# 📄 Church Ministry Rostering System  
## Complete Requirements Document (Web + Mobile App)  
**Version 7.0** — *Updated April 2026: Added AI-Powered Roster Proposal Module (Addendum, Sections 14–17)*  
*Previous: v6.0 – FINAL CORRECTION*  
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

> 🙏 **v6.0 summary:**  
> ✅ **Tuesday Bible Study** = Restricted (`CanLeadBibleStudy`)  
> ✅ **Sunday Preaching** = Restricted (`CanLeadPreaching`)  
> ✅ **All other tasks** = Open to ANY member  
> ✅ Plus: Mobile App, Accept/Reject Workflow, Configurable Skills, Past Event Automation, Printable Reports, and Weekly/Monthly Views.  

---

## 🤖 14. AI-POWERED ROSTER PROPOSAL MODULE *(Addendum v7.0)*

> **Added:** April 2026  
> **Scope:** Admin-only feature. Members are not affected by this module.  
> **Cost:** Zero — uses **GitHub Models** free tier via a GitHub PAT token.  
> **Generation:** Fully **asynchronous** — the UI never blocks while the AI works.

### 14.1 Purpose

Allow Administrators to auto-generate a full draft roster for any custom date range using an AI agent. The AI agent reads the existing task catalog, member skills, and fairness constraints, then proposes a complete schedule as a **Draft**. The Admin reviews, edits, and — when satisfied — publishes the draft to the live calendar.

Publishing does **not** bypass the existing workflow. Each published assignment starts as **Pending** and members still Accept/Reject as normal.

### 14.2 Updated User Roles

| Role | New Capability |
|------|----------------|
| **Administrator** | Generate AI draft proposals, preview/edit them, publish to live calendar, print draft PDF, view proposal history |
| **Member** | No change — members only see the result after Admin publishes (Pending assignments + push notifications) |

### 14.3 New User Stories

| ID | Role | Story | So that... |
|----|------|-------|------------|
| US-PROP-01 | Admin | Generate a roster proposal for a custom date range using AI | I save hours of manual scheduling |
| US-PROP-02 | Admin | See a "Generating roster…" spinner while the AI works | I know the system is busy and don't click twice |
| US-PROP-03 | Admin | Preview and edit any item in a draft (add / edit / delete) | I can correct AI errors before publishing |
| US-PROP-04 | Admin | Publish a draft to the live calendar | Real assignments are created and members are notified |
| US-PROP-05 | Admin | Download a PDF of the draft with a DRAFT watermark | I can discuss it with leadership before finalising |
| US-PROP-06 | Admin | View the full history of all proposals (Draft, Published, Archived) | I have an audit trail |
| US-PROP-07 | System | Skip conflicts during publish and log them | The whole batch never fails because of one conflict |
| US-PROP-08 | System | Enforce tenant isolation on all proposal data | Each church only sees its own proposals |

### 14.4 How the AI Agent Works

The agent uses **Microsoft AI Agent Framework (`Microsoft.Extensions.AI`)** connected to the **GitHub Models API** (`https://models.inference.ai.azure.com`). The model name and access token are read from `appsettings` — never hardcoded.

**Agent pattern:**
1. A **system prompt** gives the agent all scheduling rules (skill requirements, fairness limits, day rules, monthly task rules)
2. The agent calls **C# tool functions** (real database calls) to fetch tasks, members, and existing assignments
3. The agent calls a **write tool function** to create each proposal item directly in the database
4. When the agent stops calling tools, generation is complete

**Agent tool functions:**

| # | Tool | What it does |
|---|------|-------------|
| 1 | `GetRecurringTasksAsync` | Fetches all tasks with their day rules and skill requirements |
| 2 | `GetQualifiedMembersAsync` | Returns members who have the skill required for a given task |
| 3 | `GetMemberAssignmentCountAsync` | Returns how many tasks a member already has in a given month |
| 4 | `GetExistingAssignmentsAsync` | Returns live assignments in the date range (for conflict awareness) |
| 5 | `CreateProposalItemAsync` | Writes a proposed slot into the draft in the database |
| 6 | `LogSkippedSlotAsync` | Records why the agent could not fill a slot (audit trail) |

### 14.5 Async Generation Flow

```
Admin clicks "Generate"
        │
        ▼
API creates RosterProposal (Status = Processing)
Returns HTTP 202 + { proposalId } immediately
        │
        ▼
Background job dequeues proposalId
Runs AI agent loop (may take 10–60 seconds)
        │
        ▼
On completion → Status = Draft
        │
        ▼
Frontend polls GET /proposals/{id} every 3 s
Spinner disappears → Edit view appears
```

> **Concurrency rule:** Only **one** proposal per church (tenant) may be in `Processing` status at a time. A second generate attempt returns `409 Conflict`.

### 14.6 Proposal Lifecycle

```
[Processing] → [Draft] → [Published]
                  ↓
             [Archived]
```

| Status | Meaning |
|--------|---------|
| `Processing` | AI agent is running — read-only, no edits allowed |
| `Draft` | Generation complete — Admin can add / edit / delete items |
| `Published` | Pushed to live calendar — permanent history, no edits |
| `Archived` | Manually archived by Admin — hidden from active view |

### 14.7 Publish Rules

When an Admin publishes a Draft:

- For **each item** in the proposal:
  - If a live `Assignment` already exists for the **same church + same task + same date** → item is marked `Skipped`, a skip log is written, processing **continues** (the whole publish never fails for one conflict)
  - Otherwise → a new `Assignment` is created with `Status = Pending`, push/email notification sent to the member
- The proposal's status is set to `Published`
- Published proposals remain visible forever as history

### 14.8 New Business Rules

| Rule ID | Description |
|---------|-------------|
| **BR-10** | AI proposal generation is asynchronous — API returns 202 immediately, result polled by frontend |
| **BR-11** | Model name and GitHub token are read from `appsettings` — never hardcoded in source code |
| **BR-12** | Only one `Processing` proposal allowed per church at a time (409 on duplicate) |
| **BR-13** | Published proposals are permanent history — never deleted |
| **BR-14** | Publish skips (not fails) items that conflict with existing live assignments |
| **BR-15** | Every skipped item during publish is recorded in a skip log for Admin review |
| **BR-16** | All proposal data is tenant-isolated via EF Core Global Query Filter |
| **BR-17** | Draft editing (add/edit/delete items) is only permitted when Status = `Draft` |

### 14.9 New Data Entities

```plaintext
ROSTER_PROPOSALS
├─ proposal_id        (PK, UUID)
├─ tenant_id          (FK → TENANTS)
├─ name               (e.g. "May 2026 Draft")
├─ status             (Processing | Draft | Published | Archived)
├─ date_range_start   (DATE)
├─ date_range_end     (DATE)
├─ generated_at       (TIMESTAMP)
├─ published_at       (TIMESTAMP, nullable)
├─ created_by_user_id (FK → USERS)

ROSTER_PROPOSAL_ITEMS
├─ item_id      (PK, UUID)
├─ proposal_id  (FK → ROSTER_PROPOSALS, cascade delete)
├─ task_id      (FK → TASKS)
├─ user_id      (FK → USERS)
├─ event_date   (DATE)
├─ status       (Proposed | Skipped)
├─ skip_reason  (TEXT, nullable)

PROPOSAL_SKIP_LOGS
├─ log_id       (PK, UUID)
├─ proposal_id  (FK → ROSTER_PROPOSALS, cascade delete)
├─ task_id      (INT)
├─ event_date   (DATE)
├─ reason       (TEXT)
├─ logged_at    (TIMESTAMP)
```

### 14.10 New API Endpoints

| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| `POST` | `/api/v1/proposals` | Start async generation — returns 202 + proposalId | Admin |
| `GET` | `/api/v1/proposals` | List all proposals (history) | Admin |
| `GET` | `/api/v1/proposals/{id}` | Get proposal detail + items + skip logs | Admin |
| `PATCH` | `/api/v1/proposals/{id}/items/{itemId}` | Edit an item (swap member) | Admin |
| `POST` | `/api/v1/proposals/{id}/items` | Add a new item to draft | Admin |
| `DELETE` | `/api/v1/proposals/{id}/items/{itemId}` | Remove an item from draft | Admin |
| `POST` | `/api/v1/proposals/{id}/publish` | Publish draft to live calendar | Admin |
| `POST` | `/api/v1/proposals/{id}/archive` | Archive a proposal | Admin |
| `GET` | `/api/v1/proposals/{id}/pdf` | Download draft PDF with DRAFT watermark | Admin |

### 14.11 New Frontend Pages

| Route | Page | Purpose |
|-------|------|---------|
| `/proposals` | `ProposalsPage` | History list, status badges, Generate button |
| `/proposals/generate` | `GenerateProposalPage` | Date range pickers + submit form |
| `/proposals/:id` | `ProposalDetailPage` | View items, edit members, Publish / Archive / Print |

**Async UX:** While `status === "Processing"`, the detail page shows a spinner with "Generating roster…". The page polls `GET /proposals/{id}` every 3 seconds and automatically transitions to the edit view when `status` becomes `"Draft"`.

### 14.12 Dashboard Widget (Admin)

A new widget on the Admin Dashboard shows:
- Count of proposals currently in `Draft` or `Processing` state
- Name and date of the most recently generated proposal
- Quick link to `/proposals`

---

## 🔧 15. CONFIGURATION (AI Module)

The AI module requires two values in server configuration. These must **never** be committed to source control.

```json
// appsettings.Development.json  (git-ignored)
"GitHubModels": {
  "Endpoint":  "https://models.inference.ai.azure.com",
  "ModelName": "gpt-4o",
  "Token":     "ghp_YOUR_GITHUB_PAT_HERE"
}
```

```json
// appsettings.json  (committed — safe placeholders only)
"GitHubModels": {
  "Endpoint":  "https://models.inference.ai.azure.com",
  "ModelName": "",
  "Token":     ""
}
```

> On Render (production), set `GitHubModels__ModelName` and `GitHubModels__Token` as **environment variables**.

---

## ✅ 16. ACCEPTANCE CRITERIA (AI Module)

| ID | Criterion |
|----|-----------|
| AC-01 | Admin can generate a proposal for any custom date range |
| AC-02 | UI shows "Generating roster…" and auto-updates when generation completes |
| AC-03 | Draft proposals are fully editable: add, edit, and delete items |
| AC-04 | Publish skips and logs conflicts — the batch never fails for one conflict |
| AC-05 | Published proposals remain in history permanently with status `Published` |
| AC-06 | Draft PDF renders with a diagonal DRAFT watermark via QuestPDF |
| AC-07 | All proposal data is tenant-isolated (EF Core Global Query Filter enforced) |
| AC-08 | Model name and token come from `IConfiguration` — never hardcoded |
| AC-09 | Only one `Processing` proposal per church at a time (returns 409 otherwise) |
| AC-10 | Admin Dashboard widget shows draft/processing count + most recent proposal |

---

## 📋 17. UPDATED MUST-HAVE CHECKLIST (Release 1.1 — AI Module)

### Backend
- [ ] `RosterProposal`, `RosterProposalItem`, `ProposalSkipLog` entities created
- [ ] EF Core Global Query Filter applied to `RosterProposal` (tenant-isolated)
- [ ] DB migration applied to Supabase
- [ ] 6 agent tool functions implemented and registered
- [ ] `ProposalAgentService` running against GitHub Models
- [ ] Async background job (`Channel<Guid>` + `BackgroundService`) confirmed working
- [ ] All CQRS command and query handlers implemented
- [ ] Publish logic: skip+log conflicts, create live Assignments, trigger notifications
- [ ] Draft PDF with DRAFT watermark generated via QuestPDF
- [ ] All 9 API endpoints require Admin role + X-Tenant-Id header

### Frontend
- [ ] `/proposals` history page with status badges
- [ ] `/proposals/generate` page with date range pickers
- [ ] `/proposals/:id` detail page with edit, publish, archive, print buttons
- [ ] "Generating roster…" spinner with 3-second polling
- [ ] Dashboard widget showing draft count + most recent proposal name
- [ ] All API calls use `authToken` + `X-Tenant-Id` (consistent with existing pages)

### System
- [ ] `ModelName` and `Token` read from `IConfiguration` — not hardcoded anywhere
- [ ] One-in-flight concurrency enforced per tenant (409 on duplicate generate)
- [ ] Skip logs written for every conflict during publish
- [ ] Published proposals never deleted from history
