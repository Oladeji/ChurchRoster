# 🔍 Notification Troubleshooting Guide

## Current Status

✅ Backend compiled with enhanced logging  
❓ Notifications not being received  
🔍 Need to diagnose the issue

## Step-by-Step Debugging

### Step 1: Restart Backend
**Stop and restart your backend in Visual Studio:**
- Press **Shift+F5** to stop
- Press **F5** to start debugging again

This loads the new logging code.

---

### Step 2: Check Frontend - Enable Notifications

**On your member account at http://localhost:3000:**

1. **Open Browser DevTools:**
   - Press **F12**
   - Go to **Console** tab

2. **Look for the notification banner:**
   - Should see a yellow banner at top: "Enable Notifications"
   - If you don't see it, check:
     - Are you logged in as a **member** (not admin)?
     - Check console for errors

3. **Click "Enable" button:**
   - Browser should prompt for notification permission
   - Click **Allow**

4. **Check Console for Success:**
   Look for these messages:
   ```
   ✅ "Device token: [long string]"
   ✅ "Device token saved successfully"
   ```

   **If you see errors instead:**
   - `Firebase Messaging not initialized` → Check Firebase config in `.env`
   - `Permission denied` → You need to grant permission
   - Network error → Backend not reachable

---

### Step 3: Verify Device Token Saved

**In browser console, run:**
```javascript
// Check if token exists in local storage or state
console.log("Checking device token...");
```

**In backend, check database:**
The user record should have a `device_token` value.

---

### Step 4: Create Assignment & Watch Logs

**As admin in another tab:**
1. Create a new assignment for the member
2. Assignment should be created successfully

**Watch Backend Output Window for these logs:**

```
[NOTIFICATION] Starting notification process for assignment 16
[NOTIFICATION] Found device token for [Member Name], sending notification...
ChurchRoster.Application.Services.NotificationService: Information: Sending notification to [token]...
[NOTIFICATION] ✅ Notification sent successfully!
```

---

## Expected Log Patterns

### ✅ Success Pattern:
```
[NOTIFICATION] Starting notification process for assignment 17
[NOTIFICATION] Found device token for John Doe, sending notification...
Firebase initialized from ServiceAccountJson
Firebase Messaging initialized successfully
[NOTIFICATION] ✅ Notification sent successfully!
```

### ⚠️ No Device Token:
```
[NOTIFICATION] Starting notification process for assignment 17
[NOTIFICATION] User John Doe (ID: 3) has NO device token
```
**Fix:** Member needs to click "Enable" button in frontend

### ❌ User Not Found:
```
[NOTIFICATION] Starting notification process for assignment 17
[NOTIFICATION] User 3 not found
```
**Fix:** Check if user_id=3 exists in database

### ❌ Firebase Not Configured:
```
[NOTIFICATION] Starting notification process for assignment 17
[NOTIFICATION] Found device token for John Doe, sending notification...
Firebase not configured. Skipping notification
[NOTIFICATION] ❌ Notification send failed
```
**Fix:** Check `appsettings.json` has `Firebase.ServiceAccountJson`

### ❌ Firebase Error:
```
[NOTIFICATION] Starting notification process for assignment 17
[NOTIFICATION] Found device token for John Doe, sending notification...
[NOTIFICATION] ❌ Exception: FirebaseMessagingException: [error details]
```
**Fix:** Check Firebase credentials, API key, VAPID key

---

## Common Issues & Solutions

### Issue 1: No "[NOTIFICATION]" logs at all

**Symptom:** Assignment created but no notification logs appear

**Possible Causes:**
1. Backend not restarted after code changes
2. Exception thrown before logging starts

**Solution:**
- Restart Visual Studio debugger
- Check for exceptions in Output window

---

### Issue 2: "User has NO device token"

**Symptom:** Log shows user found but no device token

**Root Cause:** Member hasn't enabled notifications in browser

**Solution:**
1. Go to http://localhost:3000 as member
2. Look for yellow notification banner
3. Click "Enable" button
4. Grant browser permission
5. Verify "Device token saved successfully" in console
6. Try creating assignment again

---

### Issue 3: Device token saved but still no notification

**Symptom:** Logs show notification sent successfully, but browser doesn't show notification

**Possible Causes:**
1. **Service worker not registered**
   - Check: DevTools → Application tab → Service Workers
   - Should see `firebase-messaging-sw.js` active

2. **Browser notifications blocked**
   - Check: Browser address bar for blocked notification icon
   - Go to: `chrome://settings/content/notifications`
   - Ensure localhost:3000 is allowed

3. **Firebase VAPID key mismatch**
   - Frontend `.env` has: `VITE_FIREBASE_VAPID_KEY`
   - Must match Firebase Console web push certificate

4. **Wrong device token**
   - Token was generated for different browser/device
   - Solution: Clear token, re-enable notifications

---

### Issue 4: Firebase Exception

**Symptom:** `FirebaseMessagingException` or `FirebaseException` in logs

**Common Errors:**

**"Invalid APNs credentials":**
- Ignore if you're not using iOS

**"Registration token is invalid":**
- Device token expired or from different project
- Solution: Member re-enables notifications

**"The default FirebaseApp does not exist":**
- Firebase not initialized properly
- Solution: Check service account JSON in appsettings.json

**"Requested entity was not found":**
- Project ID mismatch or wrong credentials
- Solution: Verify Firebase project ID matches

---

## Testing Checklist

Run through this checklist in order:

### Backend Checks:
- [ ] Backend restarted after code changes
- [ ] Sees "Firebase initialized from ServiceAccountJson" on startup
- [ ] No errors in Output window on startup

### Frontend Checks (as Member):
- [ ] Logged in as member (not admin)
- [ ] Yellow notification banner visible on Dashboard
- [ ] Clicked "Enable" button
- [ ] Browser prompted for permission
- [ ] Granted notification permission
- [ ] Console shows "Device token saved successfully"
- [ ] Service worker registered (F12 → Application → Service Workers)

### Integration Test:
- [ ] Admin creates assignment for member
- [ ] Assignment created successfully (201 response)
- [ ] Backend logs show "[NOTIFICATION] Starting..."
- [ ] Backend logs show "[NOTIFICATION] Found device token..."
- [ ] Backend logs show "[NOTIFICATION] ✅ Notification sent successfully!"
- [ ] **Member's browser shows notification!** 🔔

---

## Quick Test Commands

### Check Frontend Console:
```javascript
// Check notification permission
console.log("Permission:", Notification.permission);

// Check if service worker registered
navigator.serviceWorker.getRegistrations().then(regs => {
  console.log("Service workers:", regs.length);
  regs.forEach(reg => console.log("- ", reg.scope));
});
```

### Expected Output:
```
Permission: granted
Service workers: 1
-  http://localhost:3000/
```

---

## Next Steps

1. **Restart backend** (Shift+F5, then F5)
2. **As member:** Enable notifications in browser
3. **Watch backend logs** for `[NOTIFICATION]` messages
4. **As admin:** Create assignment
5. **Report back:** What logs do you see?

Tell me:
- ✅ Did you see "[NOTIFICATION] Starting..." logs?
- ✅ Did it find the device token?
- ✅ Did Firebase send successfully?
- ❌ Any errors or exceptions?

This will help me pinpoint exactly where the issue is!
