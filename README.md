# Church Roster Management System

A comprehensive church roster and assignment management system built with React 19.2 + TypeScript frontend and .NET 10 backend.

## Features

### Admin Features
- **Skills Management**: Create, edit, and delete skills/qualifications
- **Tasks Management**: Create ministry tasks and assign required skills
- **Member Management**: Manage members and assign skills to them
- **Assignment System**: Assign members to tasks with automatic qualified filtering
- **Email Invitations**: Send invitation emails to new members
- **Calendar View**: Visual monthly schedule

### Member Features
- View personal assignments
- Accept/Reject assignments
- View ministry calendar
- Accept invitation and set password

## Getting Started

See DEVELOPMENT_GUIDE.md for detailed setup instructions.

## Quick Start

Backend:
```bash
cd backend/ChurchRoster.Api
dotnet run
```

Frontend:
```bash
cd frontend
npm install
npm run dev
```

## Documentation
- [Development Guide](./DEVELOPMENT_GUIDE.md)
- [API Documentation](./API_DOCUMENTATION.md)
- [User Guide](./USER_GUIDE.md)
