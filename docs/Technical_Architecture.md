# Church Ministry Rostering System
## Technical Architecture Document
**Version: 1.1**  
**Last Updated: March 31, 2026**

---

## 📋 Table of Contents
1. [Architecture Overview](#architecture-overview)
2. [System Architecture](#system-architecture)
3. [Technology Stack](#technology-stack)
4. [Infrastructure Architecture](#infrastructure-architecture)
5. [Application Architecture](#application-architecture)
6. [Data Architecture](#data-architecture)
7. [Security Architecture](#security-architecture)
8. [Integration Architecture](#integration-architecture)
9. [Deployment Architecture](#deployment-architecture)
10. [Monitoring and Logging](#monitoring-and-logging)

---

## 1. Architecture Overview

### 1.1 Architecture Style
The Church Ministry Rostering System follows a **modern three-tier architecture** with clear separation of concerns:

- **Presentation Layer**: React PWA frontend
- **Application Layer**: .NET 10 Web API
- **Data Layer**: PostgreSQL database

### 1.2 Architecture Principles

1. **Separation of Concerns**: Clean Architecture with distinct layers
2. **Dependency Injection**: Loose coupling via DI containers
3. **API-First**: RESTful API as the single source of truth
4. **Mobile-First**: PWA for optimal mobile experience
5. **Cloud-Native**: Designed for serverless/container deployment
6. **Zero-Cost**: Leverage free tier services

### 1.3 Architecture Diagram

```
┌─────────────────────────────────────────────────────────┐
│                    CLIENT DEVICES                        │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐              │
│  │ Desktop  │  │  Mobile  │  │  Tablet  │              │
│  │ Browser  │  │ (PWA)    │  │ Browser  │              │
│  └─────┬────┘  └─────┬────┘  └─────┬────┘              │
└────────┼─────────────┼─────────────┼────────────────────┘
         │             │             │
         └─────────────┼─────────────┘
                       │ HTTPS
         ┌─────────────▼─────────────┐
         │   CDN / VERCEL (Frontend) │
         │   React PWA Application   │
         └─────────────┬─────────────┘
                       │ REST API
         ┌─────────────▼─────────────┐
         │   RENDER (Backend)        │
         │   .NET 10 Web API         │
         │   ┌───────────────────┐   │
         │   │  Controllers      │   │
         │   │  Services         │   │
         │   │  Repositories     │   │
         │   └───────────────────┘   │
         └─────────────┬─────────────┘
                       │
         ┌─────────────▼─────────────┐
         │   SUPABASE (Database)     │
         │   PostgreSQL              │
         └───────────────────────────┘
                       │
         ┌─────────────▼─────────────┐
         │   EXTERNAL SERVICES       │
         │   ┌──────────┬──────────┐ │
         │   │ Firebase │  Brevo   │ │
         │   │   (FCM)  │  (SMTP)  │ │
         │   └──────────┴──────────┘ │
         └───────────────────────────┘
```

---

## 2. System Architecture

### 2.1 High-Level Components

| Component | Technology | Purpose | Hosting |
|-----------|-----------|---------|---------|
| **Frontend** | React 18+ TypeScript | User interface | Vercel (Free) |
| **Backend API** | .NET 10 Web API | Business logic | Render (Free) |
| **Database** | PostgreSQL | Data storage | Supabase (Free) |
| **Notifications** | Firebase Cloud Messaging | Push notifications | Firebase (Free) |
| **Email** | Brevo SMTP | Email delivery | Brevo (Free) |
| **File Storage** | Vercel CDN | Static assets | Vercel (Free) |

### 2.2 Communication Patterns

- **Frontend ↔ Backend**: REST over HTTPS, JSON payload
- **Backend ↔ Database**: PostgreSQL protocol, connection pooling
- **Backend ↔ Firebase**: Firebase Admin SDK
- **Backend ↔ Email**: SMTP over TLS

---

## 3. Technology Stack

### 3.1 Backend Stack

```yaml
Platform: .NET 10
Language: C# 12
Framework: ASP.NET Core Web API

Dependencies:
  - Npgsql.EntityFrameworkCore.PostgreSQL (10.0.0)
  - Microsoft.EntityFrameworkCore (10.0.0)
  - Microsoft.AspNetCore.Authentication.JwtBearer (10.0.0)
  - FirebaseAdmin (3.5.0)
  - MailKit (4.10.0)
  - BCrypt.Net-Next (4.0.3)
  - FluentValidation (11.11.0)
  - AutoMapper (14.0.0)
  - Swashbuckle.AspNetCore (7.2.0)

Architecture Pattern: Clean Architecture
  - ChurchRoster.Api (Presentation)
  - ChurchRoster.Application (Business Logic)
  - ChurchRoster.Core (Domain Entities)
  - ChurchRoster.Infrastructure (Data Access)
```

### 3.2 Frontend Stack

```yaml
Platform: Node.js 18+
Language: TypeScript 5.9
Framework: React 19.2
Build Tool: Vite 8.0

Dependencies:
  - react-router-dom (7.5.0)
  - axios (1.7.9)
  - firebase (11.2.0)
  - vite-plugin-pwa (latest)

Architecture Pattern: Feature-based
  - components/ (Reusable UI)
  - pages/ (Route components)
  - services/ (API integration)
  - context/ (Global state)
  - hooks/ (Custom hooks)
  - types/ (TypeScript definitions)
```

### 3.3 Database Stack

```yaml
Database: PostgreSQL 15+
Hosting: Supabase
Schema Management: SQL Scripts + EF Core Migrations

Features:
  - ACID transactions
  - Referential integrity
  - Stored procedures
  - Database views
  - Triggers
  - Indexes for performance
```

---

## 4. Infrastructure Architecture

### 4.1 Hosting Strategy

#### Frontend (Vercel)
```
- Automatic HTTPS
- Global CDN
- Edge caching
- Instant rollbacks
- Preview deployments
- Custom domain support
- Free tier: 100GB bandwidth/month
```

#### Backend (Render)
```
- Docker container deployment
- Automatic scaling
- HTTPS included
- Health checks
- Log streaming
- Free tier: 750 hours/month
```

#### Database (Supabase)
```
- Managed PostgreSQL
- Automatic backups
- Connection pooling
- Real-time subscriptions (optional)
- Free tier: 500MB storage, 2GB bandwidth
```

### 4.2 Network Architecture

```
                    INTERNET
                       │
                       ▼
        ┌──────────────────────────┐
        │  Cloudflare / Vercel CDN │
        │  (SSL Termination)       │
        └──────────────┬───────────┘
                       │
        ┌──────────────▼───────────┐
        │   Load Balancer          │
        │   (Auto-scaling)         │
        └──────────────┬───────────┘
                       │
        ┌──────────────▼───────────┐
        │   Application Servers    │
        │   (Render Containers)    │
        └──────────────┬───────────┘
                       │
        ┌──────────────▼───────────┐
        │   Database Cluster       │
        │   (Supabase PostgreSQL)  │
        └──────────────────────────┘
```

---

## 5. Application Architecture

### 5.1 Backend Architecture (Clean Architecture)

```
ChurchRoster.Api/
├── Controllers/           # HTTP request handling
│   ├── AuthController.cs
│   ├── MembersController.cs
│   ├── TasksController.cs
│   ├── AssignmentsController.cs
│   └── SkillsController.cs
├── Middleware/            # Cross-cutting concerns
│   ├── ExceptionHandlingMiddleware.cs
│   └── RequestLoggingMiddleware.cs
└── Program.cs             # Application entry point

ChurchRoster.Application/
├── Services/              # Business logic
│   ├── AuthService.cs
│   ├── MemberService.cs
│   ├── AssignmentService.cs
│   └── NotificationService.cs
├── DTOs/                  # Data transfer objects
├── Interfaces/            # Service contracts
└── Validators/            # Input validation

ChurchRoster.Core/
├── Entities/              # Domain models
│   ├── User.cs
│   ├── Skill.cs
│   ├── Task.cs
│   └── Assignment.cs
├── Enums/                 # Domain enums
└── Interfaces/            # Repository contracts

ChurchRoster.Infrastructure/
├── Data/                  # EF Core context
│   └── AppDbContext.cs
├── Repositories/          # Data access
└── Services/              # External integrations
    ├── EmailService.cs
    └── PushNotificationService.cs
```

### 5.2 Frontend Architecture

```
src/
├── components/            # Reusable UI components
│   ├── ProtectedRoute.tsx
│   ├── Calendar.tsx
│   └── AssignmentCard.tsx
├── pages/                 # Route components
│   ├── Login.tsx
│   ├── Dashboard.tsx
│   ├── Members.tsx
│   ├── Assignments.tsx
│   └── Calendar.tsx
├── services/              # API integration
│   ├── api.service.ts     # Base HTTP client
│   ├── auth.service.ts
│   ├── assignment.service.ts
│   └── firebase.service.ts
├── context/               # Global state
│   └── AuthContext.tsx
├── hooks/                 # Custom hooks
│   ├── useAssignments.ts
│   └── useMembers.ts
├── types/                 # TypeScript types
│   └── index.ts
└── utils/                 # Helper functions
    └── helpers.ts
```

---

## 6. Data Architecture

### 6.1 Entity Relationship Diagram

```
┌─────────────────┐         ┌─────────────────┐
│     Users       │         │     Skills      │
├─────────────────┤         ├─────────────────┤
│ user_id (PK)    │─────────│ skill_id (PK)   │
│ name            │    ╲    │ skill_name      │
│ email (UQ)      │     ╲   │ description     │
│ password_hash   │      ╲  │ is_active       │
│ role            │       ╲ └─────────────────┘
│ monthly_limit   │        ╲
│ device_token    │         ╲┌────────────────┐
│ is_active       │          │  UserSkills    │
└─────────────────┘          ├────────────────┤
         │                   │ user_id (FK)   │
         │                   │ skill_id (FK)  │
         │                   │ assigned_date  │
         │                   └────────────────┘
         │
         │
┌────────▼──────────┐       ┌─────────────────┐
│   Assignments     │       │     Tasks       │
├───────────────────┤       ├─────────────────┤
│ assignment_id(PK) │───────│ task_id (PK)    │
│ task_id (FK)      │       │ task_name       │
│ user_id (FK)      │       │ frequency       │
│ event_date        │       │ day_rule        │
│ status            │       │ required_skill  │
│ rejection_reason  │       │ is_restricted   │
│ is_override       │       └─────────────────┘
│ assigned_by (FK)  │
│ created_at        │
│ updated_at        │
└───────────────────┘
```

### 6.2 Data Flow

```
1. User Login
   ┌──────┐   POST /auth/login    ┌──────┐   Query    ┌──────────┐
   │Client│──────────────────────>│ API  │───────────>│PostgreSQL│
   └──────┘                        └──────┘            └──────────┘
      ▲                               │                      │
      │      JWT Token + User Data    │   User Record        │
      └───────────────────────────────┴──────────────────────┘

2. Create Assignment
   ┌──────┐   POST /assignments   ┌──────┐
   │Client│──────────────────────>│ API  │
   └──────┘                        └──┬───┘
                                     │
                    ┌────────────────┼────────────────┐
                    │                │                │
                    ▼                ▼                ▼
            ┌──────────┐      ┌──────────┐    ┌──────────┐
            │PostgreSQL│      │ Firebase │    │  Brevo   │
            │  (Save)  │      │  (Push)  │    │ (Email)  │
            └──────────┘      └──────────┘    └──────────┘
```

---

## 7. Security Architecture

### 7.1 Authentication Flow

```
1. User submits credentials
   ┌──────┐
   │Client│
   └───┬──┘
       │ POST /auth/login {email, password}
       ▼
   ┌────────┐
   │   API  │
   └───┬────┘
       │ 1. Validate credentials
       │ 2. Verify BCrypt hash
       │ 3. Generate JWT token
       ▼
   ┌──────────┐
   │PostgreSQL│
   └──────────┘

2. Client stores JWT token in localStorage
3. All subsequent requests include:
   Authorization: Bearer <token>

4. API validates token on each request:
   - Check signature
   - Verify expiration
   - Extract user claims
```

### 7.2 Authorization Model

```
Role-Based Access Control (RBAC)

Admin Role:
  - All endpoints accessible
  - Can modify any data
  - Can override business rules

Member Role:
  - Limited endpoints
  - Can only view/modify own data
  - Cannot access admin functions

Endpoint Protection:
  [Authorize(Roles = "Admin")]
  [Authorize(Roles = "Admin,Member")]
```

### 7.3 Security Measures

| Layer | Security Measure | Implementation |
|-------|-----------------|----------------|
| **Transport** | HTTPS/TLS 1.3 | Automatic via Vercel/Render |
| **Authentication** | JWT + BCrypt | ASP.NET Identity + BCrypt.Net |
| **Authorization** | Role-based | Custom middleware |
| **Input Validation** | FluentValidation | Server-side validation |
| **SQL Injection** | Parameterized queries | Entity Framework Core |
| **XSS Protection** | React escaping | React default behavior |
| **CSRF Protection** | SameSite cookies | ASP.NET Core defaults |
| **Rate Limiting** | Request throttling | Render platform feature |
| **Secrets Management** | Environment variables | Never committed to Git |

---

## 8. Integration Architecture

### 8.1 Firebase Cloud Messaging (FCM)

```yaml
Purpose: Push notifications to mobile devices

Integration Flow:
  1. Frontend requests notification permission
  2. Firebase generates device token
  3. Token stored in user.device_token
  4. Backend sends notifications via Firebase Admin SDK

Configuration:
  - Firebase project ID
  - Service account JSON
  - VAPID keys for web push

Message Format:
  {
    "notification": {
      "title": "New Assignment",
      "body": "You have been assigned: Lead Prayer"
    },
    "data": {
      "assignmentId": "123",
      "taskName": "Lead Prayer",
      "eventDate": "2026-04-15"
    }
  }
```

### 8.2 Email Service (Brevo/MailKit)

```yaml
Purpose: Email notifications for assignments

Integration Flow:
  1. Assignment created in database
  2. Backend composes email
  3. Email sent via SMTP
  4. Delivery status logged

Configuration:
  - SMTP host: smtp-relay.brevo.com
  - Port: 587 (TLS)
  - Authentication: API key

Email Template:
  Subject: New Ministry Assignment
  Body: HTML template with task details
  CTA: Accept/Reject buttons (links to app)
```

### 8.3 API Contract

```yaml
Base URL: https://api.church-roster.com
Content-Type: application/json
Authentication: Bearer <JWT>

Endpoints:
  Authentication:
    POST   /api/auth/login
    POST   /api/auth/register

  Members:
    GET    /api/members
    GET    /api/members/{id}
    POST   /api/members
    PUT    /api/members/{id}
    DELETE /api/members/{id}
    GET    /api/members/{id}/skills
    POST   /api/members/{id}/skills
    DELETE /api/members/{id}/skills/{skillId}

  Tasks:
    GET    /api/tasks
    GET    /api/tasks/{id}
    POST   /api/tasks
    PUT    /api/tasks/{id}
    DELETE /api/tasks/{id}

  Assignments:
    GET    /api/assignments
    GET    /api/assignments/{id}
    POST   /api/assignments
    PUT    /api/assignments/{id}
    PATCH  /api/assignments/{id}/accept
    PATCH  /api/assignments/{id}/reject
    DELETE /api/assignments/{id}

Error Response Format:
  {
    "error": "Error message",
    "statusCode": 400,
    "timestamp": "2026-03-31T10:00:00Z"
  }
```

---

## 9. Deployment Architecture

### 9.1 CI/CD Pipeline

```
┌─────────────┐
│   GitHub    │
│ (Repository)│
└──────┬──────┘
       │ git push
       ▼
┌─────────────────────────────────┐
│   GitHub Actions                │
│   ┌─────────────────────────┐   │
│   │  1. Checkout code       │   │
│   │  2. Run tests           │   │
│   │  3. Build application   │   │
│   │  4. Build Docker image  │   │
│   └─────────────────────────┘   │
└──────┬────────────┬─────────────┘
       │            │
       │ Backend    │ Frontend
       ▼            ▼
┌─────────┐    ┌──────────┐
│ Render  │    │ Vercel   │
│ (Deploy)│    │ (Deploy) │
└─────────┘    └──────────┘
```

### 9.2 Backend Deployment (Render)

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ChurchRoster.Api.dll"]
```

### 9.3 Frontend Deployment (Vercel)

```bash
# Build command
npm run build

# Output directory
dist/

# Environment variables (set in Vercel dashboard)
VITE_API_URL=https://api.church-roster.com
VITE_FIREBASE_API_KEY=xxx
VITE_FIREBASE_PROJECT_ID=xxx
```

### 9.4 Database Migrations

```bash
# Development
dotnet ef migrations add MigrationName
dotnet ef database update

# Production (Supabase SQL Editor)
1. Export migration SQL
2. Review changes
3. Execute in Supabase dashboard
4. Verify schema
```

---

## 10. Monitoring and Logging

### 10.1 Application Monitoring

```yaml
Frontend (Vercel Analytics):
  - Page load times
  - Core Web Vitals
  - Error rates
  - User geography

Backend (Render Metrics):
  - CPU usage
  - Memory usage
  - Response times
  - Error rates
  - Request volume

Database (Supabase Metrics):
  - Query performance
  - Connection pool usage
  - Storage usage
  - Slow query log
```

### 10.2 Logging Strategy

```csharp
// Backend logging levels
LogLevel.Critical  // System failures
LogLevel.Error     // Request failures
LogLevel.Warning   // Business rule violations
LogLevel.Information // Business events
LogLevel.Debug     // Development only

// Structured logging
logger.LogInformation(
    "Assignment created: {AssignmentId} for {UserId} on {Date}",
    assignment.Id, assignment.UserId, assignment.EventDate
);
```

### 10.3 Error Tracking

```yaml
Frontend:
  - Browser console errors
  - React error boundaries
  - API error responses
  - Network failures

Backend:
  - Exception middleware
  - Stack traces
  - Request context
  - User information

Database:
  - PostgreSQL logs
  - Query errors
  - Connection issues
  - Deadlocks
```

---

## 11. Performance Optimization

### 11.1 Frontend Optimization

- Code splitting (React.lazy)
- Image optimization (WebP)
- Bundle size optimization (tree shaking)
- Service worker caching (PWA)
- CDN delivery (Vercel Edge)

### 11.2 Backend Optimization

- Database query optimization (indexes)
- Connection pooling
- Response caching
- Async/await for I/O operations
- Pagination for large datasets

### 11.3 Database Optimization

```sql
-- Indexes for common queries
CREATE INDEX idx_assignments_user_date ON assignments(user_id, event_date);
CREATE INDEX idx_assignments_status ON assignments(status);

-- Database views for complex queries
CREATE VIEW vw_upcoming_assignments AS
SELECT ... FROM assignments WHERE event_date >= CURRENT_DATE;
```

---

## 12. Disaster Recovery

### 12.1 Backup Strategy

```yaml
Database (Supabase):
  - Automatic daily backups
  - 7-day retention
  - Point-in-time recovery
  - Manual backup on-demand

Application Code:
  - Git version control
  - GitHub repository
  - Multiple deployment environments
  - Rollback capability

Configuration:
  - Environment variables backed up
  - Secrets in password manager
  - Infrastructure as code
```

### 12.2 Recovery Procedures

```
Database Failure:
  1. Restore from Supabase backup
  2. Verify data integrity
  3. Update connection strings
  4. Restart application

Application Failure:
  1. Rollback to previous deployment
  2. Check logs for errors
  3. Fix issue
  4. Redeploy

Complete System Failure:
  1. Restore database from backup
  2. Redeploy backend to Render
  3. Redeploy frontend to Vercel
  4. Verify all services
  5. Test end-to-end functionality
```

---

## 13. Cost Analysis

### 13.1 Free Tier Limits

| Service | Free Tier | Estimated Usage | Sufficient? |
|---------|-----------|-----------------|-------------|
| **Vercel** | 100GB bandwidth | ~5GB/month | ✅ Yes |
| **Render** | 750 hours | 730 hours/month | ✅ Yes |
| **Supabase** | 500MB storage | ~100MB | ✅ Yes |
| **Firebase** | 10K messages/day | ~100/day | ✅ Yes |
| **Brevo** | 300 emails/day | ~20/day | ✅ Yes |

### 13.2 Scaling Considerations

If usage exceeds free tier:
- Vercel Pro: $20/month (1TB bandwidth)
- Render Standard: $7/month per service
- Supabase Pro: $25/month (8GB storage)
- Firebase Blaze: Pay-as-you-go
- Brevo Lite: $25/month (10K emails)

---

## Appendix A: Technology Decision Matrix

| Requirement | Options Considered | Selected | Reason |
|-------------|-------------------|----------|---------|
| **Backend Framework** | Node.js, .NET, Python | .NET 10 | Performance, type safety, C# expertise |
| **Frontend Framework** | React, Vue, Angular | React | Large ecosystem, PWA support |
| **Database** | PostgreSQL, MySQL, MongoDB | PostgreSQL | ACID compliance, free hosting |
| **Hosting (Backend)** | Render, Heroku, Railway | Render | Free tier, Docker support |
| **Hosting (Frontend)** | Vercel, Netlify, GitHub Pages | Vercel | PWA support, edge network |
| **Notifications** | FCM, OneSignal, Pusher | FCM | Free, reliable, cross-platform |

---

*Document Version: 1.1*  
*Last Updated: March 31, 2026*  
*Status: Approved*
