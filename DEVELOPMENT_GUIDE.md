# Church Roster System - Development Guide

## Table of Contents
1. [Project Overview](#project-overview)
2. [Technology Stack](#technology-stack)
3. [Architecture](#architecture)
4. [Setup Instructions](#setup-instructions)
5. [Development Workflow](#development-workflow)
6. [Feature Implementation Guide](#feature-implementation-guide)
7. [Testing](#testing)
8. [Deployment](#deployment)

## Project Overview

The Church Roster Management System is a full-stack application designed to manage church member assignments for ministry tasks. It includes skills management, task scheduling, and automated assignment workflows.

### Key Features
- **Skills Management**: Admin can create and manage skills/qualifications
- **Tasks Management**: Admin can create tasks and assign required skills
- **Member Management**: Manage members and assign skills to them
- **Intelligent Assignment**: System filters qualified members based on task requirements
- **Email Invitations**: Send invitation emails with Brevo
- **Role-Based Access**: Admin and Member roles

## Technology Stack

### Frontend
- **React 19.2** with TypeScript
- **Vite 8.0** - Build tool with OXC parser
- **Tailwind CSS v4** - Utility-first CSS framework
- **Heroicons v2** - Icon library
- **React Router v6** - Client-side routing
- **Firebase** - Push notifications

### Backend
- **.NET 10** - Minimal APIs
- **Entity Framework Core** - ORM
- **PostgreSQL** - Database (via Supabase)
- **Brevo** - Email service
- **JWT** - Authentication

### Infrastructure
- **Supabase** - PostgreSQL hosting
- **GitHub** - Version control
- **Vercel/Netlify** - Frontend hosting (optional)
- **Azure/AWS** - Backend hosting (optional)

## Architecture

### Backend Architecture

```
ChurchRoster.Api/
├── Endpoints/V1/         # Minimal API endpoints
│   ├── MemberEndpoints.cs
│   ├── SkillEndpoints.cs
│   ├── TaskEndpoints.cs
│   ├── AssignmentEndpoints.cs
│   └── InvitationEndpoints.cs
├── Program.cs            # Application entry point
└── appsettings.json      # Configuration

ChurchRoster.Application/
├── DTOs/                 # Data Transfer Objects
│   ├── Members/
│   ├── Skills/
│   ├── Tasks/
│   ├── Assignments/
│   └── Invitations/
├── Interfaces/           # Service interfaces
└── Services/             # Business logic

ChurchRoster.Core/
└── Entities/             # Domain models
    ├── User.cs
    ├── Skill.cs
    ├── UserSkill.cs
    ├── Task.cs
    ├── Assignment.cs
    └── Invitation.cs

ChurchRoster.Infrastructure/
└── Data/
    └── AppDbContext.cs   # EF Core DbContext
```

### Frontend Architecture

```
frontend/src/
├── components/           # Reusable components
│   ├── ProtectedRoute.tsx
│   └── [other components]
├── context/              # React context
│   └── AuthContext.tsx
├── pages/                # Page components
│   ├── Dashboard.tsx
│   ├── SkillsPage.tsx
│   ├── TasksPage.tsx
│   ├── Members.tsx
│   ├── AssignmentsPage.tsx
│   ├── CalendarPage.tsx
│   └── MyAssignmentsPage.tsx
├── services/             # API clients
│   ├── api.service.ts
│   ├── skill.service.ts
│   ├── task.service.ts
│   ├── member.service.ts
│   ├── assignment.service.ts
│   └── invitation.service.ts
├── types/                # TypeScript types
│   └── index.ts
├── App.tsx               # Main app component
└── main.tsx              # Entry point
```

## Setup Instructions

### Prerequisites
- **Node.js** 18+ and npm
- **.NET 10 SDK**
- **PostgreSQL** database (Supabase account)
- **Brevo account** for email
- **Firebase account** for notifications (optional)

### Backend Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/Oladeji/ChurchRoster.git
   cd church-roster-system
   ```

2. **Configure database connection**

   Create or update `backend/ChurchRoster.Api/appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=your-supabase-host.supabase.co;Database=postgres;Username=postgres;Password=your-password;SSL Mode=Require"
     },
     "Jwt": {
       "Key": "your-secret-key-must-be-at-least-32-characters-long",
       "Issuer": "ChurchRoster",
       "Audience": "ChurchRosterUsers",
       "ExpiryInMinutes": 1440
     },
     "Email": {
       "BrevoApiKey": "your-brevo-api-key",
       "SenderEmail": "noreply@yourchurch.com",
       "SenderName": "Your Church Name"
     }
   }
   ```

3. **Run database migrations**

   In Supabase SQL Editor, run these scripts in order:

   a. `database_setup.sql` - Creates all tables
   b. `create_invitations_table.sql` - Creates invitations table

4. **Build and run backend**
   ```bash
   cd backend/ChurchRoster.Api
   dotnet restore
   dotnet build
   dotnet run
   ```

   Backend runs on: `https://localhost:7001`

### Frontend Setup

1. **Install dependencies**
   ```bash
   cd frontend
   npm install
   ```

2. **Configure environment variables**

   Create `frontend/.env`:
   ```env
   VITE_API_URL=https://localhost:7001/api
   VITE_FIREBASE_API_KEY=your-firebase-api-key
   VITE_FIREBASE_AUTH_DOMAIN=your-app.firebaseapp.com
   VITE_FIREBASE_PROJECT_ID=your-project-id
   VITE_FIREBASE_STORAGE_BUCKET=your-app.appspot.com
   VITE_FIREBASE_MESSAGING_SENDER_ID=your-sender-id
   VITE_FIREBASE_APP_ID=your-app-id
   VITE_FIREBASE_VAPID_KEY=your-vapid-key
   ```

3. **Run frontend**
   ```bash
   npm run dev
   ```

   Frontend runs on: `http://localhost:5173`

4. **Build for production**
   ```bash
   npm run build
   ```

## Development Workflow

### Adding a New Feature

#### Backend Development

1. **Create Entity** (if needed)
   ```csharp
   // In ChurchRoster.Core/Entities/
   public class NewEntity
   {
       public int Id { get; set; }
       public string Name { get; set; } = string.Empty;
   }
   ```

2. **Update DbContext**
   ```csharp
   // In ChurchRoster.Infrastructure/Data/AppDbContext.cs
   public DbSet<NewEntity> NewEntities { get; set; }
   ```

3. **Create DTOs**
   ```csharp
   // In ChurchRoster.Application/DTOs/
   public record NewEntityDto(int Id, string Name);
   public record CreateNewEntityRequest(string Name);
   ```

4. **Create Service Interface**
   ```csharp
   // In ChurchRoster.Application/Interfaces/
   public interface INewEntityService
   {
       Task<IEnumerable<NewEntityDto>> GetAllAsync();
       Task<NewEntityDto?> GetByIdAsync(int id);
       // ... other methods
   }
   ```

5. **Implement Service**
   ```csharp
   // In ChurchRoster.Application/Services/
   public class NewEntityService : INewEntityService
   {
       private readonly AppDbContext _context;
       // Implementation
   }
   ```

6. **Create Endpoints**
   ```csharp
   // In ChurchRoster.Api/Endpoints/V1/
   public static class NewEntityEndpoints
   {
       public static void MapNewEntityEndpoints(this IEndpointRouteBuilder routes)
       {
           var group = routes.MapGroup("/api/newentities");
           // Map endpoints
       }
   }
   ```

7. **Register Service**
   ```csharp
   // In ChurchRoster.Api/Program.cs
   builder.Services.AddScoped<INewEntityService, NewEntityService>();
   routes.MapNewEntityEndpoints();
   ```

#### Frontend Development

1. **Define TypeScript Types**
   ```typescript
   // In frontend/src/types/index.ts
   export interface NewEntity {
       id: number;
       name: string;
   }
   ```

2. **Create Service**
   ```typescript
   // In frontend/src/services/newentity.service.ts
   class NewEntityService {
       async getAll(): Promise<NewEntity[]> {
           return await apiService.get<NewEntity[]>('/newentities');
       }
   }
   export default new NewEntityService();
   ```

3. **Create Page Component**
   ```typescript
   // In frontend/src/pages/NewEntityPage.tsx
   const NewEntityPage: React.FC = () => {
       // Component logic
   };
   ```

4. **Add Route**
   ```typescript
   // In frontend/src/App.tsx
   <Route path="/newentities" element={
       <ProtectedRoute requireAdmin={true}>
           <NewEntityPage />
       </ProtectedRoute>
   } />
   ```

## Feature Implementation Guide

### Skills Management (IMPLEMENTED)

#### Backend
- **Endpoints**: `/api/skills`
- **Service**: `ISkillService`, `SkillService`
- **Entity**: `Skill`
- **Operations**: CRUD + User assignment

#### Frontend
- **Page**: `SkillsPage.tsx`
- **Service**: `skill.service.ts`
- **Features**:
  - List all skills with search/filter
  - Create new skill (modal)
  - Edit skill (modal)
  - Delete skill (confirmation)
  - View members with skill

### Tasks Management (IMPLEMENTED)

#### Backend
- **Endpoints**: `/api/tasks`
- **Service**: `ITaskService`, `TaskService`
- **Entity**: `MinistryTask`
- **Operations**: CRUD + Skill assignment

#### Frontend
- **Page**: `TasksPage.tsx`
- **Service**: `task.service.ts`
- **Features**:
  - List all tasks
  - Create task with skill selection
  - Edit task (update required skill)
  - Delete task
  - Filter by frequency/status

### Member Skills Assignment (IMPLEMENTED)

#### Backend
- **Endpoints**: `/api/members/{id}/skills`
- **Methods**:
  - `GET` - Get member's skills
  - `POST` - Assign skill to member
  - `DELETE` - Remove skill from member

#### Frontend
- **Component**: `ManageSkillsModal` in `Members.tsx`
- **Features**:
  - View current skills
  - Assign new skills
  - Remove skills

### Assignment with Qualified Filtering (IMPLEMENTED)

#### Backend
- **Endpoint**: `/api/members/qualified/{taskId}`
- **Logic**: Returns members who have the required skill for the task

#### Frontend
- **Page**: `AssignmentsPage.tsx`
- **Flow**:
  1. Select task
  2. System fetches qualified members
  3. Admin selects from qualified members only

## Testing

### Backend Testing

```bash
cd backend
dotnet test
```

### Frontend Testing

```bash
cd frontend
npm run test
```

### Manual Testing Checklist

#### Skills Management
- [ ] Create new skill
- [ ] Edit skill name/description
- [ ] Toggle skill active status
- [ ] Delete skill
- [ ] Verify skill appears in task dropdown

#### Tasks Management
- [ ] Create task without required skill
- [ ] Create task with required skill
- [ ] Edit task to add required skill
- [ ] Edit task to remove required skill
- [ ] Delete task
- [ ] Verify task appears in assignments dropdown

#### Member Skills
- [ ] Open skills modal for member
- [ ] Assign skill to member
- [ ] Remove skill from member
- [ ] Verify changes persist

#### Qualified Assignment
- [ ] Select task with required skill
- [ ] Verify only qualified members shown
- [ ] Select task without required skill
- [ ] Verify all active members shown

## Deployment

### Backend Deployment (Azure Example)

1. **Publish application**
   ```bash
   dotnet publish -c Release -o ./publish
   ```

2. **Create Azure App Service**
3. **Configure connection strings in Azure**
4. **Deploy publish folder**

### Frontend Deployment (Vercel Example)

1. **Build production bundle**
   ```bash
   npm run build
   ```

2. **Deploy to Vercel**
   ```bash
   vercel --prod
   ```

3. **Configure environment variables in Vercel dashboard**

## Common Issues and Solutions

### CORS Errors
- Ensure backend CORS policy includes frontend URL
- Check `Program.cs` CORS configuration

### Database Connection
- Verify Supabase connection string
- Check SSL Mode requirement
- Ensure IP is whitelisted in Supabase

### JWT Authentication
- Ensure JWT key is at least 32 characters
- Verify token expiry settings
- Check token storage in localStorage

### Build Errors
- Backend: Run `dotnet restore` and `dotnet build`
- Frontend: Delete `node_modules` and run `npm install`

## Best Practices

### Code Style
- Use meaningful variable names
- Add comments for complex logic
- Follow existing patterns
- Keep components small and focused

### Git Workflow
- Create feature branches from `main`
- Write descriptive commit messages
- Test before committing
- Submit PR for review

### Security
- Never commit sensitive credentials
- Use environment variables
- Validate all user input
- Use parameterized queries

## Additional Resources

- [.NET 10 Documentation](https://docs.microsoft.com/dotnet)
- [React Documentation](https://react.dev)
- [Tailwind CSS](https://tailwindcss.com)
- [Supabase Docs](https://supabase.com/docs)
- [Brevo API](https://developers.brevo.com)
