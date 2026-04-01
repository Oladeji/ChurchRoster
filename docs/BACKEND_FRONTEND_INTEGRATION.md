# Backend-Frontend Integration Guide

## Overview

This document explains how the backend (.NET 10 API) and frontend (React + TypeScript) work together.

---

## API Base URL Configuration

### Backend (Deployed on Render)
```
https://churchroster.onrender.com
```

### Frontend Environment Variable
```env
VITE_API_URL=https://churchroster.onrender.com/api
```

**Note:** Frontend adds `/api` prefix to all requests.

---

## Authentication Flow

### 1. User Login

**Frontend Request:**
```typescript
// src/services/auth.service.ts
await apiService.post<AuthResponse>('/auth/login', {
  email: 'admin@church.com',
  password: 'Admin123!'
});
```

**HTTP Request:**
```http
POST https://churchroster.onrender.com/api/auth/login
Content-Type: application/json

{
  "email": "admin@church.com",
  "password": "Admin123!"
}
```

**Backend Response:**
```json
{
  "userId": 1,
  "name": "Admin User",
  "email": "admin@church.com",
  "role": "Admin",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2026-03-13T10:00:00Z"
}
```

**Frontend Storage:**
```typescript
// Saves to localStorage
localStorage.setItem('authToken', response.token);
localStorage.setItem('user', JSON.stringify({
  userId: response.userId,
  name: response.name,
  email: response.email,
  role: response.role,
  isActive: true
}));
```

### 2. Authenticated Requests

**Frontend Auto-Injection:**
```typescript
// src/services/api.service.ts - Request Interceptor
this.api.interceptors.request.use((config) => {
  const token = localStorage.getItem('authToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});
```

**HTTP Request:**
```http
GET https://churchroster.onrender.com/api/members
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### 3. Token Expiration Handling

**Frontend Response Interceptor:**
```typescript
// src/services/api.service.ts - Response Interceptor
this.api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Clear auth data
      localStorage.removeItem('authToken');
      localStorage.removeItem('user');
      // Redirect to login
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);
```

---

## Type Mapping (Backend ↔ Frontend)

### AuthResponse

**Backend (.NET):**
```csharp
// ChurchRoster.Application/DTOs/Auth/AuthResponse.cs
public record AuthResponse(
    int UserId,
    string Name,
    string Email,
    string Role,
    string Token,
    DateTime ExpiresAt
);
```

**Frontend (TypeScript):**
```typescript
// src/types/index.ts
export interface AuthResponse {
  userId: number;
  name: string;
  email: string;
  role: 'Admin' | 'Member';
  token: string;
  expiresAt: string;
}
```

### User

**Backend (.NET):**
```csharp
// ChurchRoster.Core/Entities/User.cs
public class User
{
    public int UserId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Role { get; set; }
    public int? MonthlyLimit { get; set; }
    public bool IsActive { get; set; }
    // ... other properties
}
```

**Frontend (TypeScript):**
```typescript
// src/types/index.ts
export interface User {
  userId: number;
  name: string;
  email: string;
  phone: string;
  role: 'Admin' | 'Member';
  monthlyLimit?: number;
  deviceToken?: string;
  isActive: boolean;
  userSkills?: UserSkill[];
}
```

### Assignment

**Backend (.NET):**
```csharp
// ChurchRoster.Core/Entities/Assignment.cs
public class Assignment
{
    public int AssignmentId { get; set; }
    public int TaskId { get; set; }
    public int UserId { get; set; }
    public DateTime EventDate { get; set; }
    public string Status { get; set; }
    public string? RejectionReason { get; set; }
    public bool IsOverride { get; set; }
    // ... navigation properties
}
```

**Frontend (TypeScript):**
```typescript
// src/types/index.ts
export interface Assignment {
  assignmentId: number;
  taskId: number;
  userId: number;
  eventDate: string;
  status: AssignmentStatus;
  rejectionReason?: string;
  isOverride: boolean;
  assignedBy: number;
  createdAt: string;
  updatedAt: string;
  task?: Task;
  user?: User;
}

export type AssignmentStatus = 
  | 'Pending' 
  | 'Accepted' 
  | 'Rejected' 
  | 'Confirmed' 
  | 'Completed' 
  | 'Expired';
```

---

## API Endpoints Usage

### Member Management (Admin Only)

**Get All Members:**
```typescript
// Frontend
const members = await memberService.getAll();

// HTTP
GET /api/members
Authorization: Bearer {token}
```

**Create Member:**
```typescript
// Frontend
const newMember = await memberService.create({
  name: 'Jane Doe',
  email: 'jane@church.com',
  phone: '555-0102',
  monthlyLimit: 4
});

// HTTP
POST /api/members
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "Jane Doe",
  "email": "jane@church.com",
  "phone": "555-0102",
  "monthlyLimit": 4
}
```

### Assignment Management

**Get User Assignments:**
```typescript
// Frontend
const assignments = await assignmentService.getByUser(userId);

// HTTP
GET /api/assignments/user/{userId}
Authorization: Bearer {token}
```

**Create Assignment:**
```typescript
// Frontend
const assignment = await assignmentService.create({
  taskId: 1,
  userId: 2,
  eventDate: '2026-03-15',
  assignedBy: 1
});

