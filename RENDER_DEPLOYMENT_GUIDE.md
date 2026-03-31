# Deploying to Render - Step-by-Step Guide

## Prerequisites
- [x] GitHub account
- [x] Render account (free tier available at https://render.com)
- [x] Supabase database already set up
- [x] Code in GitHub repository

---

## Step 1: Prepare Your Repository

### 1.1 Create GitHub Repository
```bash
# Navigate to project root
cd C:\Works\CSharp\ChurchRoster\church-roster-system

# Initialize git (if not already done)
git init

# Add all files
git add .

# Commit
git commit -m "Week 2: Backend complete with all CRUD endpoints and business rules"

# Add remote (replace with your GitHub repo URL)
git remote add origin https://github.com/YOUR_USERNAME/church-roster-system.git

# Push to GitHub
git push -u origin main
```

### 1.2 Create .gitignore (if needed)
Ensure your `.gitignore` excludes sensitive files:
```
# Add to .gitignore
**/bin/
**/obj/
**/.vs/
**/appsettings.Development.json
**/.env
```

---

## Step 2: Set Up Render Account

1. Go to https://render.com
2. Click **"Get Started"**
3. Sign up using your GitHub account (recommended)
4. Authorize Render to access your repositories

---

## Step 3: Create Web Service on Render

### 3.1 Create New Web Service
1. From Render Dashboard, click **"New +"**
2. Select **"Web Service"**
3. Connect your GitHub repository
4. Select `church-roster-system` repository

### 3.2 Configure Service Settings

**Basic Settings:**
- **Name:** `church-roster-api` (or your preferred name)
- **Region:** Choose closest to your location (e.g., Oregon, Frankfurt)
- **Branch:** `main`
- **Root Directory:** Leave blank (Dockerfile is at root)
- **Environment:** `Docker`
- **Dockerfile Path:** `./Dockerfile`

**Instance Type:**
- Free tier: Select **"Free"** ($0/month, goes to sleep after inactivity)
- Paid: Select **"Starter"** ($7/month, always on)

---

## Step 4: Configure Environment Variables

### 4.1 Add Environment Variables
In the Render dashboard, scroll to **"Environment Variables"** section and add:

| Key | Value | Notes |
|-----|-------|-------|
| `ASPNETCORE_ENVIRONMENT` | `Production` | Required |
| `ConnectionStrings__DefaultConnection` | Your Supabase connection string | See below |
| `JwtSettings__SecretKey` | Generate a strong secret | See below |
| `JwtSettings__Issuer` | `ChurchRosterApi` | |
| `JwtSettings__Audience` | `ChurchRosterClient` | |
| `JwtSettings__ExpirationInMinutes` | `1440` | 24 hours |

### 4.2 Get Your Connection String from Supabase
1. Go to Supabase Dashboard → Settings → Database
2. Copy the **Connection String** (URI format)
3. Replace `[YOUR-PASSWORD]` with your actual password

Example format:
```
Host=db.edxjeuoutitcdfuqzyxp.supabase.co;Database=postgres;Username=postgres;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true
```

### 4.3 Generate JWT Secret Key
Run this in PowerShell to generate a secure key:
```powershell
-join ((65..90) + (97..122) + (48..57) | Get-Random -Count 64 | % {[char]$_})
```

Copy the generated string and use it as your JWT secret.

### 4.4 Optional: Email Settings (for Week 5)
```
EmailSettings__SmtpServer=smtp-relay.brevo.com
EmailSettings__SmtpPort=587
EmailSettings__SenderEmail=your-email@church.com
EmailSettings__Username=your-brevo-username
EmailSettings__Password=your-brevo-password
```

---

## Step 5: Update appsettings.json for Production

Ensure your `backend/ChurchRoster.Api/appsettings.json` has production-safe defaults:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=postgres;Username=postgres;Password=password"
  },
  "JwtSettings": {
    "SecretKey": "CHANGE_THIS_IN_PRODUCTION",
    "Issuer": "ChurchRosterApi",
    "Audience": "ChurchRosterClient",
    "ExpirationInMinutes": 1440
  },
  "CorsSettings": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:5173",
      "https://your-frontend-domain.com"
    ]
  }
}
```

**Note:** The environment variables you set in Render will override these values.

---

## Step 6: Deploy

### 6.1 Automatic Deployment
1. Click **"Create Web Service"** at the bottom
2. Render will automatically:
   - Pull your code from GitHub
   - Build the Docker image
   - Deploy the container
   - Assign a public URL

### 6.2 Monitor Deployment
1. Watch the **"Logs"** tab for build progress
2. Look for: `Building Dockerfile...`
3. Wait for: `Your service is live 🎉`

**Expected build time:** 3-5 minutes

---

## Step 7: Test Your Deployed API

### 7.1 Get Your API URL
Your API will be available at:
```
https://church-roster-api.onrender.com
```
(Replace `church-roster-api` with your service name)

### 7.2 Test Authentication Endpoint
```bash
curl https://church-roster-api.onrender.com/api/auth/login \
  --request POST \
  --header 'Content-Type: application/json' \
  --data '{
    "email": "admin@church.com",
    "password": "Admin@123"
  }'
