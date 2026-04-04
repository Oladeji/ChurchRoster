# 🔧 Device Token Not Saving - Debugging Guide

## 🎯 Root Cause Identified

**Database shows:** All device_token values are **NULL**  
**This means:** Frontend is failing to save the token to the backend

## 🔍 Enhanced Logging Added

I've added detailed console logging to help us diagnose the exact issue.

---

## 📋 Testing Steps

### Step 1: Clear Everything and Start Fresh

**In your browser (http://localhost:3000):**

1. **Open DevTools** (F12)
2. **Go to Application tab**
3. **Clear all:**
   - Click "Clear site data" button
   - OR manually clear:
     - Local Storage → Delete all
     - Session Storage → Delete all
     - Cookies → Delete all
4. **Refresh the page** (Ctrl+R)

### Step 2: Login as Member

1. Login with member credentials
2. Make sure you see the Dashboard

### Step 3: Enable Notifications & Watch Console

**Before clicking "Enable", make sure Console tab is open!**

1. Go to **Console** tab in DevTools
2. Click the **"Enable"** button on yellow banner
3. Click **"Allow"** in browser permission dialog

**Watch for these logs in Console:**

---

## ✅ Expected Success Pattern:

```javascript
[NOTIFICATIONS] Requesting permission...
// Firebase logs...
[NOTIFICATIONS] ✅ Permission granted, device token received: daBcDeFgHiJkLmNoPqRsTuVwXyZ...
[NOTIFICATIONS] Saving device token to backend...
POST https://localhost:7288/api/members/device-token 200 OK
[NOTIFICATIONS] ✅ Device token saved to backend successfully
```

**If you see this:** ✅ **SUCCESS! Token is saved!**

---

## ❌ Possible Error Patterns:

### Error 1: Authentication Failed (401)

```javascript
[NOTIFICATIONS] ❌ Failed to save device token: Error
[NOTIFICATIONS] Error details: {
  status: 401,
  statusText: "Unauthorized"
}
[NOTIFICATIONS] ⚠️ Not authenticated - please log in again
```

**Fix:**
- You're not logged in or token expired
- **Solution:** Logout and login again
- Make sure you see your name on Dashboard

---

### Error 2: Bad Request (400)

```javascript
[NOTIFICATIONS] ❌ Failed to save device token: Error
[NOTIFICATIONS] Error details: {
  status: 400,
  response: { message: "Invalid user" }
}
```

**Fix:**
- JWT token doesn't have userId claim
- **Solution:** Check backend JWT token generation includes userId

---

### Error 3: Network Error

```javascript
[NOTIFICATIONS] ❌ Failed to save device token: AxiosError
[NOTIFICATIONS] Error details: {
  message: "Network Error"
}
```

**Fix:**
- Backend not running or wrong URL
- **Solution:** 
  - Check backend is running at https://localhost:7288
  - Check `.env` has `VITE_API_URL=https://localhost:7288/api`

---

### Error 4: CORS Error

```javascript
Access to XMLHttpRequest at 'https://localhost:7288/api/members/device-token' 
from origin 'http://localhost:3000' has been blocked by CORS policy
```

**Fix:**
- CORS not configured for http://localhost:3000
- **Solution:** Check backend CORS settings allow localhost:3000

---

### Error 5: No Permission

```javascript
[NOTIFICATIONS] ❌ Permission denied or failed
```

**Fix:**
- You clicked "Block" instead of "Allow"
- **Solution:** 
  - Click lock icon in address bar
  - Change Notifications from "Block" to "Ask"
  - Refresh and try again

---

## 🛠️ Backend Logging

I also added logging on the backend. When you click "Enable", check **Visual Studio Output Window** for:

```
POST /api/members/device-token
Authenticated user: 3
Updating device token for user 3
Device token updated successfully
Response: 200 OK
```

**If you see errors:**
```
POST /api/members/device-token
Error: User not found
Response: 404 Not Found
```
OR
```
POST /api/members/device-token
Error: Invalid user (no userId claim)
Response: 400 Bad Request
```

---

## 🔍 Manual Database Check

After clicking "Enable" and seeing success logs, verify in database:

```sql
SELECT user_id, name, device_token 
FROM users 
WHERE user_id = 3;  -- Your member's ID
```

**Expected:**
- `device_token` column should have a long string (not NULL)

**If still NULL:**
- Backend endpoint isn't actually saving
- Check for database errors in backend logs

---

## 🚀 Quick Diagnostic Checklist

Before clicking "Enable", verify:

- [ ] Backend is running (https://localhost:7288)
- [ ] Frontend is running (http://localhost:3000)
- [ ] Logged in as **member** (see name on Dashboard)
- [ ] Browser DevTools Console tab is open
- [ ] Yellow notification banner is visible

After clicking "Enable":

- [ ] Browser showed permission dialog
- [ ] Clicked "Allow" (not Block)
- [ ] Console shows success logs (green ✅)
- [ ] No error logs (red ❌)
- [ ] Backend logs show POST /api/members/device-token 200
- [ ] Database shows device_token is not NULL

---

## 📞 Report Back

After trying this, please share:

1. **What logs appeared in browser console?** (Copy/paste them)
2. **Did you see any errors?** (Share the error details)
3. **What did backend logs show?**
4. **Is device_token still NULL in database?**

This will tell us exactly what's failing!

---

## 💡 Common Issues & Quick Fixes

### "I don't see any [NOTIFICATIONS] logs"

**Problem:** Old code is still running  
**Fix:** 
- Hard refresh: Ctrl+Shift+R
- Or clear browser cache
- Or use Incognito mode

### "Backend shows 401 Unauthorized"

**Problem:** Auth token not being sent  
**Fix:**
- Check localStorage has 'authToken' key
- Run in console: `console.log(localStorage.getItem('authToken'))`
- Should show a JWT token, not null

### "Everything looks successful but database still NULL"

**Problem:** Backend endpoint not actually saving  
**Fix:**
- Check backend MemberService.UpdateDeviceTokenAsync implementation
- Add logging in that method
- Verify database transaction commits

---

## 🎯 Next Steps

1. **Clear browser data**
2. **Login as member**
3. **Click "Enable" with console open**
4. **Share the console output with me**

This enhanced logging will show us **exactly** where the failure is happening! 🔍
