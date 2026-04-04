# API URL Configuration Fix

## Problem Identified ✅

The Accept button wasn't working because:
- ❌ Service Worker was calling `http://localhost:8080/api` 
- ✅ Backend is actually running on `https://localhost:7288/api`
- ❌ URL mismatch caused all API calls to fail

## Root Cause

The Service Worker (`firebase-messaging-sw.js`) had a hardcoded URL that didn't match the actual backend configuration in `.env`:

```javascript
// ❌ WRONG - Old hardcoded URL
const response = await fetch(`http://localhost:8080/api/assignments/${assignmentId}/accept`, {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  }
});
```

## Solution Applied ✅

### Files Updated:

1. **`frontend/public/firebase-messaging-sw.js`** (Line 160)
   - Changed: `http://localhost:8080/api` → `https://localhost:7288/api`
   - Now matches `.env` configuration

2. **`frontend/public/health-check.html`**
   - Updated API URL check to use `https://localhost:7288/api`
   - Will now correctly detect backend status

3. **`frontend/public/test-accept.html`**
   - Updated test API call to use `https://localhost:7288/api`
   - Test page will now work correctly

## Configuration Reference

Your backend is configured in **`frontend/.env`**:
```env
VITE_API_URL=https://localhost:7288/api
```

This is the URL your backend is running on (verified via Scalar at `https://localhost:7288/`).

## Testing Steps

### 1. Clear Service Worker Cache (IMPORTANT!)

Since we updated the Service Worker code, you MUST clear the cache:

**In browser console (F12):**
```javascript
navigator.serviceWorker.getRegistration().then(r => {
  if (r) r.unregister();
}).then(() => location.reload());
```

### 2. Verify Health Check

Go to: `http://localhost:3000/health-check.html`

Should now show:
- ✅ Auth Token: Found
- ✅ Service Worker: Active
- ✅ Backend API: Running on https://localhost:7288/api

### 3. Test Accept Functionality

**Option A: Use Test Page**
1. Go to: `http://localhost:3000/test-accept.html`
2. Enter an assignment ID
3. Click "Test Accept API"
4. Should see: `✅ Accept API SUCCESS!`

**Option B: Real Notification**
1. Go to your main app
2. Create a new assignment
3. Click **Accept** button on notification
4. Should see: "Assignment Accepted! ✓"
5. Assignment status should change to "Accepted"

## Expected Console Logs

When Accept button works correctly:

```
[SW] ========== NOTIFICATION CLICKED ==========
[SW] Action: accept
[SW] Notification Data: {type: "new_assignment", assignmentId: "1", ...}
[SW] Extracted Assignment ID: 1
[SW] ✅ ACCEPT ACTION TRIGGERED for assignment: 1
[SW] Attempting to accept assignment: 1
[SW] Requesting auth token from main app...
[SW] Found 1 active client(s), requesting token...
[NOTIFICATIONS] Service Worker requested auth token: Found
[SW] ✅ Auth token received from client
[SW] Auth token found, sending accept request...
[SW] ✅ Assignment accepted successfully
[SW] ✅ Accept completed, opening app...
```

## Troubleshooting

### Still getting "Failed to fetch"?

**Check 1: Backend is running**
```powershell
Test-NetConnection -ComputerName localhost -Port 7288
```
Should return: `TcpTestSucceeded: True`

**Check 2: Verify backend URL**
Open Scalar: `https://localhost:7288/`
Should see API documentation

**Check 3: Clear browser cache**
- Press Ctrl+Shift+Delete
- Clear "Cached images and files"
- Restart browser

### Health check shows backend not responding?

**Issue**: Browser might block HTTPS with self-signed certificate

**Solution**: 
1. Visit `https://localhost:7288/` directly
2. Accept the certificate warning (Advanced → Proceed)
3. Then test health check again

### Accept button still doesn't work?

**Double-check Service Worker update:**
```javascript
// In browser console:
navigator.serviceWorker.getRegistration().then(reg => {
  console.log('Active SW URL:', reg.active.scriptURL);
  reg.update().then(() => {
    console.log('SW updated, reloading...');
    location.reload();
  });
});
```

## Summary

✅ **Fixed**: Service Worker now uses correct API URL (`https://localhost:7288/api`)  
✅ **Fixed**: Health check detects backend correctly  
✅ **Fixed**: Test page uses correct API URL  
✅ **Next**: Clear Service Worker cache and test  

**After clearing cache, the Accept button should work!** 🎉

---

**Last Updated**: 2025-01-XX  
**Backend URL**: https://localhost:7288/api  
**Frontend URL**: http://localhost:3000  
**Status**: Ready for testing