```

### 7.3 Access API Documentation
Open in browser:
```
https://church-roster-api.onrender.com/scalar/v1
```

---

## Step 8: Configure CORS for Frontend (Week 3)

### 8.1 Update Environment Variables
Add your frontend domain to CORS:
```
CorsSettings__AllowedOrigins__0=https://your-frontend-domain.vercel.app
```

### 8.2 Update Program.cs (Already done)
Your API should have CORS configured in `Program.cs`:
```csharp
app.UseCors(policy =>
{
    policy.WithOrigins(configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>() ?? new[] { "*" })
          .AllowAnyMethod()
          .AllowAnyHeader()
          .AllowCredentials();
});
```

---

## Step 9: Set Up Custom Domain (Optional)

### 9.1 Add Custom Domain in Render
1. Go to **Settings** → **Custom Domain**
2. Click **"Add Custom Domain"**
3. Enter your domain (e.g., `api.yourchurch.com`)
4. Follow DNS configuration instructions

### 9.2 Update DNS Records
Add these records to your DNS provider:
```
Type: CNAME
Name: api
Value: church-roster-api.onrender.com
```

**SSL:** Render automatically provides free SSL certificates.

---

## Step 10: Set Up Automatic Deployments

### 10.1 Enable Auto-Deploy
In Render dashboard:
1. Go to **Settings**
2. Under **Build & Deploy**, enable:
   - **Auto-Deploy:** Yes (deploys on every push to main branch)

### 10.2 Deploy from Specific Branch
You can also set up deploy previews:
- **Preview branches:** Deploy pull requests automatically
- **Branch deploys:** Deploy from specific branches (e.g., `staging`)

---

## Step 11: Monitor and Maintain

### 11.1 View Logs
```
Dashboard → Your Service → Logs
```
- Monitor errors
- Check performance
- Debug issues

### 11.2 Health Checks
Render automatically pings your service at:
```
GET /
```

### 11.3 Metrics (Paid plans)
- CPU usage
- Memory usage
- Request count
- Response times

---

## Step 12: Troubleshooting

### Common Issues

#### 1. Build Fails
**Error:** `The type or namespace name 'X' could not be found`
**Solution:** 
- Ensure all `.csproj` files are copied in Dockerfile
- Check `Directory.Packages.props` is included

#### 2. Service Won't Start
**Error:** `Application startup exception`
**Solution:**
- Check environment variables are set correctly
- Verify connection string format
- Check logs for specific error

#### 3. Database Connection Fails
**Error:** `The ConnectionString property has not been initialized`
**Solution:**
- Verify environment variable name: `ConnectionStrings__DefaultConnection`
- Note the double underscore `__`
- Check Supabase connection string is correct

#### 4. CORS Errors
**Error:** `Access to fetch at 'https://api...' from origin 'https://...' has been blocked`
**Solution:**
- Add frontend URL to `CorsSettings__AllowedOrigins`
- Restart service after updating environment variables

#### 5. Free Tier Sleep
**Issue:** Service goes to sleep after 15 minutes of inactivity
**Solution:**
- Upgrade to Starter plan ($7/month)
- Or use a service like UptimeRobot to ping your API every 10 minutes

---

## Step 13: Production Checklist

Before going live, ensure:

### Security
- [x] JWT secret is strong and unique
- [x] Database password is secure
- [x] appsettings.Development.json is not deployed (in .dockerignore)
- [x] CORS is configured with specific origins (not "*")
- [x] HTTPS is enabled (automatic on Render)

### Database
- [x] Connection string uses SSL (Supabase requires it)
- [x] Database migrations are applied
- [x] Seed data is present

### API
- [x] All endpoints return correct responses
- [x] Business rules work as expected
- [x] Error handling is implemented
- [x] Swagger is accessible (or disabled in production if desired)

### Monitoring
- [x] Logs are accessible
- [x] Health checks are configured
- [x] Error alerts are set up (optional)

---

## Step 14: Disable Swagger in Production (Optional)

If you want to disable Swagger in production for security:

Update `Program.cs`:
```csharp
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Church Roster API");
        options.WithTheme(ScalarTheme.Kepler);
    });
}
```

---

## Deployment Costs

### Free Tier
- **Cost:** $0/month
- **Limitations:**
  - Service sleeps after 15 minutes of inactivity
  - 750 hours/month (enough for 1 service)
  - Spins up in ~30 seconds when accessed

### Starter Tier
- **Cost:** $7/month
- **Benefits:**
  - Always on (no sleep)
  - 512 MB RAM
  - 0.5 CPU
  - Perfect for small church (< 100 members)

### Standard Tier
- **Cost:** $25/month
- **Benefits:**
  - 2 GB RAM
  - 1 CPU
  - For larger churches

---

## Post-Deployment

### Update Frontend Configuration
Once deployed, update your frontend `.env`:
```
VITE_API_URL=https://church-roster-api.onrender.com
```

### Test All Endpoints
Use the testing guide:
1. Open `https://church-roster-api.onrender.com/scalar/v1`
2. Follow `backend/API_TESTING_GUIDE.md`

### Share with Team
Your API is now live at:
```
https://church-roster-api.onrender.com
```

---

## Continuous Deployment Workflow

1. **Make changes locally**
2. **Commit to Git:**
   ```bash
   git add .
   git commit -m "Description of changes"
   ```
3. **Push to GitHub:**
   ```bash
   git push origin main
   ```
4. **Render auto-deploys** (takes 3-5 minutes)
5. **Verify deployment** in Render logs

---

## Support Resources

- **Render Docs:** https://render.com/docs
- **Docker Docs:** https://docs.docker.com
- **Supabase Docs:** https://supabase.com/docs
- **.NET Deployment:** https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/

---

## Next Steps After Deployment

1. ✅ Verify all endpoints work
2. ✅ Test authentication
3. ✅ Test business rules
4. ✅ Update frontend configuration (Week 3)
5. ✅ Set up monitoring/alerts (optional)
6. ✅ Configure custom domain (optional)

---

**🎉 Congratulations! Your API is now live on the internet!**

*Deployment Guide v1.0 - Updated for .NET 10 and Render*
