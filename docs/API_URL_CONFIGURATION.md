# Single Source of Truth for API URL Configuration

## Overview

The application now uses a **single source of truth** for the API URL configuration: the `.env` file's `VITE_API_URL` variable. This eliminates the need to update URLs in multiple places when deploying to different environments.

## How It Works

### 1. Configuration Source (`.env`)

```env
# frontend/.env
VITE_API_URL=https://localhost:7288/api
```

**For deployment**, simply update this one line:
```env
# Production
VITE_API_URL=https://api.yourchurch.com/api

# Staging
VITE_API_URL=https://staging-api.yourchurch.com/api

# Development
VITE_API_URL=https://localhost:7288/api
```

### 2. Distribution Flow

```
.env (VITE_API_URL)
    ↓
    ├─→ Main App (api.service.ts)
    ├─→ Service Worker (firebase-messaging-sw.js)
    ├─→ localStorage (for test pages)
    └─→ Test Pages (test-accept.html, health-check.html)
```

### 3. Implementation Details

#### A. Main Application

**File: `frontend/src/services/api.service.ts`**
```typescript
const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';
```
Uses Vite's `import.meta.env` to read from `.env` file.

#### B. Service Worker

**File: `frontend/public/firebase-messaging-sw.js`**
```javascript
// Default fallback
let API_URL = 'https://localhost:7288/api';

// Receives URL from main app at runtime
self.addEventListener('message', (event) => {
  if (event.data && event.data.type === 'SET_API_URL') {
    API_URL = event.data.apiUrl; // ✅ Updated from .env
    console.log('[SW] ✅ API URL updated from main app:', API_URL);
  }
});
```

**File: `frontend/src/hooks/useNotifications.ts`**
```typescript
// Sends API URL to Service Worker on app startup
const apiUrl = import.meta.env.VITE_API_URL || 'https://localhost:7288/api';

// Store for test pages
localStorage.setItem('apiUrl', apiUrl);

// Send to Service Worker
registration.active?.postMessage({
  type: 'SET_API_URL',
  apiUrl: apiUrl
});
```

#### C. Test Pages

**File: `frontend/public/config.js`**
```javascript
const getApiUrl = () => {
  // Read from localStorage (set by main app from .env)
  const storedUrl = localStorage.getItem('apiUrl');
  return storedUrl || 'https://localhost:7288/api'; // Fallback
};

window.APP_CONFIG = {
  API_URL: getApiUrl()
};
```

**Files: `test-accept.html`, `health-check.html`**
```html
<script src="/config.js"></script>
<script>
  const API_URL = window.APP_CONFIG.API_URL; // ✅ Uses .env config
</script>
```

## Deployment Guide

### Development Environment

```env
# frontend/.env
VITE_API_URL=https://localhost:7288/api
```

### Production Deployment

1. **Update `.env` file** (or use environment-specific files):

```env
# frontend/.env.production
VITE_API_URL=https://api.yourchurch.com/api
```

2. **Build with production env**:

```bash
npm run build
# Vite automatically uses .env.production for production builds
```

3. **Verify build output**:

```javascript
// Check compiled code contains correct URL
console.log(import.meta.env.VITE_API_URL);
```

### Environment-Specific Configuration

Vite supports multiple `.env` files:

```
frontend/
├── .env                  # Base config (all environments)
├── .env.local            # Local overrides (gitignored)
├── .env.development      # Development
├── .env.production       # Production
└── .env.staging          # Staging (custom)
```

**Priority**: `.env.[mode].local` > `.env.[mode]` > `.env.local` > `.env`

## Benefits

✅ **Single Point of Configuration**: Change API URL in one place (`.env`)  
✅ **Environment-Specific**: Different URLs for dev/staging/prod  
✅ **No Code Changes**: Switch environments without touching code  
✅ **Type-Safe**: TypeScript knows about `import.meta.env`  
✅ **Consistent**: Same URL across main app, Service Worker, and test pages  
✅ **CI/CD Friendly**: Easy to inject environment variables during build  

## Verification

### 1. Check Main App

```javascript
// Browser console
console.log('API Service URL:', 
  document.querySelector('script')?.textContent?.match(/baseURL:\s*['"]([^'"]+)['"]/)?.[1]
);
```

### 2. Check Service Worker

```javascript
// Browser console
navigator.serviceWorker.controller.postMessage({ type: 'GET_CONFIG' });

// In Service Worker (add this for debugging):
self.addEventListener('message', (event) => {
  if (event.data.type === 'GET_CONFIG') {
    console.log('[SW] Current API URL:', API_URL);
  }
});
```

### 3. Check Test Pages

Open: `http://localhost:3000/health-check.html`

Should display:
```
✅ Backend API
Running on https://localhost:7288/api
Source: .env VITE_API_URL
```

### 4. Check Build Output

After `npm run build`, check the Service Worker receives the URL:

```
[SW] ✅ API URL updated from main app: https://your-domain.com/api
[SW] API URL source: .env VITE_API_URL
```

## Console Logs

When configured correctly, you should see:

```
[NOTIFICATIONS] ✅ API URL configured: https://localhost:7288/api
[NOTIFICATIONS] Source: .env VITE_API_URL
[SW] ✅ API URL updated from main app: https://localhost:7288/api
[SW] API URL source: .env VITE_API_URL
[CONFIG] API URL: https://localhost:7288/api
```

## Troubleshooting

### Service Worker still uses old URL?

**Clear Service Worker cache:**
```javascript
navigator.serviceWorker.getRegistration().then(r => {
  if (r) r.unregister();
}).then(() => location.reload());
```

### Test pages show wrong URL?

**Check localStorage:**
```javascript
console.log('Stored API URL:', localStorage.getItem('apiUrl'));
```

**Fix**: Open main app first so it sets localStorage, then open test pages.

### Build uses wrong URL?

**Check which `.env` file is being used:**
```bash
# Development build
npm run dev
# Uses: .env.development or .env

# Production build
npm run build
# Uses: .env.production or .env
```

## Migration Notes

### Before (Hardcoded):
```javascript
// ❌ Multiple places to update
const API_URL = 'http://localhost:8080/api'; // Service Worker
const API_URL = 'http://localhost:8080/api'; // test-accept.html
const API_URL = 'http://localhost:8080/api'; // health-check.html
baseURL: 'http://localhost:5000/api',         // api.service.ts
```

### After (Single Source):
```env
# ✅ One place to update
VITE_API_URL=https://your-domain.com/api
```

---

**Last Updated**: 2025-01-XX  
**Status**: ✅ Implemented - Single source of truth for API URL  
**Configuration File**: `frontend/.env`  
**Key**: `VITE_API_URL`