// HTTP
POST /api/assignments
Authorization: Bearer {token}
Content-Type: application/json

{
  "taskId": 1,
  "userId": 2,
  "eventDate": "2026-03-15",
  "assignedBy": 1
}
```

**Update Assignment Status:**
```typescript
// Frontend
await assignmentService.updateStatus(assignmentId, {
  status: 'Accepted'
});

// HTTP
PUT /api/assignments/{assignmentId}/status
Authorization: Bearer {token}
Content-Type: application/json

{
  "status": "Accepted"
}
```

---

## Error Handling

### Backend Error Response

```json
{
  "error": "Internal Server Error",
  "message": "Specific error message",
  "path": "/api/members",
  "timestamp": "2026-03-12T10:00:00Z"
}
```

### Frontend Error Handling

```typescript
// In components
try {
  await authService.login(email, password);
  navigate('/dashboard');
} catch (error: any) {
  setError(error.response?.data?.message || 'Login failed');
}
```

---

## CORS Configuration

### Backend (appsettings.json)

```json
{
  "CorsSettings": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "https://your-app.vercel.app"
    ]
  }
}
```

### Backend (Program.cs)

```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

app.UseCors();
```

---

## Testing Integration

### 1. Test Backend Endpoints

Using Thunder Client, Postman, or curl:

```bash
# Login
curl -X POST https://churchroster.onrender.com/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@church.com","password":"Admin123!"}'

# Get Members (use token from login)
curl -X GET https://churchroster.onrender.com/api/members \
  -H "Authorization: Bearer {your-token}"
```

### 2. Test Frontend Integration

```bash
cd frontend
npm run dev
# Open http://localhost:3000
# Login with admin@church.com / Admin123!
```

### 3. Check Browser DevTools

**Network Tab:**
- Verify requests go to correct URL
- Check Authorization header is present
- Check response status codes

**Console Tab:**
- Look for JavaScript errors
- Check logged messages

**Application Tab:**
- Verify token in localStorage
- Verify user object in localStorage

---

## Common Integration Issues

### Issue 1: CORS Error

**Error:**
```
Access to XMLHttpRequest at 'https://churchroster.onrender.com/api/auth/login' 
from origin 'http://localhost:3000' has been blocked by CORS policy
```

**Solution:**
- Add `http://localhost:3000` to backend CORS allowed origins
- Redeploy backend
- Clear browser cache

### Issue 2: 401 Unauthorized

**Error:**
```
Request failed with status code 401
```

**Possible Causes:**
1. Token expired → Login again
2. Token not sent → Check Authorization header
3. Invalid token → Clear localStorage and login again
4. Backend JWT validation failed → Check JWT settings match

**Solution:**
```typescript
// Clear auth and retry login
localStorage.clear();
window.location.href = '/login';
```

### Issue 3: Network Error

**Error:**
```
Network Error
```

**Possible Causes:**
1. Backend is down → Check Render dashboard
2. Wrong API URL → Verify VITE_API_URL in .env
3. DNS issues → Try IP address instead of domain

**Solution:**
- Check backend is running: Visit https://churchroster.onrender.com
- Verify .env has correct URL
- Restart frontend dev server after changing .env

### Issue 4: 500 Internal Server Error

**Error:**
```
Request failed with status code 500
```

**Solution:**
- Check backend logs on Render
- Verify database connection is working
- Check backend error response body for details

---

## Environment Configuration

### Development (Local Backend)

**Backend:**
```bash
cd backend
dotnet run
# Runs on http://localhost:5000
```

**Frontend (.env):**
```env
VITE_API_URL=http://localhost:5000/api
```

### Production (Deployed)

**Backend on Render:**
```
https://churchroster.onrender.com
```

**Frontend (.env):**
```env
VITE_API_URL=https://churchroster.onrender.com/api
```

---

## Security Considerations

1. **HTTPS Only in Production**
   - Never send tokens over HTTP
   - Both backend and frontend must use HTTPS

2. **Token Storage**
   - Stored in localStorage (acceptable for now)
   - For higher security, use httpOnly cookies

3. **CORS**
   - Only allow trusted frontend domains
   - Never use `AllowAnyOrigin()` in production

4. **Token Expiration**
   - Backend sets expiration (24 hours default)
   - Frontend handles 401 and redirects to login

5. **API Keys**
   - Never commit .env file to Git
   - Use environment variables on hosting platforms

---

## Deployment Checklist

### Backend (Render)
- [x] Backend deployed
- [x] Environment variables set
- [x] Database connected
- [x] CORS configured for frontend domain

### Frontend (Vercel)
- [ ] Push code to GitHub
- [ ] Connect repo to Vercel
- [ ] Set VITE_API_URL environment variable
- [ ] Deploy
- [ ] Test login and API calls

### Post-Deployment
- [ ] Add frontend URL to backend CORS
- [ ] Test login from deployed frontend
- [ ] Test all API endpoints
- [ ] Verify PWA installation works

---

## Next: Week 4 Implementation

With the integration working:
1. Calendar component will call `assignmentService.getAll()`
2. Assignment modal will call `assignmentService.create()`
3. Task list will call `taskService.getAll()`
4. Member selection will call `memberService.getAll()`

All the foundation is in place! 🎉
