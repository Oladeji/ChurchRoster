# 📄 Church Ministry Rostering System  
## Development Guide & Roadmap  
**Version 1.1**  
*Updated: March 2026 - Added Email Invitation System to Week 5*
*Copy this entire document and paste into Word, Google Docs, or save as .txt/.md*

---

## 🎯 1. DEVELOPMENT OVERVIEW

You now have:
- ✅ **Requirements Document (v6.0)** – What to build
- ✅ **Technical Architecture (v1.1)** – How to host it
- ✅ **This Guide** – Step-by-step development instructions

**Estimated Timeline:** 4-6 Weeks (Part-time)  
**Your Stack:** .NET 10, React, TypeScript, PostgreSQL (Supabase)  
**Goal:** Zero monthly cost, PWA for mobile

---

## 📁 2. PROJECT STRUCTURE
** use central package for managing libraries and dependencies across backend 
Create a monorepo or separate repos. Here's the recommended structure:

```
church-roster-system/
├── backend/                    # .NET 10 Web API
│   ├── ChurchRoster.Api/
│   │   ├── Controllers/
│   │   ├── Models/
│   │   ├── Services/
│   │   ├── Data/
│   │   ├── DTOs/
│   │   └── Program.cs
│   ├── Dockerfile
│   └── appsettings.json
│
├── frontend/                   # React + TypeScript (Vite)
│   ├── src/
│   │   ├── components/
│   │   ├── pages/
│   │   ├── services/
│   │   ├── hooks/
│   │   ├── context/
│   │   └── App.tsx
│   ├── public/
│   │   ├── manifest.json
│   │   └── icons/
│   ├── vite.config.ts
│   └── package.json
│
├── docs/                       # Requirements & Architecture docs
├── database/                   # SQL scripts & migrations
└── README.md
```

---

## 🗓️ 3. DEVELOPMENT ROADMAP (WEEK BY WEEK)

### **Week 1: Backend Foundation**
| Day | Task | Deliverable |
|-----|------|-------------|
| 1 | Set up .NET 10 Web API project | Running API locally |
| 2 | Set up Supabase PostgreSQL database | Connection string working |
| 3 | Create database tables (Users, Skills, Tasks, Assignments) | SQL scripts complete |
| 4 | Implement Entity Framework Core models & DbContext | DB migration working |
| 5 | Create Auth endpoints (Login, Register, JWT) | Can authenticate users |
| 6-7 | Buffer/Testing | API tested with Swagger |

### **Week 2: Backend Core Features**
| Day | Task | Deliverable |
|-----|------|-------------|
| 1 | Create Member Management endpoints (CRUD) | Can add/edit members |
| 2 | Create Skills Management endpoints | Can assign skills to members |
| 3 | Create Task Catalog endpoints | All 7 tasks defined in DB |
| 4 | Create Assignment endpoints (Create, Read, Update) | Can assign tasks |
| 5 | Implement business rules (qualification check, conflict detection) | Validation working |
| 6-7 | Deploy Backend to Render | API live on internet |

### **Week 3: Frontend Foundation**
| Day | Task | Deliverable |
|-----|------|-------------|
| 1 | Set up React + TypeScript with Vite | App running locally |
| 2 | Configure PWA (manifest.json, service worker) | Installable on phone |
| 3 | Create Auth pages (Login, Register) | Can log in |
| 4 | Create Admin Dashboard layout | Navigation working |
| 5 | Create Member Dashboard layout | Navigation working |
| 6-7 | Connect Frontend to Backend API | Data flowing |

### **Week 4: Calendar & Assignment UI**
| Day | Task | Deliverable |
|-----|------|-------------|
| 1 | Build Calendar Component (Month view) | Calendar renders |
| 2 | Build Assignment Modal (Select task, select member) | Can assign tasks |
| 3 | Implement qualification filter in dropdown | Only qualified members show |
| 4 | Build Task Status indicators (Pending, Accepted, Rejected) | Status visible |
| 5 | Build Member Task List view | Members see their tasks |
| 6-7 | Testing & Bug Fixes | Core flow working |

### **Week 5: Notifications, Reports & Email Invitations**
| Day | Task | Deliverable |
|-----|------|-------------|
| 1 | Set up Brevo email service & SMTP configuration | Can send emails |
| 2 | Implement Email Invitation System (Backend) | Invitation endpoints ready |
| 3 | Implement Email Invitation System (Frontend) | Accept invitation page |
| 4 | Set up Firebase project & get VAPID keys | Firebase ready |
| 5 | Implement Push Notifications (Frontend & Backend) | Can send/receive pushes |
| 6 | Build Printable Report (PDF generation) | Can print roster |
| 7 | Testing & Bug Fixes | Notifications & invitations working |

