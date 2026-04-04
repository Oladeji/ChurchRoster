# Church Roster System - Quick Fix for Accept Button

## Problem Found ✅

The Accept button isn't working because:
1. ❌ **Backend API is NOT running** - Port 8080 is not responding
2. ❌ **Service Worker cache** - Old version is waiting to activate

## Solution

### Quick Fix (Recommended)

**Option 1: Use the startup script**
```powershell
# Run this in the project root:
.\start-dev.ps1
```
This will start both backend and frontend in separate windows.

---

**Option 2: Manual startup**

**Terminal 1 - Backend:**
```powershell
cd backend\ChurchRoster.Api
dotnet run
```
Wait for: `Now listening on: http://localhost:8080`

**Terminal 2 - Frontend:**
```powershell
cd frontend
npm run dev
```
Wait for: `Local: http://localhost:5173/`

---

### Clear Service Worker Cache

Once backend is running, update the Service Worker:

**In browser console (F12):**
```javascript
navigator.serviceWorker.getRegistration().then(r => {
  if (r.waiting) r.waiting.postMessage({type:'SKIP_WAITING'});
  return r.unregister();
}).then(() => location.reload());
```

---

### Test the Fix

1. **Test API directly:**
   - Go to: http://localhost:5173/test-accept.html
   - Click "Test Accept API"
   - Should see: `✅ Accept API SUCCESS!`

2. **Test notification button:**
   - Click "Show Test Notification"
   - Click the **Accept** button on notification
   - Should see: "Assignment Accepted! ✓"

3. **Test in real app:**
   - Go to main app
   - Create a new assignment
   - Click **Accept** on notification
   - Assignment status should change to "Accepted"

---

## Expected Console Logs (When Working)

When you click the Accept button, you should see:

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

And you should see a notification:
```
Title: "Assignment Accepted! ✓"
Body: "You have successfully accepted the assignment!"
```

---

## Troubleshooting

### Still Getting "Failed to fetch"?
- ✅ Make sure backend is running: `Test-NetConnection localhost -Port 8080`
- ✅ Check backend logs for errors
- ✅ Verify URL in Service Worker is `http://localhost:8080`

### Accept button does nothing?
- ✅ Clear Service Worker cache (see above)
- ✅ Check browser console for errors
- ✅ Verify auth token exists: `localStorage.getItem('authToken')`

### "No auth token found"?
- ✅ Log out and log back in
- ✅ Check token in console: `localStorage.getItem('authToken')`

---

## Test Results from Your Session

```
✅ Auth token: FOUND
   User ID: 3
   Email: akomspatrick@yahoo.com
   Role: Member

⚠️ Service Worker: Old version waiting
   Fix: Clear cache and reload

❌ Backend API: NOT RUNNING
   Error: Failed to fetch
   Fix: Start backend with 'dotnet run'
```

---

## Next Steps

1. ✅ Start backend (`dotnet run`)
2. ✅ Clear Service Worker cache
3. ✅ Test API endpoint (should succeed)
4. ✅ Test notification Accept button
5. ✅ Verify assignment status changes

---

**Last Updated:** 2025-01-XX  
**Status:** Root cause identified - Backend not running  
**Solution:** Start backend + clear SW cache
