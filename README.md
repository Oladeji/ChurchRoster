# 📚 Church Ministry Rostering System

A modern web-based Progressive Web Application (PWA) for managing church ministry schedules, task assignments, and member qualifications.

[![.NET Version](https://img.shields.io/badge/.NET-10.0-purple)](https://dotnet.microsoft.com/)
[![React Version](https://img.shields.io/badge/React-19.2-blue)](https://react.dev/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.9-blue)](https://www.typescriptlang.org/)
[![License](https://img.shields.io/badge/License-Church_Use-green)](LICENSE)

## 🎯 Project Overview

The Church Ministry Rostering System helps church administrators efficiently manage ministry task assignments while ensuring:
- ✅ Members are qualified for their assigned tasks
- ✅ Fair distribution of responsibilities
- ✅ No scheduling conflicts
- ✅ Timely notifications to members
- ✅ Mobile accessibility via PWA
- ✅ Zero monthly hosting costs

## ✨ Key Features

### For Administrators
- 📋 Manage church members and their skills
- 📅 Create and assign ministry tasks
- 🔔 Automated email and push notifications
- 📊 Generate printable ministry schedules
- ⚖️ Track fairness with monthly limits
- 🛡️ Override qualification rules when needed

### For Members
- 📱 Install as mobile app (PWA)
- 🔔 Receive push notifications for new assignments
- ✅ Accept or reject assigned tasks
- 📆 View all upcoming assignments
- 📧 Respond via email links

## 🏗️ Project Structure

```
church-roster-system/
├── backend/                    # .NET 10 Web API
│   ├── ChurchRoster.Api/       # API Controllers & Startup
│   ├── ChurchRoster.Application/ # Business Logic
│   ├── ChurchRoster.Core/      # Domain Models
│   ├── ChurchRoster.Infrastructure/ # Data Access
│   ├── Directory.Packages.props # Central Package Management
│   ├── Dockerfile              # Container configuration
│   └── README.md
│
├── frontend/                   # React + TypeScript (Vite)
│   ├── src/
│   │   ├── components/         # Reusable UI components
│   │   ├── pages/              # Route components
│   │   ├── services/           # API integration
│   │   ├── hooks/              # Custom React hooks
│   │   ├── context/            # Global state management
│   │   ├── types/              # TypeScript definitions
│   │   └── utils/              # Helper functions
│   ├── public/
│   │   └── manifest.json       # PWA manifest
│   ├── vite.config.ts          # Vite + PWA configuration
│   └── README.md
│
├── database/                   # PostgreSQL scripts
│   ├── scripts/
│   │   ├── 01_create_schema.sql    # Database schema
│   │   ├── 02_create_views.sql     # Database views
│   │   └── 03_create_functions.sql # Stored procedures
│   ├── seed-data/
│   │   └── 01_initial_seed.sql     # Default data
│   └── README.md
│
├── docs/                       # Documentation
│   ├── Requirements_Document.md    # Functional requirements
│   ├── Technical_Architecture.md   # System architecture
│   ├── Development_Guide.md        # Development roadmap
│   └── README.md
│
└── README.md                   # This file
```

## 🚀 Quick Start

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/)
- [PostgreSQL 15+](https://www.postgresql.org/) or [Supabase](https://supabase.com) account
- [Git](https://git-scm.com/)

### 1. Clone Repository

```bash
git clone https://github.com/your-org/church-roster-system.git
cd church-roster-system
```

### 2. Set Up Database

**Option A: Using Supabase (Recommended)**
1. Create account at [supabase.com](https://supabase.com)
2. Create new project
3. Run scripts from `database/scripts/` in Supabase SQL Editor
4. Get connection string from Settings → Database

**Option B: Using Local PostgreSQL**
```bash
psql -U postgres -c "CREATE DATABASE church_roster;"
psql -U postgres -d church_roster -f database/scripts/01_create_schema.sql
psql -U postgres -d church_roster -f database/scripts/02_create_views.sql
psql -U postgres -d church_roster -f database/scripts/03_create_functions.sql
psql -U postgres -d church_roster -f database/seed-data/01_initial_seed.sql
```

### 3. Run Backend

```bash
cd backend/ChurchRoster.Api

# Update appsettings.json with your connection string
# ConnectionStrings:DefaultConnection

dotnet restore
dotnet run

# API runs at https://localhost:7xxx
```

### 4. Run Frontend

```bash
cd frontend

# Install dependencies
npm install

# Create .env file
cp .env.example .env

# Update .env with your API URL
# VITE_API_URL=https://localhost:7xxx/api

npm run dev

# App runs at http://localhost:3000
```

### 5. Access the Application

- **Frontend**: http://localhost:3000
- **Backend API**: https://localhost:7xxx
- **Swagger UI**: https://localhost:7xxx/swagger

**Default Admin Login**:
- Email: admin@church.com
- Password: Admin@123 (⚠️ Change immediately!)

## 📖 Documentation

Comprehensive documentation is available in the `docs/` folder:

| Document | Description | Link |
|----------|-------------|------|
| **Requirements** | What the system does | [Requirements_Document.md](docs/Requirements_Document.md) |
| **Architecture** | How the system is built | [Technical_Architecture.md](docs/Technical_Architecture.md) |
| **Development Guide** | Step-by-step development | [Development_Guide.md](docs/Development_Guide.md) |

### Quick Documentation Links

- 📝 [User Roles & Permissions](docs/Requirements_Document.md#5-user-roles)
- 🎯 [Functional Requirements](docs/Requirements_Document.md#3-functional-requirements)
- 🏛️ [System Architecture](docs/Technical_Architecture.md#2-system-architecture)
- 💻 [Technology Stack](docs/Technical_Architecture.md#3-technology-stack)
- 🛠️ [Development Roadmap](docs/Development_Guide.md#3-development-roadmap-week-by-week)
- 🚀 [Deployment Guide](docs/Development_Guide.md#7-deployment-checklist)

## 🛠️ Technology Stack

### Backend
- **.NET 10** - Modern C# web framework
- **ASP.NET Core Web API** - RESTful API
- **Entity Framework Core** - ORM for PostgreSQL
- **JWT Authentication** - Secure token-based auth
- **BCrypt** - Password hashing
- **FirebaseAdmin** - Push notifications
- **MailKit** - Email notifications

### Frontend
- **React 19** - UI library
- **TypeScript** - Type-safe JavaScript
- **Vite** - Fast build tool
- **React Router** - Client-side routing
- **Axios** - HTTP client
- **Firebase** - Push notifications
- **PWA** - Installable web app

### Database
- **PostgreSQL 15+** - Relational database
- **Supabase** - Managed PostgreSQL hosting

### Infrastructure
- **Vercel** - Frontend hosting (Free tier)
- **Render** - Backend hosting (Free tier)
- **Supabase** - Database hosting (Free tier)
- **Firebase Cloud Messaging** - Push notifications (Free tier)
- **Brevo** - Email service (Free tier)

## 🔧 Development

### Backend Development

```bash
cd backend/ChurchRoster.Api

# Run in development mode
dotnet run

# Run tests
dotnet test

# Create migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Build for production
dotnet publish -c Release
```

### Frontend Development

```bash
cd frontend

# Run development server
npm run dev

# Run linter
npm run lint

# Build for production
npm run build

# Preview production build
npm run preview
```

### Database Development

```sql
-- Check available members for a task
SELECT * FROM get_available_members(task_id, '2026-04-15');

-- View upcoming assignments
SELECT * FROM vw_upcoming_assignments;

-- Check monthly assignment counts
SELECT * FROM vw_monthly_assignment_count;

-- Expire old assignments (run daily)
CALL expire_past_assignments();
```

## 🧪 Testing

### Backend Tests
```bash
cd backend
dotnet test --logger "console;verbosity=detailed"
```

### Frontend Tests
```bash
cd frontend
npm run test
```

### Manual Testing Checklist
- [ ] User can login
- [ ] Admin can create member
- [ ] Admin can assign skills to member
- [ ] Admin can create task
- [ ] Admin can assign task to qualified member
- [ ] System prevents assigning unqualified member
- [ ] System detects scheduling conflicts
- [ ] Member receives notification
- [ ] Member can accept/reject assignment
- [ ] Calendar displays correctly
- [ ] Report can be generated
- [ ] PWA can be installed on mobile

## 🚀 Deployment

### Deploy Backend to Render

1. Create account at [render.com](https://render.com)
2. Create new Web Service
3. Connect GitHub repository
4. Configure:
   - Build Command: `docker build`
   - Start Command: Auto-detected from Dockerfile
5. Add environment variables:
   - `ConnectionStrings__DefaultConnection`
   - `JwtSettings__Secret`
   - `Firebase__ProjectId`
   - `SmtpSettings__Host`, `SmtpSettings__Username`, `SmtpSettings__Password`
6. Deploy

### Deploy Frontend to Vercel

1. Create account at [vercel.com](https://vercel.com)
2. Import GitHub repository
3. Configure:
   - Framework: Vite
   - Build Command: `npm run build`
   - Output Directory: `dist`
4. Add environment variables:
   - `VITE_API_URL`
   - `VITE_FIREBASE_API_KEY`
   - `VITE_FIREBASE_PROJECT_ID`
   - (All Firebase config)
5. Deploy

### Database Setup (Supabase)

1. Create project at [supabase.com](https://supabase.com)
2. Go to SQL Editor
3. Run scripts in order:
   - `database/scripts/01_create_schema.sql`
   - `database/scripts/02_create_views.sql`
   - `database/scripts/03_create_functions.sql`
   - `database/seed-data/01_initial_seed.sql`
4. Get connection string from Settings → Database
5. Add to backend environment variables

## 📱 Installing as PWA

### iOS
1. Open app in Safari
2. Tap Share button
3. Select "Add to Home Screen"
4. Tap "Add"

### Android
1. Open app in Chrome
2. Tap menu (⋮)
3. Select "Install App"
4. Tap "Install"

## 🔐 Security

- ✅ All API endpoints require authentication
- ✅ Passwords hashed with BCrypt
- ✅ JWT tokens expire after 24 hours
- ✅ HTTPS enforced on all connections
- ✅ CORS configured for frontend domain only
- ✅ SQL injection prevented (parameterized queries)
- ✅ Environment variables never committed to Git

**⚠️ Important**: Change the default admin password immediately after setup!

## 📊 Database Schema

### Core Tables
- **users** - Church members and administrators
- **skills** - Ministry skills (e.g., CanLeadPreaching)
- **user_skills** - Member skill assignments
- **tasks** - Ministry tasks (e.g., Lead Bible Study)
- **assignments** - Task assignments to members

### Key Views
- **vw_member_skills** - Members with their skills
- **vw_upcoming_assignments** - Future assignments
- **vw_monthly_assignment_count** - Member workload tracking

See [database/README.md](database/README.md) for complete schema documentation.

## 🤝 Contributing

### Development Workflow

1. Create feature branch: `git checkout -b feature/your-feature`
2. Make changes and test locally
3. Commit with meaningful message: `git commit -m "Add feature X"`
4. Push to GitHub: `git push origin feature/your-feature`
5. Create Pull Request
6. Wait for review and merge

### Code Standards

**Backend (C#)**
- Follow Microsoft C# conventions
- Use async/await for I/O operations
- Write XML documentation for public APIs
- Keep controllers thin, logic in services

**Frontend (TypeScript)**
- Follow React best practices
- Use functional components with hooks
- Type everything (avoid `any`)
- Keep components small and focused

## 🐛 Troubleshooting

### Backend won't start
- Check connection string in appsettings.json
- Verify PostgreSQL is running
- Check port 5000/7000 is not in use

### Frontend won't connect to API
- Verify VITE_API_URL in .env
- Check backend is running
- Check CORS configuration

### Database connection fails
- Verify Supabase connection string
- Check firewall settings
- Test connection with psql

### Push notifications not working
- Verify Firebase configuration
- Check VAPID keys
- Ensure user granted notification permission
- iOS: App must be installed to home screen

## 📞 Support

- **Documentation**: See `docs/` folder
- **Issues**: Create GitHub issue
- **Email**: support@church.com

## 📄 License

This project is for church ministry use. See [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Development guide based on modern .NET and React best practices
- Architecture follows Clean Architecture principles
- Free tier hosting made possible by Vercel, Render, and Supabase

---

## 📈 Project Status

- [x] Requirements defined
- [x] Architecture designed
- [x] Backend structure created
- [x] Frontend structure created
- [x] Database schema designed
- [ ] Core features implementation
- [ ] Testing
- [ ] Deployment
- [ ] User acceptance testing
- [ ] Production launch

**Current Phase**: Development (Week 1)  
**Target Launch**: 6 weeks from start date

---

*Last Updated: March 31, 2026*  
*Version: 1.0*
