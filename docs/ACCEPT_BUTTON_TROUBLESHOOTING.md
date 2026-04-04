# Accept Button Troubleshooting Guide

## Problem
Accept button on notification doesn't work - assignment status doesn't change.

## Diagnostic Tool
**First, use this**: `http://localhost:3000/accept-diagnostic.html`

This page will automatically:
- ✅ Check if you're logged in (auth token exists)
- ✅ Check if Service Worker is active
- ✅ Check if backend is running
- ✅ Let you test the Accept button in isolation
- ✅ Show live console logs
- ✅ Test API endpoint directly

## Common Issues & Solutions

### Issue 1: Service Worker Cache Not Cleared ⚠️ (MOST COMMON)

**Symptoms:**
- No console logs when clicking Accept
- Old behavior persists after code changes

**Fix:**
```javascript
// Paste in browser console (F12 → Console):
navigator.serviceWorker.getRegistration().then(r => {
  if (r) {
    console.log('Unregistering Service Worker...');
    r.unregister();
  }
}).then(() => {
  console.log('Reloading...');
  location.reload();
});
```

**Verify it worked:**
After reload, check console for:
```
[NOTIFICATIONS] ✅ API URL configured: https://localhost:7288/api
[SW] ✅ API URL updated from main app: https://localhost:7288/api
```

---

### Issue 2: Backend Not Running

**Symptoms:**
- Console shows: `Failed to fetch`
- Error: `net::ERR_CONNECTION_REFUSED`

**Fix:**
```powershell
# Start backend
cd backend\ChurchRoster.Api
dotnet run

# Wait for:
# Now listening on: https://localhost:7288
```

**Verify it works:**
Open: `https://localhost:7288/`
Should see Scalar API documentation.

---

### Issue 3: Not Logged In / Token Expired

**Symptoms:**
- Console shows: `[SW] ❌ No auth token found`
- Notification says: "Authentication Required"

**Fix:**
1. Log out of the app
2. Log back in
3. Try Accept button again

**Verify token exists:**
```javascript
// In browser console:
const token = localStorage.getItem('authToken');
console.log('Token:', token ? 'EXISTS' : 'MISSING');
```

---

### Issue 4: Wrong API URL

**Symptoms:**
- Console shows: `404 Not Found`
- Or: Backend running but API calls fail

**Fix:**

Check `.env` file matches backend URL:
```env
# frontend/.env
VITE_API_URL=https://localhost:7288/api
```

Rebuild:
```bash
cd frontend
npm run build
```

**Verify correct URL:**
```javascript
// In browser console:
console.log('API URL:', localStorage.getItem('apiUrl'));
// Should match your backend URL
```

---

### Issue 5: Assignment ID Missing

**Symptoms:**
- Console shows: `[SW] Extracted Assignment ID: undefined`
- Accept handler doesn't trigger

**Fix:**

This means notification data is missing. Check how notification is created.

Should include:
```javascript
data: {
  type: 'new_assignment',
  assignmentId: '123',  // ← Must be present!
  url: '/my-assignments'
}
```

---

### Issue 6: CORS or Certificate Issues

**Symptoms:**
- `net::ERR_CERT_AUTHORITY_INVALID`
- `CORS policy` errors

**Fix for Certificate (Development):**
1. Open `https://localhost:7288/` directly
2. Click "Advanced"
3. Click "Proceed to localhost (unsafe)"
4. This accepts the self-signed certificate

**Fix for CORS (if needed):**
Backend should already have CORS configured, but verify:
```csharp
// backend/ChurchRoster.Api/Program.cs
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", policy => {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

app.UseCors("AllowAll");
```

---

## Step-by-Step Debugging

### Step 1: Open Diagnostic Tool
```
http://localhost:3000/accept-diagnostic.html
```

Click "Run Full Diagnostic"

**Expected results:**
- ✅ Auth Token: Found
- ✅ API URL: https://localhost:7288/api
- ✅ Service Worker: Active
- ✅ Notifications: Granted
- ✅ Backend: Responding

If any are ❌, fix that issue first!

### Step 2: Test Notification

On diagnostic page:
1. Enter Assignment ID (e.g., `1`)
2. Click "Show Test Notification"
3. Click the **Accept** button on notification
4. Watch the "Live Console Logs" section

