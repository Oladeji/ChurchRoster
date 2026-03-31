# Church Ministry Rostering System
## Requirements Document
**Version: 6.0**  
**Last Updated: March 31, 2026**

---

## 📋 Table of Contents
1. [Project Overview](#project-overview)
2. [Stakeholders](#stakeholders)
3. [Functional Requirements](#functional-requirements)
4. [Non-Functional Requirements](#non-functional-requirements)
5. [User Roles](#user-roles)
6. [System Features](#system-features)
7. [Business Rules](#business-rules)
8. [User Stories](#user-stories)
9. [Technical Constraints](#technical-constraints)
10. [Success Criteria](#success-criteria)

---

## 1. Project Overview

### 1.1 Purpose
The Church Ministry Rostering System is a web-based Progressive Web Application (PWA) designed to streamline the scheduling and management of church ministry tasks. The system enables administrators to assign tasks to qualified members while ensuring fair distribution and preventing scheduling conflicts.

### 1.2 Scope
The system will:
- Manage church members and their ministry skills
- Define and track ministry tasks with skill requirements
- Create and manage task assignments
- Send notifications to members about assignments
- Allow members to accept or reject assignments
- Generate printable reports for ministry schedules
- Support both web and mobile access via PWA

### 1.3 Target Users
- Church administrators (1-3 users)
- Church members (20-100 users)
- Ministry coordinators

### 1.4 Key Benefits
- **Efficiency**: Reduce time spent on manual scheduling
- **Fairness**: Ensure equitable distribution of ministry responsibilities
- **Transparency**: Members can see their assignments and respond
- **Accessibility**: Access from any device, including mobile
- **Cost**: Zero monthly hosting costs using free tier services

---

## 2. Stakeholders

| Stakeholder | Role | Responsibilities |
|-------------|------|------------------|
| Church Administrator | Primary User | Manage members, assign tasks, generate reports |
| Church Members | End Users | View assignments, accept/reject tasks |
| Ministry Coordinators | Power Users | Coordinate specific ministry areas |
| IT Support | Technical | System maintenance, troubleshooting |
| Church Leadership | Oversight | Review reports, approve system usage |

---

## 3. Functional Requirements

### 3.1 User Management

#### FR-UM-001: User Registration
- System shall allow administrators to register new users
- Required fields: Name, Email, Phone, Role
- Email must be unique
- Password must meet security requirements (min 8 characters, uppercase, lowercase, number)

#### FR-UM-002: User Authentication
- System shall support email/password authentication
- System shall use JWT tokens for session management
- Tokens shall expire after 24 hours
- System shall support logout functionality

#### FR-UM-003: User Roles
- System shall support two roles: Admin and Member
- Admins have full system access
- Members can only view their own assignments

#### FR-UM-004: User Profile Management
- Users shall be able to update their contact information
- Admins shall be able to deactivate user accounts
- System shall track user creation and modification dates

### 3.2 Skills Management

#### FR-SM-001: Skill Definition
- Administrators shall be able to create ministry skills
- Each skill shall have: Name, Description, Active status
- Skill names must be unique
- Skills can be deactivated but not deleted (data integrity)

#### FR-SM-002: Skill Assignment
- Administrators shall assign skills to members
- A member can have multiple skills
- Skill assignments shall be tracked with assignment date
- Admins can remove skills from members

#### FR-SM-003: Predefined Skills
- System shall include default skills:
  - CanLeadBibleStudy
  - CanLeadPreaching
  - CanLeadPrayer
  - CanMakeAnnouncements

### 3.3 Task Management

#### FR-TM-001: Task Definition
- Administrators shall create ministry tasks
- Required fields: Task Name, Frequency, Day Rule
- Optional fields: Required Skill, Restricted Flag
- Tasks can be marked as active/inactive

#### FR-TM-002: Task Frequencies
- System shall support two frequencies:
  - Weekly (specific day of week)
  - Monthly (specific day or rule like "Last Friday")

#### FR-TM-003: Task Restrictions
- Tasks can be marked as "Restricted"
- Restricted tasks require specific skills
- Non-restricted tasks can be assigned to any member

#### FR-TM-004: Default Tasks
System shall include 7 default tasks:
1. Lead Bible Study (Weekly-Tuesday, Restricted)
2. Lead Prayer Meeting (Weekly-Tuesday)
3. Lead Preaching (Weekly-Sunday, Restricted)
4. Lead Opening Prayer (Weekly-Sunday)
5. Lead Announcements (Weekly-Sunday)
6. Lead Closing Prayer (Weekly-Sunday)
7. Lead All-Night Prayer (Monthly-Last Friday)
8. Lead Vigil Prayer (Monthly-Last Saturday)

### 3.4 Assignment Management

#### FR-AM-001: Create Assignment
- Administrators shall assign tasks to members for specific dates
- System shall validate member qualification
- System shall check for scheduling conflicts
- System shall track who created the assignment

#### FR-AM-002: Assignment Statuses
System shall support six assignment statuses:
- **Pending**: Initial state after creation
- **Accepted**: Member accepted the assignment
- **Rejected**: Member rejected the assignment
- **Confirmed**: Admin confirmed after acceptance
- **Completed**: Task was completed (past event)
- **Expired**: Past pending assignment that wasn't accepted

#### FR-AM-003: Assignment Validation
Before creating assignment, system shall check:
- Member is active
- Task is active
- Member is qualified (if task is restricted)
- No scheduling conflict (same date)
- Monthly limit not exceeded (warning only)

#### FR-AM-004: Override Capability
- Admins can override qualification requirements
- Override shall be flagged in the assignment record
- Override requires explicit admin confirmation

#### FR-AM-005: Member Response
- Members shall be able to accept assignments
- Members shall be able to reject assignments with reason
- Response shall update assignment status
- Response shall trigger notification to admin

#### FR-AM-006: Auto-Status Updates
- System shall auto-expire pending assignments past event date
- System shall auto-complete accepted assignments past event date
- Status updates shall run daily

### 3.5 Conflict Detection

#### FR-CD-001: Same-Day Conflict
- System shall prevent assigning multiple tasks to same member on same date
- Exception: Admin override allowed with warning

#### FR-CD-002: Qualification Check
- System shall verify member has required skill for restricted tasks
- System shall display only qualified members in assignment dropdown
- Exception: Admin override allowed

#### FR-CD-003: Fairness Warning
- System shall track monthly assignment count per member
- System shall warn when member exceeds monthly limit
- Warning shall not block assignment (admin decision)

### 3.6 Notification System

#### FR-NS-001: Push Notifications
- System shall send push notification when task is assigned
- Notification shall include: Task Name, Date, Status
- Members must opt-in to notifications

#### FR-NS-002: Email Notifications
- System shall send email when task is assigned
- Email shall include: Task details, Accept/Reject links
- Email shall be sent to member's registered email

#### FR-NS-003: Admin Notifications
- Admins shall be notified when member responds to assignment
- Notification shall include member name and response

### 3.7 Calendar & Reporting

#### FR-CR-001: Calendar View
- System shall display assignments in monthly calendar view
- Calendar shall show: Date, Task Name, Assigned Member, Status
- Calendar shall be filterable by member, task, status

#### FR-CR-002: Printable Report
- System shall generate printable ministry schedule
- Report shall include all assignments for selected month
- Report shall be formatted for printing (PDF)

#### FR-CR-003: Assignment History
- System shall maintain complete assignment history
- Admins can view past assignments
- System shall track all status changes

---

## 4. Non-Functional Requirements

### 4.1 Performance

#### NFR-PF-001: Response Time
- Page loads shall complete within 2 seconds
- API responses shall complete within 500ms
- Database queries shall execute within 100ms

#### NFR-PF-002: Scalability
- System shall support up to 100 concurrent users
- System shall handle up to 1000 assignments per month
- Database shall support up to 10,000 total records

### 4.2 Security

#### NFR-SC-001: Authentication
- All API endpoints shall require authentication
- JWT tokens shall be securely stored
- Passwords shall be hashed using BCrypt

#### NFR-SC-002: Authorization
- Admin-only endpoints shall verify admin role
- Members shall only access their own data
- SQL injection shall be prevented via parameterized queries

#### NFR-SC-003: Data Protection
- HTTPS shall be enforced for all connections
- Sensitive data shall not be logged
- Environment variables shall not be committed to repository

### 4.3 Usability

#### NFR-US-001: Mobile Responsiveness
- System shall be fully functional on mobile devices
- UI shall adapt to screen sizes (320px to 2560px)
- Touch interactions shall be optimized

#### NFR-US-002: PWA Capabilities
- App shall be installable on iOS and Android
- App shall work offline (basic functionality)
- App shall load within 3 seconds on 3G network

#### NFR-US-003: Accessibility
- System shall meet WCAG 2.1 Level AA standards
- Forms shall have proper labels
- Color contrast shall meet accessibility requirements

### 4.4 Reliability

#### NFR-RL-001: Availability
- System shall have 99% uptime
- Planned maintenance shall be scheduled during off-peak hours
- System shall handle graceful degradation

#### NFR-RL-002: Data Integrity
- Database shall maintain referential integrity
- Transactions shall be ACID-compliant
- Data shall be backed up daily

### 4.5 Maintainability

#### NFR-MT-001: Code Quality
- Code shall follow language-specific best practices
- Code shall be documented with comments
- Code shall pass linting checks

#### NFR-MT-002: Deployment
- System shall support CI/CD pipeline
- Deployment shall complete within 10 minutes
- Rollback capability shall be available

---

## 5. User Roles

### 5.1 Administrator

**Permissions:**
- Create, update, delete members
- Assign skills to members
- Create, update, delete tasks
- Create, update, delete assignments
- Override qualification requirements
- View all assignments and reports
- Generate printable schedules
- Manage system settings

**Use Cases:**
- Weekly task assignment
- Monthly schedule planning
- Member skill management
- Report generation

### 5.2 Member

**Permissions:**
- View own profile
- Update contact information
- View own assignments
- Accept/reject assignments
- View ministry calendar

**Use Cases:**
- Check upcoming assignments
- Respond to task assignments
- View ministry schedule

---

## 6. System Features

### 6.1 Dashboard
- Admin: View statistics, recent assignments, pending responses
- Member: View upcoming assignments, pending tasks

### 6.2 Member Management
- List all members with skills
- Add/edit/deactivate members
- Assign/remove skills
- View member assignment history

### 6.3 Task Catalog
- List all ministry tasks
- Create new tasks
- Edit task details
- Activate/deactivate tasks

### 6.4 Assignment Workflow
- Create assignment → Send notification → Member responds → Admin confirms → Task completed

### 6.5 Calendar Interface
- Month view with all assignments
- Color-coded by status
- Click to view assignment details
- Filter by member, task, status

---

## 7. Business Rules

### BR-001: Qualification Rules
- If task is restricted, member MUST have required skill (unless override)
- Admin can override with explicit confirmation
- Override is logged in assignment record

### BR-002: Scheduling Rules
- One member cannot have multiple tasks on same date (unless override)
- Assignments can be created up to 6 months in advance
- Past-date assignments default to "Completed" status

### BR-003: Fairness Rules
- Each member has optional monthly assignment limit
- System warns when limit exceeded
- Warning does not block assignment (admin decision)

### BR-004: Notification Rules
- Notifications sent immediately upon assignment creation
- Member can opt-out of push notifications
- Email notifications cannot be disabled

### BR-005: Status Transition Rules
Valid status transitions:
- Pending → Accepted, Rejected, Expired
- Accepted → Confirmed, Rejected, Completed
- Confirmed → Completed
- Rejected → Pending (new assignment)
- Expired → (no further transitions)
- Completed → (no further transitions)

---

## 8. User Stories

### As an Administrator

**US-A-001**: As an admin, I want to assign tasks to qualified members so that ministry responsibilities are properly distributed.

**US-A-002**: As an admin, I want to see which members are available for a specific date so that I avoid double-booking.

**US-A-003**: As an admin, I want to receive notifications when members respond to assignments so that I can plan accordingly.

**US-A-004**: As an admin, I want to generate a printable monthly schedule so that I can post it in church.

**US-A-005**: As an admin, I want to track assignment history so that I can ensure fair distribution.

### As a Member

**US-M-001**: As a member, I want to receive notifications about new assignments so that I know my responsibilities.

**US-M-002**: As a member, I want to accept or reject assignments so that I can manage my commitments.

**US-M-003**: As a member, I want to see all my upcoming assignments in one place so that I can plan ahead.

**US-M-004**: As a member, I want to access the system from my phone so that I can respond on the go.

**US-M-005**: As a member, I want to provide a reason when rejecting an assignment so that the admin understands my situation.

---

## 9. Technical Constraints

### 9.1 Technology Stack
- **Backend**: .NET 10, C#, Entity Framework Core
- **Database**: PostgreSQL (Supabase)
- **Frontend**: React 18+, TypeScript, Vite
- **Notifications**: Firebase Cloud Messaging
- **Email**: Brevo SMTP or MailKit
- **Hosting**: Render (backend), Vercel (frontend)

### 9.2 Browser Support
- Chrome 90+
- Firefox 88+
- Safari 14+
- Edge 90+
- Mobile browsers (iOS Safari, Chrome Mobile)

### 9.3 Data Retention
- Active assignments: Indefinite
- Completed assignments: 2 years
- User accounts: Indefinite (can be deactivated)
- Audit logs: 1 year

---

## 10. Success Criteria

### 10.1 Functional Success
- [x] All 7 default tasks can be created
- [x] Assignments can be created and managed
- [x] Members receive notifications
- [x] Calendar displays correctly
- [x] Reports can be generated
- [x] PWA can be installed on mobile

### 10.2 Performance Success
- [x] Page loads < 2 seconds
- [x] API responses < 500ms
- [x] 99% uptime achieved

### 10.3 User Acceptance
- [x] Admin can complete assignment workflow in < 2 minutes
- [x] Members can respond to assignments in < 30 seconds
- [x] 90% user satisfaction rating
- [x] Zero data loss incidents

### 10.4 Business Success
- [x] Zero monthly hosting costs
- [x] Reduce scheduling time by 80%
- [x] 100% member participation
- [x] Deployed within 6 weeks

---

## Appendix A: Glossary

| Term | Definition |
|------|------------|
| **Assignment** | A task assigned to a specific member for a specific date |
| **Restricted Task** | Task requiring specific skill qualification |
| **Override** | Admin ability to bypass qualification rules |
| **Monthly Limit** | Maximum assignments per member per month |
| **PWA** | Progressive Web App - installable web application |
| **JWT** | JSON Web Token - authentication mechanism |

---

## Appendix B: Change Log

| Version | Date | Changes | Author |
|---------|------|---------|--------|
| 6.0 | 2026-03-31 | Complete requirements specification | Development Team |
| 5.0 | 2026-03-15 | Added PWA requirements | Development Team |
| 4.0 | 2026-03-01 | Added notification system | Development Team |
| 3.0 | 2026-02-15 | Refined business rules | Development Team |
| 2.0 | 2026-02-01 | Added non-functional requirements | Development Team |
| 1.0 | 2026-01-15 | Initial requirements | Development Team |

---

*Document Version: 6.0*  
*Last Updated: March 31, 2026*  
*Status: Approved*
