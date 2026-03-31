# Render Deployment Checklist

Use this checklist to ensure a smooth deployment to Render.

---

## Pre-Deployment

### Code Preparation
- [ ] All code is committed to Git
- [ ] `.gitignore` is configured (excludes sensitive files)
- [ ] `Dockerfile` is at project root
- [ ] `.dockerignore` is configured
- [ ] Build succeeds locally: `dotnet build`
- [ ] All tests pass (if any)

### GitHub Setup
- [ ] Repository created on GitHub
- [ ] Code pushed to `main` branch
- [ ] Repository is accessible (public or Render has permission)

### Database Setup
- [ ] Supabase database is created
- [ ] All migrations are applied
- [ ] Seed data is present (admin user, skills, tasks)
- [ ] Connection string is available
- [ ] SSL is enabled on connection string

### Configuration
- [ ] JWT secret key generated (64+ characters)
- [ ] Environment variable names documented
- [ ] CORS origins identified (for Week 3)
- [ ] Email settings prepared (optional, for Week 5)

---

## Render Setup

### Account & Service
- [ ] Render account created
- [ ] GitHub connected to Render
- [ ] New Web Service created
- [ ] Repository selected
- [ ] Branch set to `main`

### Service Configuration
- [ ] Name: `church-roster-api` (or custom)
- [ ] Region selected
- [ ] Environment: `Docker`
- [ ] Dockerfile path: `./Dockerfile`
- [ ] Instance type selected (Free or Starter)

### Environment Variables (Required)
Copy these to Render environment variables section:

| Variable | Value | Status |
|----------|-------|--------|
| `ASPNETCORE_ENVIRONMENT` | `Production` | [ ] |
| `ConnectionStrings__DefaultConnection` | `Host=db.xxx.supabase.co;Database=postgres;...` | [ ] |
| `JwtSettings__SecretKey` | Your 64-char secret | [ ] |
| `JwtSettings__Issuer` | `ChurchRosterApi` | [ ] |
| `JwtSettings__Audience` | `ChurchRosterClient` | [ ] |
| `JwtSettings__ExpirationInMinutes` | `1440` | [ ] |

**Note:** Use double underscores `__` for nested configuration (not `:`)

---

## Deployment

### Initial Deploy
- [ ] Click "Create Web Service"
- [ ] Monitor build logs
- [ ] Wait for "Your service is live 🎉"
- [ ] Note your service URL (e.g., `https://church-roster-api.onrender.com`)

### Build Verification
Look for these in logs:
- [ ] `Building Dockerfile...`
- [ ] `Successfully built`
- [ ] `Your service is live`
- [ ] No errors in logs

---

## Post-Deployment Testing

### Basic Tests
- [ ] API is accessible: `https://your-service.onrender.com`
- [ ] Health check passes: `GET /`
- [ ] Swagger is accessible: `https://your-service.onrender.com/scalar/v1`

### Authentication Tests
```bash
# Test login endpoint
curl https://YOUR-SERVICE.onrender.com/api/auth/login \
  -X POST \
  -H 'Content-Type: application/json' \
  -d '{"email":"admin@church.com","password":"Admin@123"}'
```
- [ ] Login returns JWT token
- [ ] Token is valid (can decode at jwt.io)

### CRUD Tests
- [ ] GET `/api/members` returns data
- [ ] GET `/api/skills` returns 7 skills
- [ ] GET `/api/tasks` returns 8 tasks
- [ ] POST `/api/members` creates new member
- [ ] Business rules validation works

---

## Configuration Updates

### CORS (for Week 3)
When frontend is deployed, add environment variable:
```
CorsSettings__AllowedOrigins__0=https://your-frontend.vercel.app
```
- [ ] Frontend domain added to CORS
- [ ] Service restarted after update
- [ ] CORS working (no errors in browser console)

### Custom Domain (Optional)
- [ ] Custom domain added in Render
- [ ] DNS records configured
- [ ] SSL certificate issued (automatic)
- [ ] Domain accessible

---

## Monitoring & Maintenance

### Health Monitoring
- [ ] Logs are accessible in Render dashboard
- [ ] No errors in logs
- [ ] Service responds within 5 seconds (cold start)
- [ ] Database connection stable