**Expected logs:**
```
[SW] ========== NOTIFICATION CLICKED ==========
[SW] Action: accept
[SW] Notification Data: {...}
[SW] Extracted Assignment ID: 1
[SW] ✅ ACCEPT ACTION TRIGGERED for assignment: 1
[SW] Attempting to accept assignment: 1
[SW] Requesting auth token from main app...
[NOTIFICATIONS] Service Worker requested auth token: Found
[SW] ✅ Auth token received from client
[SW] Using API URL: https://localhost:7288/api
[SW] Auth token found, sending accept request...
[SW] ✅ Assignment accepted successfully
```

**If you see success log:**
- You should see a new notification: "Assignment Accepted! ✓"
- Assignment status in database should be "Accepted"

### Step 3: Test Direct API Call

On diagnostic page:
1. Click "Test Accept API Direct"
2. Check logs

**If API works directly but not from notification:**
- Service Worker message passing issue
- Check Service Worker is active
- Clear Service Worker cache and retry

**If API fails even when called directly:**
- Backend issue (check backend console for errors)
- Authentication issue (token expired)
- Endpoint doesn't exist (check backend code)

---

## Expected Full Flow

When Accept button works correctly:

1. **User clicks Accept on notification**
2. **Service Worker** receives click event
3. **Service Worker** requests auth token from main app
4. **Main app** responds with token from localStorage
5. **Service Worker** calls backend API:
   ```
   POST https://localhost:7288/api/assignments/{id}/accept
   Authorization: Bearer {token}
   ```
6. **Backend** validates token, updates assignment
7. **Backend** returns success (200 OK)
8. **Service Worker** shows success notification
9. **Service Worker** opens app to /my-assignments
10. **User** sees updated assignment status

---

## Console Logs Reference

### Success Flow:
```
[SW] ========== NOTIFICATION CLICKED ==========
[SW] Action: accept
[SW] Notification Data: {type: "new_assignment", assignmentId: "1", url: "/my-assignments"}
[SW] Notification Tag: church-roster-1
[SW] Extracted Assignment ID: 1
[SW] Assignment ID Type: string
[SW] ✅ ACCEPT ACTION TRIGGERED for assignment: 1
[SW] Attempting to accept assignment: 1
[SW] Requesting auth token from main app...
[SW] Found 1 active client(s), requesting token...
[NOTIFICATIONS] Service Worker requested auth token: Found
[SW] ✅ Auth token received from client
[SW] Auth token found, sending accept request...
[SW] Using API URL: https://localhost:7288/api
[SW] ✅ Assignment accepted successfully
[SW] ✅ Accept completed, opening app...
```

### Error: No Token
```
[SW] Attempting to accept assignment: 1
[SW] Requesting auth token from main app...
[SW] Found 1 active client(s), requesting token...
[NOTIFICATIONS] Service Worker requested auth token: Not found
[SW] ⚠️ Client responded but no token found
[SW] ❌ No auth token found, cannot auto-accept assignment
```
**Fix:** Log in

### Error: Backend Down
```
[SW] Using API URL: https://localhost:7288/api
[SW] ❌ Error accepting assignment: TypeError: Failed to fetch
```
**Fix:** Start backend

### Error: 401 Unauthorized
```
[SW] Using API URL: https://localhost:7288/api
[SW] ❌ Failed to accept assignment: 401 Unauthorized
```
**Fix:** Token expired - log out and log back in

### Error: 404 Not Found
```
[SW] Using API URL: https://localhost:7288/api
[SW] ❌ Failed to accept assignment: 404 Not Found
```
**Fix:** Backend endpoint missing or wrong URL

---

## Quick Checklist

Before reporting issue, verify:

- [ ] Backend is running (`https://localhost:7288/` works)
- [ ] You are logged in (`localStorage.getItem('authToken')` returns a value)
- [ ] Service Worker cache cleared (reloaded after code changes)
- [ ] Notification permission granted
- [ ] Browser console open (F12) to see logs
- [ ] Tried diagnostic tool (`accept-diagnostic.html`)
- [ ] Can see `[SW]` logs in console when clicking Accept

---

## Still Not Working?

If you've tried everything above and it still doesn't work:

1. **Copy ALL console logs** from the moment you click Accept
2. **Screenshot the diagnostic tool results**
3. **Note what happens** when you click Accept
4. **Check backend logs** for any errors

Provide this information for further debugging.

---

**Last Updated:** 2025-01-XX  
**Diagnostic Tool:** `http://localhost:3000/accept-diagnostic.html`  
**Backend Endpoint:** `POST /api/assignments/{id}/accept`

