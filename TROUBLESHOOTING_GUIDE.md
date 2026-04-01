# Detailed Logging & Troubleshooting Guide

## Changes Made to Enable Detailed Logging

### 1. Program.cs Updates
✅ **Added comprehensive logging configuration:**
- Console logging with timestamps
- Debug logging for ChurchRoster namespace
- Information logging for EF Core database commands
- Global exception handler with detailed error responses
- Request/response logging middleware
- Connection string validation with masking

✅ **Startup diagnostics:**
- Logs port number
- Logs environment
- Logs connection string (masked)
- Warns if connection string is missing

### 2. appsettings.json Updates
✅ **Enhanced logging levels:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information",
      "ChurchRoster": "Debug"
    }
  }
}
```

### 3. AuthService.cs Updates
✅ **Added logging to all critical methods:**
- `LoginAsync` - Logs each step of authentication
- `GenerateJwtToken` - Logs JWT configuration and generation
- `VerifyPassword` - Logs password verification
- `HashPassword` - Logs password hashing

### 4. AuthEndpoints.cs Updates
✅ **Added endpoint-level logging:**
- Logs incoming requests
- Logs validation failures
- Logs successful authentications
- Catches and logs exceptions

---

## How to View Logs on Render

### Step 1: Access Render Dashboard
1. Go to https://dashboard.render.com
2. Click on your `churchroster` service
3. Click on **"Logs"** tab

### Step 2: What to Look For

The logs will now show detailed information:

**On startup:**
```
=== Church Roster API Starting ===
Port: 10000
Environment: Production
Connection String: Host=db.xxx.supabase.co;...;Password=***MASKED***;...
```

**On login request:**
```
==> Request: POST /api/auth/login
Login endpoint called for email: admin@church.com
Login attempt for email: admin@church.com
User found: 1, verifying password...
Password verified, generating JWT token...
Generating JWT token for user: 1, email: admin@church.com, role: Admin
JWT Settings - Issuer: ChurchRosterApi, Audience: ChurchRosterClient...
JWT token generated successfully
Login successful for user: 1, email: admin@church.com
Login successful for email: admin@church.com
<== Response: 200 for POST /api/auth/login
```

**On error:**
```
==> Request: POST /api/auth/login
Login endpoint called for email: admin@church.com
Error during login for email: admin@church.com
System.InvalidOperationException: The ConnectionString property has not been initialized.
   at Npgsql.ThrowHelper.ThrowInvalidOperationException...
```

---

## Common Errors and Solutions

### Error 1: Connection String Not Initialized

**Log Shows:**
```
WARNING: ConnectionStrings__DefaultConnection is not set!
The ConnectionString property has not been initialized
```

**Solution:**
Check environment variables in Render:
1. Go to Render Dashboard → Your Service → Environment
2. Verify: `ConnectionStrings__DefaultConnection` exists
3. Check format:
   ```
   Host=db.xxx.supabase.co;Database=postgres;Username=postgres;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true
   ```
4. **Note:** Use double underscores `__` not single `:`

**Fix in Render:**
- Add/Update environment variable
- Click "Save Changes"
- Service will redeploy automatically

---

### Error 2: JWT SecretKey Not Configured

**Log Shows:**
```
JWT SecretKey is not configured!
InvalidOperationException: JWT SecretKey not configured
```

**Solution:**
1. Go to Render Dashboard → Environment Variables
2. Add: `JwtSettings__SecretKey` (use double underscores)
3. Value: 64+ character random string

**Generate secret:**
```powershell
-join ((65..90) + (97..122) + (48..57) | Get-Random -Count 64 | % {[char]$_})
```

---

### Error 3: Database Connection Timeout

**Log Shows:**
```
Npgsql.NpgsqlException: Exception while connecting
The connection attempt failed
```

**Solutions:**
1. Verify Supabase database is running
2. Check connection string has: `SSL Mode=Require`
3. Verify IP whitelist (Supabase allows all by default)
4. Test connection string locally first

---

### Error 4: User Not Found

**Log Shows:**
```
Login failed: User not found for email: admin@church.com
```

**Solution:**
Your database doesn't have the admin user. Run migration:

```powershell
# Locally or via Render shell
cd backend/ChurchRoster.Infrastructure
dotnet ef database update --startup-project ../ChurchRoster.Api
```

Or manually insert admin user in Supabase SQL Editor:
```sql
INSERT INTO users (name, email, phone, password_hash, role, is_active, created_at, updated_at)
VALUES (
  'System Administrator',
  'admin@church.com',
  '+1234567890',
  '$2a$11$X6qGvzOj1Xd/sOJs.rc0FOxxoda66NVWgoIHi.VkPTy2XRlzAlOJm',
  'Admin',
  true,
  NOW(),
  NOW()
);
```
Password: `Admin@123`

---

### Error 5: Password Verification Failed

**Log Shows:**
```
Password verification result: False
Login failed: Invalid password for email: admin@church.com
```

**Solution:**
Password hash in database doesn't match. Update it:

```sql
-- In Supabase SQL Editor
UPDATE users 
SET password_hash = '$2a$11$X6qGvzOj1Xd/sOJs.rc0FOxxoda66NVWgoIHi.VkPTy2XRlzAlOJm'
WHERE email = 'admin@church.com';
```

---

## Testing After Deployment

### 1. Check Startup Logs
```
=== Church Roster API Starting ===
=== Church Roster API Started Successfully ===
```

If you see these, the API started successfully.

### 2. Test Login Endpoint

**Via cURL:**
```bash
curl https://churchroster.onrender.com/api/auth/login \
  -X POST \
  -H 'Content-Type: application/json' \
  -d '{
    "email": "admin@church.com",
    "password": "Admin@123"
  }'
