# Environment Variable Injection - Complete Explanation

## The Problem

**Question**: Why is there still a hardcoded value in `firebase-messaging-sw.js`?

```javascript
let API_URL = 'https://localhost:7288/api'; // Fallback default
```

**Answer**: Service Workers **cannot** directly access `.env` files or `import.meta.env` because they run in a separate browser context outside the main app bundle.

---

## The Solution (2-Layer Approach)

We use **both** build-time injection **AND** runtime updates for maximum reliability:

### Layer 1: Build-Time Injection ✅ (Primary)

**File**: `frontend/vite.config.ts`

During `npm run build`, a Vite plugin:
1. Reads `VITE_API_URL` from `.env`
2. Opens `public/firebase-messaging-sw.js`
3. **Replaces** the hardcoded URL with `.env` value
4. Copies the modified file to `dist/`

**Result**:
```javascript
// Source (before build):
let API_URL = 'https://localhost:7288/api'; // Fallback default

// Dist (after build):
let API_URL = 'https://api.yourchurch.com/api'; // Injected from .env VITE_API_URL at build time
```

### Layer 2: Runtime Update ✅ (Backup)

**File**: `frontend/src/hooks/useNotifications.ts`

When the app starts, it:
1. Reads `VITE_API_URL` from `.env`
2. Sends it to the Service Worker via `postMessage`
3. Service Worker updates `API_URL` variable

**Why both layers?**
- **Build-time**: Service Worker has correct URL immediately when installed
- **Runtime**: Allows hot-swapping URL without reinstalling Service Worker (useful for development)

---

## Complete Flow Diagram

```
┌─────────────────────────────────────────────────────┐
│                  .env FILE                          │
│  VITE_API_URL=https://api.yourchurch.com/api      │
└─────────────────────────────────────────────────────┘
                       │
                       ├──────────────────┬───────────────────┐
                       │                  │                   │
                       ▼                  ▼                   ▼
           ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐
           │  Build Time     │  │  Main App       │  │  Test Pages  │
           │  (Vite Plugin)  │  │  (Runtime)      │  │  (Runtime)   │
           └─────────────────┘  └─────────────────┘  └──────────────┘
                   │                     │                    │
                   │                     │                    │
                   ▼                     ▼                    ▼
           ┌─────────────────┐  ┌──────────────────┐ ┌──────────────┐
           │ Inject into SW  │  │ postMessage('    │ │ localStorage │
           │ during build    │  │   SET_API_URL')  │ │ .setItem()   │
           └─────────────────┘  └──────────────────┘ └──────────────┘
                   │                     │                    │
                   └──────────┬──────────┘                    │
                              ▼                               ▼
                   ┌─────────────────────┐         ┌──────────────────┐
                   │  Service Worker     │         │  config.js       │
                   │  API_URL updated    │         │  reads from      │
                   │                     │         │  localStorage    │
                   └─────────────────────┘         └──────────────────┘
```

---

## How to Use It

### Development

**File**: `frontend/.env`
```env
VITE_API_URL=https://localhost:7288/api
```

Build and run:
```bash
npm run build  # Injects https://localhost:7288/api
npm run dev    # Or serve the built files
```

### Production

**File**: `frontend/.env.production`
```env
VITE_API_URL=https://api.yourchurch.com/api
```

Build for production:
```bash
npm run build  # Injects https://api.yourchurch.com/api
```

**Verify injection worked**:
```bash
# Check the built Service Worker
cat frontend/dist/firebase-messaging-sw.js | grep "let API_URL"

# Should show:
# let API_URL = 'https://api.yourchurch.com/api'; // Injected from .env...
```

---

## Vite Plugin Explanation

**File**: `frontend/vite.config.ts`