**Week 5 Focus Areas:**

**Email Invitation System (Days 1-3):**
- **Backend Tasks:**
  - Create `Invitation` entity and database table
  - Implement `InvitationService` (create, verify, accept tokens)
  - Implement `EmailService` using Brevo SMTP
  - Add invitation endpoints: `/api/invitations/send`, `/api/invitations/verify/{token}`, `/api/invitations/accept`
  - Update member creation to support invitation option
  - Create HTML email template for invitations

- **Frontend Tasks:**
  - Create `AcceptInvitation.tsx` page for password setup
  - Update `Members.tsx` modal with "Send invitation email" checkbox
  - Add invitation routes to `App.tsx`
  - Implement token verification on invitation page
  - Build password creation form for invited members

- **Database Migration:**
  - Add `invitations` table with token, expiry, and usage tracking
  - Modify `users.password_hash` to allow NULL (for pending invitations)

- **Features Delivered:**
  - ✅ Admin can create member and send invitation email
  - ✅ Member receives professional invitation email
  - ✅ Member clicks link to set up their own password
  - ✅ Invitation tokens expire after 7 days
  - ✅ Tokens can only be used once
  - ✅ Option to still use manual password for urgent cases

**Push Notifications (Days 4-5):**
- Set up Firebase Cloud Messaging
- Implement notification service in backend
- Configure service worker in frontend
- Test push notifications on mobile devices

**Reports (Day 6):**
- Implement PDF generation for ministry rosters
- Create printable calendar view
- Add export functionality

### **Week 6: Polish & Launch**
| Day | Task | Deliverable |
|-----|------|-------------|
| 1 | Implement Accept/Reject workflow | Members can respond |
| 2 | Implement Past Event auto-status update | Completed/Expired logic |
| 3 | Security audit (API protection, role checks) | Secure system |
| 4 | User testing with real church members | Feedback collected |
| 5 | Fix bugs from testing | Stable system |
| 6-7 | Go Live & Training | System launched |

### **Cross-Cutting Upgrade: Multitenancy (One Church = One Tenant)**

**Approved Multitenancy Strategy:**
- One tenant maps to one church
- Single database with discriminator column (`TenantId`)
- All business data is tenant-specific
- Email uniqueness is per tenant, not global
- No domain-based tenant resolution
- Login uses email + church selector dropdown
- No super admin; admin belongs to one church only
- Existing data migrates into a seeded tenant named `Default Church`

**Critical Enforcement Rule:**
- Use **EF Core Global Query Filters** at the Infrastructure Layer so tenant filtering is automatic and developers cannot accidentally leak data by forgetting a `.Where()` clause.

**Backend Multitenancy Tasks:**
- Create `Tenant` entity/table
- Add `TenantId` to: `users`, `skills`, `tasks`, `assignments`, `invitations`, `user_skills`
- Add a scoped tenant context service
- Add Tenant Resolution Middleware
- Resolve tenant from JWT `TenantId` claim for authenticated requests
- Resolve tenant from request header (such as `X-Tenant-Id`) for login and pre-auth flows
- Add EF Core global query filters for all tenant-owned entities
- Auto-stamp `TenantId` during `SaveChanges`/`SaveChangesAsync`
- Update JWT generation to include `TenantId`
- Add tenant endpoints for frontend church selection
- Update unique indexes to use composite tenant-aware uniqueness

**Frontend Multitenancy Tasks:**
- Add church selector dropdown to login form
- Add tenant loading service for login UX
- Store selected tenant using best-practice local persistence
- Attach `TenantId` to API requests through the API interceptor
- Keep tenant selection explicit; do not auto-select by email domain

**Migration Strategy:**
- Create a default tenant named `Default Church`
- Assign all existing records to `Default Church`
- Convert global unique email constraint to tenant-scoped uniqueness

**Security Outcome:**
- Tenant isolation enforced centrally
- Lower risk of cross-tenant data exposure
- Auth, invitations, members, reports, assignments, tasks, and skills all remain tenant-scoped

---

## 💻 4. GET STARTED: STEP-BY-STEP

### **Step 1: Create Backend (.NET 10)**

