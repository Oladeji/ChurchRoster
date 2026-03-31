# Backend Structure Summary

## ✅ Created Projects

### 1. **ChurchRoster.Api** (ASP.NET Core Web API)
   - **Purpose**: API layer / Presentation layer
   - **Framework**: .NET 10
   - **Folders Created**:
     - `Controllers/` - API endpoints
     - `DTOs/` - Data Transfer Objects for API
     - `Middleware/` - Custom middleware
   - **Key Files**:
     - `Program.cs` - Application entry point
     - `appsettings.json` - Configuration (with templates for DB, JWT, Email, Firebase)
   - **Dependencies**:
     - ChurchRoster.Application
     - ChurchRoster.Infrastructure
     - JWT Bearer Authentication
     - Swagger/OpenAPI
     - FirebaseAdmin (Push Notifications)
     - MailKit (Email)
     - BCrypt (Password Hashing)

### 2. **ChurchRoster.Core** (Class Library)
   - **Purpose**: Domain layer - Core business entities and rules
   - **Framework**: .NET 10
   - **Folders Created**:
     - `Entities/` - Domain models (User, Task, Assignment, Skill, etc.)
     - `Interfaces/` - Repository interfaces
     - `Enums/` - Status, Role, Frequency enums
   - **Dependencies**: None (Pure domain layer)

### 3. **ChurchRoster.Application** (Class Library)
   - **Purpose**: Business logic layer
   - **Framework**: .NET 10
   - **Folders Created**:
     - `Services/` - Business logic services
     - `Interfaces/` - Service interfaces
     - `DTOs/` - Application-level DTOs
     - `Validators/` - FluentValidation validators
   - **Dependencies**:
     - ChurchRoster.Core
     - FluentValidation
     - AutoMapper

### 4. **ChurchRoster.Infrastructure** (Class Library)
   - **Purpose**: Data access and external services
   - **Framework**: .NET 10
   - **Folders Created**:
     - `Data/` - DbContext, Configurations, Migrations
     - `Repositories/` - Repository implementations
     - `Services/` - External service implementations (Email, Push Notifications)
   - **Dependencies**:
     - ChurchRoster.Core
     - Entity Framework Core 10
     - Npgsql.EntityFrameworkCore.PostgreSQL

## 📦 Central Package Management

All package versions are managed centrally in `backend/Directory.Packages.props`:

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>

  <ItemGroup>
    <!-- Entity Framework Core 10.0.0 -->
    <!-- JWT Authentication 10.0.0 -->
    <!-- FirebaseAdmin 3.5.0 -->
    <!-- MailKit 4.10.0 -->
    <!-- FluentValidation 11.11.0 -->
    <!-- AutoMapper 14.0.0 -->
  </ItemGroup>
</Project>
```

## 🏗️ Project Dependencies

```
ChurchRoster.Api (Web API)
    ├── → ChurchRoster.Application
    │       └── → ChurchRoster.Core
    └── → ChurchRoster.Infrastructure
            └── → ChurchRoster.Core
```

## 📄 Configuration Files Created

1. **Directory.Packages.props** - Central package version management
2. **Dockerfile** - Docker containerization for deployment
3. **.dockerignore** - Docker build optimization
4. **.gitignore** - Git version control
5. **appsettings.json** - Application configuration template
6. **README.md** - Backend documentation

## 🚀 Next Steps

### 1. Database Setup (Week 1)
   - Create Supabase account
   - Get connection string
   - Update `appsettings.json` with actual connection string

### 2. Domain Models (Week 1)
   Create in `ChurchRoster.Core/Entities/`:
   - `User.cs` - User entity
   - `Skill.cs` - Skill entity
   - `UserSkill.cs` - Many-to-many relationship
   - `Task.cs` - Task entity
   - `Assignment.cs` - Assignment entity

### 3. Enums (Week 1)
   Create in `ChurchRoster.Core/Enums/`:
   - `AssignmentStatus.cs` (Pending, Accepted, Rejected, Confirmed, Completed, Expired)
   - `UserRole.cs` (Admin, Member)
   - `TaskFrequency.cs` (Weekly, Monthly)

### 4. DbContext (Week 1)
   Create in `ChurchRoster.Infrastructure/Data/`:
   - `AppDbContext.cs` - Entity Framework DbContext
   - Entity configurations

### 5. Repositories (Week 1-2)
   Create interfaces in `ChurchRoster.Core/Interfaces/`:
   - `IUserRepository.cs`
   - `ISkillRepository.cs`
   - `ITaskRepository.cs`
   - `IAssignmentRepository.cs`

   Implement in `ChurchRoster.Infrastructure/Repositories/`:
   - `UserRepository.cs`
   - `SkillRepository.cs`
   - `TaskRepository.cs`
   - `AssignmentRepository.cs`

### 6. Business Services (Week 2)
   Create in `ChurchRoster.Application/Services/`:
   - `AuthService.cs` - Login, Register, JWT generation
   - `MemberService.cs` - Member management
   - `SkillService.cs` - Skill management
   - `TaskService.cs` - Task catalog management
   - `AssignmentService.cs` - Assignment logic with business rules

### 7. API Controllers (Week 2)
   Create in `ChurchRoster.Api/Controllers/`:
   - `AuthController.cs` - Authentication endpoints
   - `MembersController.cs` - Member CRUD
   - `SkillsController.cs` - Skill management
   - `TasksController.cs` - Task catalog
   - `AssignmentsController.cs` - Assignment management

### 8. Middleware (Week 2)
   Create in `ChurchRoster.Api/Middleware/`:
   - `JwtMiddleware.cs` - JWT token validation
   - `ExceptionHandlingMiddleware.cs` - Global error handling

### 9. Run Migrations (Week 1)
   ```bash
   cd backend/ChurchRoster.Infrastructure
   dotnet ef migrations add InitialCreate --startup-project ../ChurchRoster.Api
   dotnet ef database update --startup-project ../ChurchRoster.Api
   ```

### 10. Test API (Week 2)
   ```bash
   cd backend/ChurchRoster.Api
   dotnet run
   ```
   Open browser: `https://localhost:5001/swagger`

## 📊 Build Status

✅ All projects created successfully
✅ Project references configured
✅ NuGet packages restored
✅ Build completed successfully
✅ Ready for development

## 🛠️ Commands

### Build
```bash
dotnet build backend/ChurchRoster.Api/ChurchRoster.Api.csproj
```

### Run
```bash
dotnet run --project backend/ChurchRoster.Api/ChurchRoster.Api.csproj
```

### Test (when tests are added)
```bash
dotnet test
```

### Docker Build
```bash
cd backend
docker build -t church-roster-api .
```

### Docker Run
```bash
docker run -p 8080:8080 church-roster-api
```

## 📝 Notes

- All projects target **.NET 10**
- Using **Central Package Management** for consistent versioning
- Following **Clean Architecture** principles
- **PostgreSQL** via Supabase for database
- Ready for **Docker deployment** to Render
- Configured for **CORS** to support frontend integration

---

**Status**: ✅ Backend structure complete and ready for Week 1 development!