```typescript
function injectEnvToServiceWorker(): Plugin {
  return {
    name: 'inject-env-to-sw',
    apply: 'build', // Only run during build, not dev
    closeBundle() {
      // After build completes...

      // 1. Read the original Service Worker from public/
      const sourcePath = path.join(publicDir, 'firebase-messaging-sw.js')
      let content = fs.readFileSync(sourcePath, 'utf-8')

      // 2. Get API URL from process.env (Vite loads this from .env)
      const apiUrl = process.env.VITE_API_URL || 'https://localhost:7288/api'

      // 3. Replace the placeholder with actual value
      content = content.replace(
        /let API_URL = ['"].*?['"];.*?\/\/ Fallback default/,
        `let API_URL = '${apiUrl}'; // Injected from .env VITE_API_URL at build time`
      )

      // 4. Write modified version to dist/
      const destPath = path.join(distDir, 'firebase-messaging-sw.js')
      fs.writeFileSync(destPath, content, 'utf-8')

      console.log(`✅ Service Worker API URL injected: ${apiUrl}`)
    }
  }
}
```

**Key Points**:
- Runs **after** build completes (`closeBundle` hook)
- Reads from `process.env.VITE_API_URL` (loaded from `.env`)
- Uses regex to find and replace the URL
- Copies modified file to `dist/` folder

---

## Build Output

When you run `npm run build`, you should see:

```bash
✓ built in 581ms
✅ firebase-messaging-sw.js copied and API URL injected: https://localhost:7288/api
✅ config.js copied and API URL injected: https://localhost:7288/api
```

This confirms:
1. Build succeeded
2. Service Worker was processed
3. API URL was injected from `.env`
4. Config.js was also updated

---

## Verification Steps

### 1. Check Source File

```bash
# Original file still has placeholder
cat frontend/public/firebase-messaging-sw.js | grep "let API_URL"
# Output: let API_URL = 'https://localhost:7288/api'; // Fallback default
```

### 2. Check Built File

```bash
# Built file has injected value
cat frontend/dist/firebase-messaging-sw.js | grep "let API_URL"
# Output: let API_URL = 'https://api.yourchurch.com/api'; // Injected from .env VITE_API_URL at build time
```

### 3. Runtime Verification

Open browser console after deploying:
```javascript
// Check Service Worker received config
// Should log:
// [SW] ✅ API URL updated from main app: https://api.yourchurch.com/api
```

---

## Why This Approach is Better

### Before (Hardcoded):
```javascript
// ❌ Must manually update in code for each environment
let API_URL = 'https://localhost:7288/api';
```

**Problems**:
- Requires code changes for deployment
- Easy to forget to update
- Different builds needed for different environments
- Risk of deploying dev URL to production

### After (Injected):
```javascript
// ✅ Automatically replaced during build from .env
let API_URL = 'https://api.yourchurch.com/api'; // Injected from .env VITE_API_URL at build time
```

**Benefits**:
- ✅ No code changes needed
- ✅ Single source of truth (`.env`)
- ✅ Environment-specific builds automatic
- ✅ Can't accidentally deploy wrong URL
- ✅ Works with CI/CD pipelines
- ✅ Service Worker gets correct URL immediately

---

## Environment-Specific Builds

### Using Different .env Files

Vite automatically loads the correct `.env` file:

```
frontend/
├── .env                    # Base config
├── .env.development        # Dev build (npm run dev)
├── .env.production         # Prod build (npm run build)
├── .env.staging           # Staging build
```

### Build for Staging

```bash
# Create .env.staging:
VITE_API_URL=https://staging-api.yourchurch.com/api

# Build:
vite build --mode staging

# Result:
✅ Service Worker API URL injected: https://staging-api.yourchurch.com/api
```

---

## CI/CD Integration

### GitHub Actions Example

```yaml
- name: Build Frontend
  env:
    VITE_API_URL: ${{ secrets.PRODUCTION_API_URL }}
  run: |
    cd frontend
    npm ci
    npm run build

# The build will use the secret value from GitHub
# No .env file needed in repository!
```

### Docker Example

```dockerfile
ARG VITE_API_URL
ENV VITE_API_URL=$VITE_API_URL

RUN npm run build
# Uses the build arg passed during docker build
```

---

## Troubleshooting

### Issue: Built Service Worker still has old URL

**Cause**: Build plugin didn't run or failed

**Solution**:
```bash
# Clean and rebuild
rm -rf frontend/dist
npm run build

# Check for injection messages:
# ✅ firebase-messaging-sw.js copied and API URL injected: [URL]
```

### Issue: Runtime update shows different URL

**Cause**: `.env` file has different value than expected

**Solution**:
```bash
# Check what's in .env
cat frontend/.env | grep VITE_API_URL

# Verify build used correct value
cat frontend/dist/firebase-messaging-sw.js | grep "let API_URL"
```

### Issue: Service Worker not updating in browser

**Cause**: Old Service Worker cached

**Solution**:
```javascript
// Clear Service Worker
navigator.serviceWorker.getRegistration().then(r => {
  if (r) r.unregister();
}).then(() => location.reload());
```

---

## Summary

**Question**: *"Can't the Service Worker just use the value from .env directly?"*

**Answer**: **No** - Service Workers can't access `.env` or `import.meta.env`.

**Solution**: **Yes** - We inject the `.env` value into the Service Worker file **during build**:

1. 📝 Edit `.env`: `VITE_API_URL=https://your-url.com/api`
2. 🔨 Run build: `npm run build`
3. ✨ Vite plugin automatically replaces hardcoded URL
4. 📦 Deployed Service Worker has correct URL
5. 🚀 No code changes needed!

**Best Practice**: 
- Keep the "Fallback default" comment in source code
- It makes it clear this value gets replaced during build
- The fallback only runs if build injection fails (very rare)

---

**Last Updated**: 2025-01-XX  
**Status**: ✅ Implemented - Build-time injection working  
**Build Output**: Check for "✅ firebase-messaging-sw.js copied and API URL injected"