```bash
# Create new Web API project
dotnet new webapi -n ChurchRoster.Api --framework net10.0

# Navigate to folder
cd ChurchRoster.Api

# Install required packages
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package FirebaseAdmin
dotnet add package SendGrid  # Or use System.Net.Mail for Brevo SMTP
```

### **Step 2: Create Database Models**

**`Models/User.cs`**
```csharp
public class User
{
    public int UserId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string PasswordHash { get; set; }
    public string Role { get; set; } // "Admin" or "Member"
    public int? MonthlyLimit { get; set; }
    public string? DeviceToken { get; set; } // For push notifications
    public bool IsActive { get; set; } = true;
    public List<UserSkill> UserSkills { get; set; }
    public List<Assignment> Assignments { get; set; }
}
```

**`Models/Skill.cs`**
```csharp
public class Skill
{
    public int SkillId { get; set; }
    public string SkillName { get; set; } // e.g., "CanLeadBibleStudy"
    public string Description { get; set; }
    public bool IsActive { get; set; } = true;
    public List<UserSkill> UserSkills { get; set; }
    public List<Task> Tasks { get; set; }
}
```

**`Models/Task.cs`**
```csharp
public class Task
{
    public int TaskId { get; set; }
    public string TaskName { get; set; }
    public string Frequency { get; set; } // "Weekly" or "Monthly"
    public string DayRule { get; set; } // "Tuesday", "Sunday", "Last Friday"
    public int? RequiredSkillId { get; set; }
    public bool IsRestricted { get; set; }
    public Skill RequiredSkill { get; set; }
    public List<Assignment> Assignments { get; set; }
}
```

**`Models/Assignment.cs`**
```csharp
public class Assignment
{
    public int AssignmentId { get; set; }
    public int TaskId { get; set; }
    public int UserId { get; set; }
    public DateTime EventDate { get; set; }
    public string Status { get; set; } // Pending, Accepted, Rejected, Confirmed, Completed, Expired
    public string? RejectionReason { get; set; }
    public bool IsOverride { get; set; }
    public int AssignedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Task Task { get; set; }
    public User User { get; set; }
}
```

### **Step 3: Create DbContext**

**`Data/AppDbContext.cs`**
```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Skill> Skills { get; set; }
    public DbSet<UserSkill> UserSkills { get; set; }
    public DbSet<Task> Tasks { get; set; }
    public DbSet<Assignment> Assignments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure relationships
        modelBuilder.Entity<UserSkill>()
            .HasKey(us => new { us.UserId, us.SkillId });

        modelBuilder.Entity<Assignment>()
            .HasIndex(a => new { a.UserId, a.EventDate });
    }
}
```

### **Step 4: Create Frontend (React + Vite)**

```bash
# Create React app with Vite
npm create vite@latest church-roster-frontend -- --template react-ts

# Navigate to folder
cd church-roster-frontend

# Install dependencies
npm install
npm install react-router-dom axios firebase vite-plugin-pwa
npm install -D @types/node
```

### **Step 5: Configure PWA**

**`vite.config.ts`**
```typescript
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import { VitePWA } from 'vite-plugin-pwa'

export default defineConfig({
  plugins: [
    react(),
    VitePWA({
      registerType: 'autoUpdate',
      includeAssets: ['favicon.ico', 'apple-touch-icon.png', 'masked-icon.svg'],
      manifest: {
        name: 'Church Ministry Roster',
        short_name: 'ChurchRoster',
        description: 'Church Ministry Scheduling App',
        theme_color: '#4F46E5',
        background_color: '#ffffff',
        display: 'standalone',
        icons: [
          {
            src: 'pwa-192x192.png',
            sizes: '192x192',
            type: 'image/png'
          },
          {
            src: 'pwa-512x512.png',
            sizes: '512x512',
            type: 'image/png'
          }
        ]
      }
    })
  ]
})
```

### **Step 6: Set Up Supabase Database**

