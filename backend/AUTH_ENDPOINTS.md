# Authentication Endpoints - Day 5

## Overview
The authentication endpoints provide user registration, login, and JWT token generation functionality for the Church Roster System.

## Endpoints

### 1. POST /api/auth/register
Register a new user account.

**Request Body:**
```json
{
  "name": "John Doe",
  "email": "john.doe@example.com",
  "password": "SecurePass123",
  "phone": "+1234567890"
}
```

**Response (201 Created):**
```json
{
  "userId": 2,
  "name": "John Doe",
  "email": "john.doe@example.com",
  "role": "Member",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2026-04-01T19:03:39.123Z"
}
```

**Password Requirements:**
- Minimum 8 characters
- At least one uppercase letter
- At least one lowercase letter
- At least one number

**Errors:**
- 400 Bad Request: Missing required fields
- 409 Conflict: Email already exists or password doesn't meet requirements

---

### 2. POST /api/auth/login
Authenticate a user and receive a JWT token.

**Request Body:**
```json
{
  "email": "admin@church.com",
  "password": "Admin@123"
}
```

**Response (200 OK):**
```json
{
  "userId": 1,
  "name": "System Administrator",
  "email": "admin@church.com",
  "role": "Admin",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2026-04-01T19:03:39.123Z"
}
```

**Errors:**
- 400 Bad Request: Missing email or password
- 401 Unauthorized: Invalid credentials or inactive user

---

## Testing with Swagger

1. Run the application: `dotnet run --project backend/ChurchRoster.Api`
2. Navigate to: `https://localhost:7xxx/scalar/v1` (check console for actual port)
3. Test the endpoints:

### Test Login (Admin User)
```json
{
  "email": "admin@church.com",
  "password": "Admin@123"
}
```

### Test Registration (New User)
```json
{
  "name": "Jane Smith",
  "email": "jane.smith@church.com",
  "password": "SecurePass123",
  "phone": "+9876543210"
}
```

---

## JWT Token Configuration

The JWT tokens are configured in `appsettings.json`:

```json
{
  "JwtSettings": {
    "SecretKey": "your-super-secret-jwt-key-change-this-in-production",
    "Issuer": "ChurchRosterApi",
    "Audience": "ChurchRosterClient",
    "ExpirationInMinutes": 1440
  }
}
```

**Token Claims:**
- `sub`: User ID
- `email`: User email address
- `role`: User role (Admin/Member)
- `jti`: Unique token identifier

**Token Expiration:** 24 hours (1440 minutes)

---

## Security Features

1. **BCrypt Password Hashing**: All passwords are hashed using BCrypt with a cost factor of 11
2. **JWT Authentication**: Tokens are signed using HMAC-SHA256
3. **Password Validation**: Enforces complexity requirements
4. **Email Uniqueness**: Prevents duplicate registrations
5. **Active User Check**: Only active users can login

---

## Database Changes

### Migration: UpdateAdminPassword
Updated the seeded admin user's password hash to a properly BCrypt-hashed value.

**Admin Credentials:**
- Email: `admin@church.com`
- Password: `Admin@123`

---

## Implementation Details

### Files Created:
1. **DTOs:**
   - `ChurchRoster.Application/DTOs/Auth/LoginRequest.cs`
   - `ChurchRoster.Application/DTOs/Auth/RegisterRequest.cs`
   - `ChurchRoster.Application/DTOs/Auth/AuthResponse.cs`

2. **Service Interface & Implementation:**
   - `ChurchRoster.Application/Interfaces/IAuthService.cs`
   - `ChurchRoster.Application/Services/AuthService.cs`

3. **API Endpoints:**
   - `ChurchRoster.Api/Endpoints/V1/AuthEndpoints.cs`

### Files Modified:
1. `ChurchRoster.Api/EndpointRegistration.cs` - Added auth endpoint registration
2. `ChurchRoster.Api/APIServiceCollection.cs` - Registered AuthService
3. `ChurchRoster.Application/ChurchRoster.Application.csproj` - Added package references
4. `ChurchRoster.Infrastructure/Data/AppDbContext.cs` - Updated admin password hash

---

## Next Steps (Week 2)

1. Add JWT authentication middleware to protect endpoints
2. Implement role-based authorization
3. Add refresh token functionality
4. Implement password reset via email
5. Add email verification for new registrations
6. Create user management endpoints (CRUD operations)

---

## Notes

- All new users are registered with the "Member" role by default
- Default monthly assignment limit is set to 4
- Only administrators can create other admin users (to be implemented)
- Remember to change the JWT SecretKey in production!