```

**Expected Response (Success):**
```json
{
  "userId": 1,
  "name": "System Administrator",
  "email": "admin@church.com",
  "role": "Admin",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2026-04-01T12:00:00Z"
}
```

**Expected Logs (Success):**
```
==> Request: POST /api/auth/login
Login attempt for email: admin@church.com
User found: 1, verifying password...
Password verified, generating JWT token...
Login successful
<== Response: 200 for POST /api/auth/login
```

### 3. Test via Swagger
```
https://churchroster.onrender.com/scalar/v1
```

---

## Environment Variables Checklist

Verify these are set in Render:

- [ ] `ASPNETCORE_ENVIRONMENT` = `Production`
- [ ] `ConnectionStrings__DefaultConnection` = `Host=db.xxx.supabase.co;...`
- [ ] `JwtSettings__SecretKey` = `your-64-char-secret`
- [ ] `JwtSettings__Issuer` = `ChurchRosterApi`
- [ ] `JwtSettings__Audience` = `ChurchRosterClient`
- [ ] `JwtSettings__ExpirationInMinutes` = `1440`

---

## Log Analysis Commands

### Filter Logs in Render

**Show only errors:**
Search for: `ERROR`

**Show login attempts:**
Search for: `Login attempt`

**Show database queries:**
Search for: `Executing DbCommand`

**Show JWT generation:**
Search for: `Generating JWT token`

---

## Quick Diagnostic Checklist

When you get 500 error, check logs for:

1. **Startup Section:**
   - [ ] "Church Roster API Starting" appears
   - [ ] Port number is logged
   - [ ] Connection string is logged (masked)
   - [ ] No "WARNING: ConnectionStrings__DefaultConnection is not set!"

2. **Request Section:**
   - [ ] "Request: POST /api/auth/login" appears
   - [ ] "Login attempt for email: xxx" appears
   - [ ] What's the last log before error?

3. **Error Section:**
   - [ ] Exception type (InvalidOperationException, NpgsqlException, etc.)
   - [ ] Error message
   - [ ] Stack trace

---

## Render Log Levels

After the changes, you'll see:

| Level | Color | Example |
|-------|-------|---------|
| INFO | White | `Login successful for user: 1` |
| DEBUG | Gray | `Verifying password` |
| WARN | Yellow | `Login failed: User not found` |
| ERROR | Red | `Error during login: Exception...` |

---

## Performance Monitoring

The logs will also show timing:

```
2026-03-31 19:30:45 ==> Request: POST /api/auth/login
2026-03-31 19:30:45 Login attempt for email: admin@church.com
2026-03-31 19:30:46 <== Response: 200 for POST /api/auth/login
```

If time between request and response > 5 seconds, check:
- Database latency
- Render instance type (Free tier has slower cold starts)

---

## Next Steps

1. **Deploy updated code:**
   ```bash
   git add .
   git commit -m "Add detailed logging for troubleshooting"
   git push origin main
   ```

2. **Wait for Render to redeploy** (3-5 minutes)

3. **Check startup logs** in Render dashboard

4. **Try login again** and check logs

5. **Look for specific error** in the detailed logs

6. **Share the error details** if you need further help

---

## Expected Log Flow for Successful Login

```
[Startup]
=== Church Roster API Starting ===
Port: 10000
Environment: Production
Connection String: Host=db.xxx.supabase.co;Database=postgres;...;Password=***MASKED***;...
=== Church Roster API Started Successfully ===

[Request Received]
==> Request: POST /api/auth/login

[Endpoint Processing]
Login endpoint called for email: admin@church.com

[Service Processing]
Login attempt for email: admin@church.com
User found: 1, verifying password...
Password verified, generating JWT token...

[JWT Generation]
Generating JWT token for user: 1, email: admin@church.com, role: Admin
JWT Settings - Issuer: ChurchRosterApi, Audience: ChurchRosterClient, Expiration: 1440 minutes
JWT token generated successfully

[Success]
Login successful for user: 1, email: admin@church.com
Login successful for email: admin@church.com

[Response]
<== Response: 200 for POST /api/auth/login
```

---

*Troubleshooting Guide v1.0 - Detailed Logging Enabled*
