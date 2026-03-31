# Church Roster System - Backend

## Project Structure

This backend follows Clean Architecture principles with clear separation of concerns:

```
backend/
├── ChurchRoster.Api/              # ASP.NET Core Web API (Presentation Layer)
│   ├── Controllers/               # API endpoints
│   ├── DTOs/                      # Data Transfer Objects for API
│   ├── Middleware/                # Custom middleware components
│   └── Program.cs                 # Application entry point
│
├── ChurchRoster.Application/      # Business Logic Layer
│   ├── Services/                  # Business logic services
│   ├── Interfaces/                # Service interfaces
│   ├── DTOs/                      # Application-level DTOs
│   └── Validators/                # FluentValidation validators
│
├── ChurchRoster.Core/             # Domain Layer (Core Business Rules)
│   ├── Entities/                  # Domain entities (User, Task, Assignment, etc.)
│   ├── Interfaces/                # Repository interfaces
│   └── Enums/                     # Enums (Status, Role, etc.)
│
├── ChurchRoster.Infrastructure/   # Data Access Layer
│   ├── Data/                      # DbContext and migrations
│   ├── Repositories/              # Repository implementations
│   └── Services/                  # External services (Email, Push Notifications)
│
└── Directory.Packages.props       # Central Package Management
```

## Technology Stack

- **.NET 10** - Latest .NET framework
- **Entity Framework Core 10** - ORM for PostgreSQL
- **PostgreSQL (Supabase)** - Database
- **JWT Authentication** - Secure API authentication
- **BCrypt** - Password hashing
- **FirebaseAdmin** - Push notifications
- **MailKit** - Email notifications
- **FluentValidation** - Input validation
- **AutoMapper** - Object-to-object mapping

## Project Dependencies

```
ChurchRoster.Api
    ↓
    ├── ChurchRoster.Application
    │       ↓
    │   ChurchRoster.Core
    │
    └── ChurchRoster.Infrastructure
            ↓
        ChurchRoster.Core
```

## Central Package Management

This solution uses Central Package Management to maintain consistent package versions across all projects. 
All package versions are defined in `Directory.Packages.props`.

## Getting Started

### Prerequisites
- .NET 10 SDK
- PostgreSQL database (Supabase account)
- IDE: Visual Studio 2026 or VS Code

### Restore Packages
```bash
cd backend
dotnet restore
```

### Build Solution
```bash
dotnet build
```

### Run API
```bash
cd ChurchRoster.Api
dotnet run
```

The API will be available at `https://localhost:5001` (or the port shown in console).

## Next Steps

1. Set up database connection in `appsettings.json`
2. Create domain entities in `ChurchRoster.Core/Entities`
3. Implement DbContext in `ChurchRoster.Infrastructure/Data`
4. Create repositories in `ChurchRoster.Infrastructure/Repositories`
5. Implement business logic services in `ChurchRoster.Application/Services`
6. Create API controllers in `ChurchRoster.Api/Controllers`

## Development Guidelines

- Follow Clean Architecture principles
- Keep domain logic in Core layer
- Infrastructure depends on Core, not vice versa
- Use interfaces for dependency injection
- Validate all inputs using FluentValidation
- Use DTOs for API communication
- Document all public APIs with XML comments
