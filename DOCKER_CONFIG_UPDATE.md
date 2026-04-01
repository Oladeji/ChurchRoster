# Docker Configuration Update

## Changes Made

### 1. File Reorganization

**Moved Files:**
- `Dockerfile` → `backend/Dockerfile`
- `.dockerignore` → `backend/.dockerignore`

**Updated Files:**
- `docker-compose.yml` - Updated build context to `./backend`
- `.gitignore` - Added docker-compose.yml

---

## Updated Docker Configuration

### Dockerfile Location
```
backend/
├── Dockerfile          ← Backend API Dockerfile
├── .dockerignore       ← Backend Docker ignore rules
├── ChurchRoster.Api/
├── ChurchRoster.Application/
├── ChurchRoster.Core/
└── ChurchRoster.Infrastructure/
```

### Frontend Dockerfile (Future)
```
frontend/
├── Dockerfile          ← Frontend React Dockerfile (to be created)
├── .dockerignore       ← Frontend Docker ignore rules (to be created)
└── src/
```

---

## Updated Deployment Configuration

### Render Deployment Settings

When deploying to Render, use these settings:

| Setting | Value |
|---------|-------|
| **Root Directory** | `backend` |
| **Environment** | `Docker` |
| **Dockerfile Path** | `./Dockerfile` |

**Important:** The Root Directory is now `backend` instead of blank.

---

## Updated docker-compose.yml

The docker-compose.yml now points to the backend folder:

```yaml
services:
  api:
    build:
      context: ./backend    # ← Updated
      dockerfile: Dockerfile
    ports:
      - "8080:8080"
```

**Note:** docker-compose.yml is now in .gitignore because it contains sensitive connection strings.

---

## Testing Locally

### Build Backend Docker Image
```powershell
# From project root
docker build -t church-roster-api ./backend

# Or use docker-compose
docker-compose up
```

### Run Backend Container
```powershell
docker run -p 8080:8080 church-roster-api
```

---

## Benefits of This Structure

### Separation of Concerns
- ✅ Backend Dockerfile in `backend/` folder
- ✅ Frontend Dockerfile in `frontend/` folder (when created)
- ✅ Each has its own `.dockerignore`
- ✅ Clean project structure

### Deployment Flexibility
- ✅ Deploy backend independently
- ✅ Deploy frontend independently
- ✅ Different deployment platforms possible
- ✅ Easier to maintain

### Security
- ✅ docker-compose.yml in .gitignore (contains secrets)
- ✅ Environment-specific configs not committed
- ✅ Sensitive data only in Render environment variables

---

## Next Steps

### For Backend Deployment (Render)
1. Push changes to GitHub
2. Create Render Web Service
3. **Set Root Directory to:** `backend`
4. Configure environment variables
5. Deploy

### For Frontend (Week 3)
When you create the frontend Dockerfile:
1. Create `frontend/Dockerfile`
2. Create `frontend/.dockerignore`
3. Update `docker-compose.yml` to include frontend service
4. Deploy frontend to Vercel or Render

---

## Example Frontend Dockerfile (Future Reference)

```dockerfile
# frontend/Dockerfile
FROM node:20-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
RUN npm run build

FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

---

## Updated Files Summary

| File | Action | Location |
|------|--------|----------|
| `Dockerfile` | Moved | `backend/Dockerfile` |
| `.dockerignore` | Moved | `backend/.dockerignore` |
| `docker-compose.yml` | Updated | Project root |
| `.gitignore` | Updated | Project root |
| `RENDER_DEPLOYMENT_GUIDE.md` | Updated | Project root |

---

## Testing Checklist

- [ ] Build backend Docker image: `docker build -t church-roster-api ./backend`
- [ ] Run container: `docker run -p 8080:8080 church-roster-api`
- [ ] Test API: `curl http://localhost:8080/api/auth/login`
- [ ] Or use docker-compose: `docker-compose up`
- [ ] Verify all endpoints work
- [ ] Push to GitHub
- [ ] Deploy to Render with **Root Directory: `backend`**

---

## Important Notes

### For Render Deployment
⚠️ **Critical:** When creating Web Service on Render, set:
```
Root Directory: backend
Dockerfile Path: ./Dockerfile
```

### For Git
⚠️ **Security:** docker-compose.yml is now ignored because it contains:
- Database connection strings with passwords
- JWT secret keys
- Other sensitive configuration

Create a `docker-compose.example.yml` if you want to share the structure:
```yaml
services:
  api:
    build:
      context: ./backend
      dockerfile: Dockerfile
    environment:
      - ConnectionStrings__DefaultConnection=CHANGE_THIS
      - JwtSettings__SecretKey=CHANGE_THIS
```

---

*Configuration Updated: March 31, 2026*