1. Go to [supabase.com](https://supabase.com) and create a free account
2. Create a new project
3. Go to **SQL Editor** and run your schema:

```sql
-- Users Table
CREATE TABLE users (
    user_id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    email VARCHAR(255) UNIQUE NOT NULL,
    phone VARCHAR(50),
    password_hash VARCHAR(255) NOT NULL,
    role VARCHAR(50) DEFAULT 'Member',
    monthly_limit INTEGER,
    device_token TEXT,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

-- Skills Table
CREATE TABLE skills (
    skill_id SERIAL PRIMARY KEY,
    skill_name VARCHAR(100) NOT NULL,
    description TEXT,
    is_active BOOLEAN DEFAULT TRUE
);

-- User Skills (Many-to-Many)
CREATE TABLE user_skills (
    user_id INTEGER REFERENCES users(user_id),
    skill_id INTEGER REFERENCES skills(skill_id),
    assigned_date TIMESTAMP DEFAULT NOW(),
    PRIMARY KEY (user_id, skill_id)
);

-- Tasks Table
CREATE TABLE tasks (
    task_id SERIAL PRIMARY KEY,
    task_name VARCHAR(255) NOT NULL,
    frequency VARCHAR(50),
    day_rule VARCHAR(50),
    required_skill_id INTEGER REFERENCES skills(skill_id),
    is_restricted BOOLEAN DEFAULT FALSE
);

-- Assignments Table
CREATE TABLE assignments (
    assignment_id SERIAL PRIMARY KEY,
    task_id INTEGER REFERENCES tasks(task_id),
    user_id INTEGER REFERENCES users(user_id),
    event_date DATE NOT NULL,
    status VARCHAR(50) DEFAULT 'Pending',
    rejection_reason TEXT,
    is_override BOOLEAN DEFAULT FALSE,
    assigned_by INTEGER REFERENCES users(user_id),
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

-- Insert Default Skills
INSERT INTO skills (skill_name, description) VALUES
('CanLeadBibleStudy', 'Can lead Tuesday Bible Study'),
('CanLeadPreaching', 'Can lead Sunday Preaching');

-- Insert Default Tasks
INSERT INTO tasks (task_name, frequency, day_rule, required_skill_id, is_restricted) VALUES
('Lead Bible Study', 'Weekly', 'Tuesday', 1, TRUE),
('Lead Prayer Meeting', 'Weekly', 'Tuesday', NULL, FALSE),
('Lead Preaching', 'Weekly', 'Sunday', 2, TRUE),
('Lead Prayer', 'Weekly', 'Sunday', NULL, FALSE),
('Lead Announcements', 'Weekly', 'Sunday', NULL, FALSE),
('Lead Prayer', 'Monthly', 'Last Friday', NULL, FALSE),
('Lead Prayer', 'Monthly', 'Last Saturday', NULL, FALSE);
```

4. Get your **Connection String** from Supabase Dashboard → Settings → Database

---

## 🔐 5. SECURITY CHECKLIST

| Item | Status |
|------|--------|
| [ ] All API endpoints require authentication | |
| [ ] Admin endpoints check for Admin role | |
| [ ] Passwords are hashed (use BCrypt or ASP.NET Identity) | |
| [ ] JWT tokens expire (set 24-hour expiry) | |
| [ ] HTTPS enforced on all connections | |
| [ ] CORS configured to allow only your frontend domain | |
| [ ] Environment variables not committed to GitHub | |
| [ ] SQL injection prevented (use EF Core parameterized queries) | |
| [ ] Tenant isolation enforced by EF Core global query filters | |
| [ ] JWT includes `TenantId` claim | |
| [ ] Login requires explicit church selection | |
| [ ] Unique email constraint scoped to tenant | |

---

## 🏢 5A. MULTITENANCY IMPLEMENTATION GUIDE

### **Architecture Decision**

This system uses **single-database multitenancy with a discriminator column**.

**Tenant definition:**
- One tenant = one church

**Isolation mechanism:**
- `TenantId` column on all tenant-owned tables
- EF Core Global Query Filters in `AppDbContext`
- Tenant Resolution Middleware in API pipeline
- JWT `TenantId` claim for authenticated users

### **Tenant Table**

Example fields:

```csharp
public class Tenant
{
    public int TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
```

### **Tenant-Owned Entities**

Add `TenantId` to:
- `User`
- `Skill`
- `MinistryTask`
- `Assignment`
- `Invitation`
- `UserSkill`

### **Global Query Filters (Critical)**

In `AppDbContext`, configure a tenant context and apply filters so all queries are automatically tenant-scoped:

```csharp
modelBuilder.Entity<User>()
    .HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
```

Apply the same pattern to every tenant-owned entity.

### **Tenant Resolution Middleware**

Resolve tenant per request using:
1. JWT `TenantId` claim for authenticated requests
2. `X-Tenant-Id` header for login and other pre-auth requests

The middleware should populate a scoped `ITenantContext` service.

### **JWT Requirements**

Every authenticated token must include:
- `TenantId`

Recommended optional claims:
- `TenantName`
- `TenantSlug`

### **Frontend Login UX**

Login form must include:
- Email
- Password
- Church selector dropdown

Do **not** auto-select a tenant by email domain.

### **API Interceptor**

Frontend API interceptor should attach:
- `Authorization: Bearer {token}` when authenticated
- `X-Tenant-Id` for login and other tenant-aware requests

### **Tenant-Aware Uniqueness**

Use composite unique indexes such as:

```sql
CREATE UNIQUE INDEX idx_users_tenant_email ON users (tenant_id, email);
```

### **Migration of Existing Data**

Migration steps:
1. Create `tenants` table
2. Insert `Default Church`
3. Add nullable `tenant_id` to existing tables
4. Update all current rows to use `Default Church`
5. Make `tenant_id` non-nullable
6. Add foreign keys and tenant-aware indexes

### **Admin Model**

- Admin belongs to one church only
- No super admin exists in this design

### **Testing Checklist for Multitenancy**

- [ ] User from Church A cannot see data from Church B
- [ ] Email uniqueness works per tenant
- [ ] Login fails if wrong church is selected
- [ ] JWT contains `TenantId`
- [ ] Invitations remain tenant-scoped
- [ ] Reports remain tenant-scoped
- [ ] CRUD operations auto-apply tenant filters without explicit `.Where()` clauses

---

## 🧪 6. TESTING STRATEGY

### **Backend Testing**
```bash
# Install xUnit for testing
dotnet add package Microsoft.NET.Test.Sdk
dotnet add package xunit
dotnet add package xunit.runner.visualstudio

# Create test project
dotnet new xunit -n ChurchRoster.Tests
```

**Test Cases to Cover:**
- [ ] User can register and login
- [ ] Admin can assign task to qualified member
- [ ] System blocks assigning unqualified member to restricted task
- [ ] System blocks double-booking same day
- [ ] Fairness warning triggers when limit exceeded
- [ ] Push notification sends on assignment
- [ ] Email sends on assignment

### **Frontend Testing**
- [ ] Login page works
- [ ] Calendar renders correctly
- [ ] Assignment modal opens and saves
- [ ] Member can accept/reject task
- [ ] PWA installs on phone
- [ ] Push notification received on phone

---

## 🚀 7. DEPLOYMENT CHECKLIST

### **Backend (Render)**
- [ ] Dockerfile created with .NET 10 base image
- [ ] GitHub repo connected to Render
- [ ] Environment variables set (DB Connection, JWT Secret, Firebase, Brevo)
- [ ] API is accessible via HTTPS
- [ ] Swagger disabled in production

### **Frontend (Vercel)**
- [ ] GitHub repo connected to Vercel
- [ ] Environment variables set (API URL, Firebase keys)
- [ ] Custom domain configured
- [ ] PWA manifest validated
- [ ] HTTPS enabled (automatic on Vercel)

### **Database (Supabase)**
- [ ] Project created
- [ ] Schema migrated
- [ ] Connection string secured in backend env vars
- [ ] Backup strategy documented

### **Notifications**
- [ ] Firebase project created
- [ ] VAPID keys configured in frontend
- [ ] Service account JSON configured in backend
- [ ] Test push sent successfully
- [ ] Brevo account created
- [ ] SMTP credentials configured
- [ ] Test email sent successfully

---

## 📱 8. MEMBER ONBOARDING GUIDE

Create a simple 1-page PDF to share with church members:

```
═══════════════════════════════════════════════════
     HOW TO INSTALL THE CHURCH ROSTER APP
═══════════════════════════════════════════════════

STEP 1: Open the link on your phone
        https://roster.yourchurch.com

STEP 2: Log in with your email and password
        (Contact admin if you need an account)

STEP 3: Add to Home Screen
        iPhone: Tap Share → "Add to Home Screen"
        Android: Tap Menu (⋮) → "Install App"

STEP 4: Allow Notifications
        Tap "Allow" when prompted

STEP 5: You're Done!
        You'll now receive alerts when assigned tasks.

═══════════════════════════════════════════════════
Need Help? Contact: admin@yourchurch.com
═══════════════════════════════════════════════════
```

---

## 🆘 9. TROUBLESHOOTING COMMON ISSUES

| Issue | Solution |
|-------|----------|
| Backend won't deploy to Render | Check Dockerfile uses correct .NET 10 image |
| Push notifications not working on iOS | Ensure user added to Home Screen + iOS 16.4+ |
| Database connection fails | Check Supabase connection string + firewall settings |
| CORS errors in frontend | Add frontend domain to allowed origins in backend |
| PWA won't install | Ensure manifest.json is valid + served over HTTPS |
| Email not sending | Check Brevo SMTP credentials + verify sender domain |
| Invitation email not received | Check spam folder, verify email service configuration |
| Invitation link expired | Generate new invitation, set longer expiry time |

---

## 📧 10. EMAIL INVITATION SYSTEM (WEEK 5)

### **Overview**

The Email Invitation System provides a professional onboarding experience where:
1. Admin creates a member account (without setting a password)
2. System automatically sends an invitation email to the member
3. Member clicks the invitation link
4. Member sets their own password
5. Member account is activated and they can log in

**Benefits:**
- ✅ More secure (member chooses their own password)
- ✅ Professional user experience
- ✅ No need for admin to manually share passwords
- ✅ Audit trail of invitation sent/accepted
- ✅ Tokens expire for security

---

### **Implementation Guide**

#### **A. Database Setup**

Add invitations table to track invitation tokens:

```sql
-- Week 5: Add Invitations Table
CREATE TABLE invitations (
    invitation_id SERIAL PRIMARY KEY,
    user_id INTEGER REFERENCES users(user_id) ON DELETE CASCADE,
    token VARCHAR(255) UNIQUE NOT NULL,
    expires_at TIMESTAMP NOT NULL,
    is_used BOOLEAN DEFAULT FALSE,
    used_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT NOW()
);

CREATE INDEX idx_invitations_token ON invitations(token);
CREATE INDEX idx_invitations_user ON invitations(user_id);

-- Modify users table to allow NULL password for pending invitations
ALTER TABLE users ALTER COLUMN password_hash DROP NOT NULL;

-- Add invitation tracking to users
ALTER TABLE users ADD COLUMN invitation_sent_at TIMESTAMP;
ALTER TABLE users ADD COLUMN invitation_accepted_at TIMESTAMP;
```

#### **B. Backend Implementation (.NET 10)**

**1. Create Invitation Entity**

```csharp
// ChurchRoster.Core/Entities/Invitation.cs
public class Invitation
{
    public int InvitationId { get; set; }
    public int UserId { get; set; }
    public string Token { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime? UsedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
}
```

**2. Create Email Service**

```csharp
// ChurchRoster.Application/Interfaces/IEmailService.cs
public interface IEmailService
{
    Task<bool> SendInvitationEmailAsync(string toEmail, string memberName, string invitationLink);
    Task<bool> SendPasswordResetEmailAsync(string toEmail, string resetLink);
}

// ChurchRoster.Infrastructure/Services/EmailService.cs
using System.Net;
using System.Net.Mail;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendInvitationEmailAsync(string toEmail, string memberName, string invitationLink)
    {
        try
        {
            var emailSettings = _configuration.GetSection("EmailSettings");

            using var client = new SmtpClient(emailSettings["SmtpServer"], 
                int.Parse(emailSettings["SmtpPort"] ?? "587"))
            {
                Credentials = new NetworkCredential(
                    emailSettings["Username"],
                    emailSettings["Password"]
                ),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(
                    emailSettings["SenderEmail"] ?? "noreply@church.com",
                    emailSettings["SenderName"] ?? "Church Roster System"
                ),
                Subject = "Welcome to Church Ministry Roster - Set Up Your Account",
                Body = GetInvitationEmailBody(memberName, invitationLink),
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);

            _logger.LogInformation("Invitation email sent to {Email}", toEmail);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send invitation email to {Email}", toEmail);
            return false;
        }
    }

    private string GetInvitationEmailBody(string memberName, string invitationLink)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; }}
        .header {{ background: linear-gradient(135deg, #4F46E5 0%, #7C3AED 100%); padding: 40px; text-align: center; }}
        .header h1 {{ color: white; margin: 0; }}
        .content {{ padding: 40px; background: #f9fafb; }}
        .button {{ background: #4F46E5; color: white; padding: 15px 30px; text-decoration: none; border-radius: 8px; display: inline-block; }}
        .footer {{ background: #1F2937; padding: 20px; text-align: center; color: white; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>Church Ministry Roster</h1>
    </div>
    <div class='content'>
        <h2>You're Invited!</h2>
        <p>Hello {memberName},</p>
        <p>You've been added to the Church Ministry Roster system. Click the button below to set up your account and create your password.</p>
        <div style='text-align: center; margin: 30px 0;'>
            <a href='{invitationLink}' class='button'>Set Up My Account</a>
        </div>
        <p style='color: #6B7280; font-size: 14px;'>
            This invitation will expire in 7 days. If you didn't expect this email, please ignore it.
        </p>
        <p style='color: #6B7280; font-size: 12px; margin-top: 30px;'>
            Or copy this link: {invitationLink}
        </p>
    </div>
    <div class='footer'>
        <p style='margin: 0; font-size: 12px;'>Church Ministry Roster System</p>
    </div>
</body>
</html>";
    }
}
```

**3. Create Invitation Service**

```csharp
// ChurchRoster.Application/Services/InvitationService.cs
public class InvitationService : IInvitationService
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public async Task<string> CreateInvitationAsync(int userId)
    {
        var token = Guid.NewGuid().ToString("N"); // 32-character token

        var invitation = new Invitation
        {
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Invitations.Add(invitation);
        await _context.SaveChangesAsync();

        return token;
    }

    public async Task<bool> SendInvitationAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        var token = await CreateInvitationAsync(userId);
        var frontendUrl = _configuration["FrontendUrl"];
        var invitationLink = $"{frontendUrl}/accept-invitation?token={token}";

        var emailSent = await _emailService.SendInvitationEmailAsync(
            user.Email, 
            user.Name, 
            invitationLink
        );

        if (emailSent)
        {
            user.InvitationSentAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return emailSent;
    }

    public async Task<Invitation?> VerifyTokenAsync(string token)
    {
        var invitation = await _context.Invitations
            .Include(i => i.User)
            .FirstOrDefaultAsync(i => i.Token == token && !i.IsUsed);

        if (invitation == null) return null;
        if (invitation.ExpiresAt < DateTime.UtcNow) return null;

        return invitation;
    }

    public async Task<bool> AcceptInvitationAsync(string token, string password)
    {
        var invitation = await VerifyTokenAsync(token);
        if (invitation == null) return false;

        var user = invitation.User;
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
        user.IsActive = true;
        user.InvitationAcceptedAt = DateTime.UtcNow;

        invitation.IsUsed = true;
        invitation.UsedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}
```

**4. Add Invitation Endpoints**

```csharp
// ChurchRoster.Api/Endpoints/InvitationEndpoints.cs
public static class InvitationEndpoints
{
    public static void MapInvitationEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/invitations").WithTags("Invitations");

        group.MapPost("/send/{userId}", SendInvitation)
            .RequireAuthorization()
            .WithName("SendInvitation");

        group.MapGet("/verify/{token}", VerifyInvitation)
            .WithName("VerifyInvitation");

        group.MapPost("/accept", AcceptInvitation)
            .WithName("AcceptInvitation");
    }

    private static async Task<IResult> SendInvitation(
        int userId, 
        IInvitationService invitationService)
    {
        var sent = await invitationService.SendInvitationAsync(userId);
        return sent ? Results.Ok() : Results.BadRequest("Failed to send invitation");
    }

    private static async Task<IResult> VerifyInvitation(
        string token, 
        IInvitationService invitationService)
    {
        var invitation = await invitationService.VerifyTokenAsync(token);
        if (invitation == null)
            return Results.BadRequest("Invalid or expired invitation");

        return Results.Ok(new 
        { 
            userName = invitation.User.Name,
            email = invitation.User.Email,
            isValid = true
        });
    }

    private static async Task<IResult> AcceptInvitation(
        AcceptInvitationRequest request,
        IInvitationService invitationService)
    {
        var accepted = await invitationService.AcceptInvitationAsync(
            request.Token, 
            request.Password
        );

        return accepted 
            ? Results.Ok(new { message = "Account activated successfully" })
            : Results.BadRequest("Failed to accept invitation");
    }
}
```

#### **C. Frontend Implementation (React + TypeScript)**

**1. Create Accept Invitation Page**

```typescript
// frontend/src/pages/AcceptInvitation.tsx
import React, { useState, useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import apiService from '../services/api.service';

const AcceptInvitation: React.FC = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const token = searchParams.get('token');

  const [invitationData, setInvitationData] = useState<any>(null);
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    verifyToken();
  }, [token]);

  const verifyToken = async () => {
    if (!token) {
      setError('Invalid invitation link');
      setLoading(false);
      return;
    }

    try {
      const data = await apiService.get(`/invitations/verify/${token}`);
      setInvitationData(data);
    } catch (err) {
      setError('This invitation link is invalid or has expired');
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (password !== confirmPassword) {
      setError('Passwords do not match');
      return;
    }

    if (password.length < 8) {
      setError('Password must be at least 8 characters');
      return;
    }

    setLoading(true);
    try {
      await apiService.post('/invitations/accept', { token, password });
      alert('Account activated! You can now log in.');
      navigate('/login');
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to activate account');
    } finally {
      setLoading(false);
    }
  };

  if (loading) return <div className="loading">Verifying invitation...</div>;
  if (error && !invitationData) {
    return (
      <div className="login-container">
        <div className="login-card">
          <h1>Invalid Invitation</h1>
          <p>{error}</p>
          <button onClick={() => navigate('/login')}>Go to Login</button>
        </div>
      </div>
    );
  }

  return (
    <div className="login-container">
      <div className="login-card">
        <h1>Welcome, {invitationData?.userName}!</h1>
        <h2>Set Up Your Account</h2>

        {error && <div className="error-message">{error}</div>}

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>Email (verified)</label>
            <input type="email" value={invitationData?.email} disabled />
          </div>

          <div className="form-group">
            <label htmlFor="password">Create Password *</label>
            <input
              id="password"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              autoComplete="new-password"
            />
          </div>

          <div className="form-group">
            <label htmlFor="confirmPassword">Confirm Password *</label>
            <input
              id="confirmPassword"
              type="password"
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              required
              autoComplete="new-password"
            />
          </div>

          <button type="submit" disabled={loading}>
            {loading ? 'Activating...' : 'Activate Account'}
          </button>
        </form>
      </div>
    </div>
  );
};

export default AcceptInvitation;
```

**2. Update Members Page**

```typescript
// Add checkbox to Members.tsx modal
const [sendInvitation, setSendInvitation] = useState(true);

<div className="form-group">
  <label>
    <input
      type="checkbox"
      checked={sendInvitation}
      onChange={(e) => setSendInvitation(e.target.checked)}
    />
    {' '}Send invitation email (recommended)
  </label>
</div>

{!sendInvitation && (
  <div className="form-group">
    <label htmlFor="password">Temporary Password *</label>
    <input id="password" type="password" ... />
  </label>
)}
```

#### **D. Configuration**

**Backend appsettings.json:**
```json
{
  "EmailSettings": {
    "SmtpServer": "smtp-relay.brevo.com",
    "SmtpPort": 587,
    "SenderEmail": "noreply@yourchurch.com",
    "SenderName": "Church Roster System",
    "Username": "your-brevo-username",
    "Password": "your-brevo-api-key"
  },
  "FrontendUrl": "https://your-app.vercel.app"
}
```

**Frontend .env:**
```env
VITE_API_URL=https://your-api.onrender.com/api
```

---

## 📞 11. GETTING HELP

| Resource | Link |
|----------|------|
| .NET 10 Docs | https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10 |
| React Docs | https://react.dev |
| Vite PWA Plugin | https://vite-pwa-org.netlify.app |
| Supabase Docs | https://supabase.com/docs |
| Firebase Cloud Messaging | https://firebase.google.com/docs/cloud-messaging |
| Render Docs | https://render.com/docs |
| Vercel Docs | https://vercel.com/docs |

---

## 📥 11. HOW TO SAVE THIS DOCUMENT

**Option 1: Google Docs / Microsoft Word**
1. Select all text above (Ctrl+A or Cmd+A)
2. Copy (Ctrl+C or Cmd+C)
3. Open Google Docs or Word
4. Paste (Ctrl+V or Cmd+V)
5. File → Download → PDF or .docx

**Option 2: Save as Text File**
1. Select all text above
2. Copy
3. Open Notepad (Windows) or TextEdit (Mac)
4. Paste
5. File → Save As → `Development_Guide.txt`

**Option 3: Save as Markdown (for tech teams)**
1. Copy all text
2. Paste into a new file named `development_guide.md`
3. Open with any code editor or Markdown viewer

---

> 🙏 **You're all set!**  
> This development guide gives you a clear path from **zero to launch** in 4-6 weeks.  
> Start with **Week 1: Backend Foundation** and follow the checklist.  
> 
> **Remember:** Build the MVP first (assign tasks + accept/reject), then add polish (reports, notifications).
> 
> Good luck with your project! Feel free to come back if you need code samples or troubleshooting help.

*Document Version: 1.0 | Last Updated: March 2026 | Prepared for: Development Team*