### Performance
- [ ] Response times acceptable (< 2 seconds)
- [ ] No timeout errors
- [ ] Memory usage stable
- [ ] CPU usage normal

### Auto-Deploy
- [ ] Auto-deploy enabled (deploys on push to main)
- [ ] Test deployment by pushing a small change
- [ ] Verify automatic build and deploy

---

## Security Checklist

### Secrets & Keys
- [ ] JWT secret is strong (64+ characters)
- [ ] Database password is secure
- [ ] Environment variables are set (not hardcoded)
- [ ] No secrets in appsettings.json
- [ ] appsettings.Development.json not deployed

### Network Security
- [ ] HTTPS enabled (automatic on Render)
- [ ] CORS configured with specific origins
- [ ] Database uses SSL connection
- [ ] No sensitive data in logs

### API Security
- [ ] Authentication endpoints working
- [ ] JWT tokens expire (24 hours)
- [ ] Admin endpoints require admin role (add in Week 3)
- [ ] Input validation working
- [ ] SQL injection prevented (EF Core parameterized queries)

---

## Troubleshooting Guide

### If Build Fails
1. [ ] Check Dockerfile syntax
2. [ ] Verify all .csproj files are copied
3. [ ] Check Directory.Packages.props is included
4. [ ] Review build logs for specific errors

### If Service Won't Start
1. [ ] Verify environment variables are set correctly
2. [ ] Check connection string format
3. [ ] Review startup logs for exceptions
4. [ ] Ensure database is accessible

### If Database Connection Fails
1. [ ] Verify connection string: `ConnectionStrings__DefaultConnection`
2. [ ] Check Supabase database is running
3. [ ] Confirm SSL Mode is set: `SSL Mode=Require`
4. [ ] Test connection locally with same string

### If Endpoints Return 500 Errors
1. [ ] Check application logs in Render
2. [ ] Verify database migrations applied
3. [ ] Check seed data exists
4. [ ] Test endpoints locally first

---

## Rollback Plan

If deployment fails:
1. [ ] Note the error in Render logs
2. [ ] Revert to previous working commit:
   ```bash
   git revert HEAD
   git push origin main
   ```
3. [ ] Render auto-deploys previous version
4. [ ] Fix issue locally, test, then redeploy

---

## Weekly Maintenance

### Every Week
- [ ] Check service is running
- [ ] Review error logs
- [ ] Monitor response times
- [ ] Check database performance

### Monthly
- [ ] Review cost (free tier limits)
- [ ] Check for .NET updates
- [ ] Review security best practices
- [ ] Backup database (Supabase has automatic backups)

---

## Support & Resources

### Documentation
- [ ] `RENDER_DEPLOYMENT_GUIDE.md` - Detailed deployment guide
- [ ] `backend/WEEK2_COMPLETE.md` - API feature documentation
- [ ] `backend/API_TESTING_GUIDE.md` - Testing workflow

### External Resources
- Render Docs: https://render.com/docs
- Supabase Docs: https://supabase.com/docs
- .NET Deployment: https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/

### Getting Help
- Render Community: https://community.render.com
- Stack Overflow: Tag with `render` and `dotnet`
- GitHub Issues: Your repository

---

## Success Criteria

Your deployment is successful when:
- ✅ API is accessible at `https://your-service.onrender.com`
- ✅ All endpoints return correct responses
- ✅ Authentication works (can login and get JWT)
- ✅ Business rules are enforced
- ✅ Swagger/Scalar UI is accessible
- ✅ No errors in logs
- ✅ Database connection is stable
- ✅ Auto-deploy works on git push

---

## Next Steps After Successful Deployment

1. [ ] Update frontend `.env` with API URL (Week 3)
2. [ ] Test all endpoints from Swagger UI
3. [ ] Share API URL with team
4. [ ] Set up monitoring/alerts (optional)
5. [ ] Document any deployment-specific configurations
6. [ ] Celebrate! 🎉 Your backend is live!

---

**Deployment Status:** 
- [ ] Not Started
- [ ] In Progress
- [ ] Deployed
- [ ] Tested
- [ ] Live in Production

**Deployed URL:** `_______________________`

**Deployment Date:** `_______________________`

---

*Deployment Checklist v1.0 - Church Roster System*
