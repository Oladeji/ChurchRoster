# 📄 Church Ministry Rostering System  
## Development Guide & Roadmap  
**Version 1.0**  
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

### **Week 5: Notifications & Reports**
| Day | Task | Deliverable |
|-----|------|-------------|
| 1 | Set up Firebase project & get VAPID keys | Firebase ready |
| 2 | Implement Push Notification in Frontend | Can receive pushes |
| 3 | Implement Push Notification in Backend | Can send pushes |
| 4 | Set up Brevo email service | Can send emails |
| 5 | Build Printable Report (PDF generation) | Can print roster |
| 6-7 | Testing & Bug Fixes | Notifications working |

### **Week 6: Polish & Launch**
| Day | Task | Deliverable |
|-----|------|-------------|
| 1 | Implement Accept/Reject workflow | Members can respond |
| 2 | Implement Past Event auto-status update | Completed/Expired logic |
| 3 | Security audit (API protection, role checks) | Secure system |
| 4 | User testing with real church members | Feedback collected |
| 5 | Fix bugs from testing | Stable system |
| 6-7 | Go Live & Training | System launched |

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

---

## 📞 10. GETTING HELP

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