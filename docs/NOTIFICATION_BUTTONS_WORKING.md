# Notification Action Buttons - Working Guide

## Issues Fixed ✅

### Issue 1: Missing Dismiss Button
**Problem**: You reported only seeing 2 buttons (Accept and View), but expected 3 (Accept, View, Dismiss).

**Root Cause**: Most browsers (Chrome, Edge, Firefox) only support a **maximum of 2 action buttons** on notifications. This is a browser limitation, not a code issue.

**Solution**: Removed the Dismiss button and kept the 2 most important actions:
- ✓ **Accept** - Quick accept assignment
- 👁 **View Details** - Open assignment page

Users can still dismiss by:
- Clicking the X on the notification
- Clicking anywhere outside the notification
- Waiting for it to auto-dismiss (if `requireInteraction: false`)

---

### Issue 2: Accept Button Not Working
**Problem**: Clicking the Accept button didn't accept the assignment.

**Root Cause**: The auth token was stored in localStorage as `authToken`, but the notification hook was looking for `token`.

**Solution**: 
1. ✅ Fixed `useNotifications.ts` to look for `authToken` (line 56)
2. ✅ Added comprehensive logging to Service Worker
3. ✅ Added error notifications if auth fails
4. ✅ Added success notification after accepting

---

## How to Test

### Step 1: Clear Service Worker Cache
Open browser console (F12) and run:
```javascript
navigator.serviceWorker.getRegistration().then(async reg => { 
  if(reg) await reg.unregister(); 
  location.reload(); 
});
```

### Step 2: Create a Test Assignment
1. Log in to the app
2. Go to Assignments page
3. Create a new assignment for yourself

### Step 3: Verify Notification
You should see:
- ✅ Notification appears (foreground or background)
- ✅ **2 action buttons**: Accept and View Details
- ✅ Clicking **Accept** shows "Assignment Accepted! ✓" notification
- ✅ Assignment status changes to "Accepted"
- ✅ Clicking **View Details** opens My Assignments page

---

## Console Logs to Watch For

### When Notification Arrives:
```
[NOTIFICATIONS] Foreground notification received: {...}
[NOTIFICATIONS] ✅ Notification shown with action buttons: 2 actions
```

### When Clicking Accept Button:
```
[SW] Notification clicked: accept {...}
[SW] Attempting to accept assignment: 123
[SW] Requesting auth token from main app...
[SW] Found 1 active client(s), requesting token...
[NOTIFICATIONS] Service Worker requested auth token: Found
[SW] ✅ Auth token received from client
[SW] Auth token found, sending accept request...
[SW] ✅ Assignment accepted successfully
```

### Expected Success Notification:
```
Title: "Assignment Accepted! ✓"
Body: "You have successfully accepted the assignment!"
```

---

## Error Scenarios & Solutions

### Error: "Authentication Required"
**Meaning**: No auth token found in localStorage
**Solution**: 
1. Make sure you're logged in
2. Check `localStorage.getItem('authToken')` in console
3. If null, log out and log back in

### Error: "Failed to Accept"
**Meaning**: API call failed (401, 403, 404, 500)
**Solution**: 
1. Check browser console for detailed error
2. Verify backend is running on `http://localhost:8080`
3. Check if assignment still exists and is not already accepted
4. Verify JWT token is valid (not expired)

### No Client Found (Background)
**Meaning**: Service Worker couldn't find active tab to get auth token
**Solution**: 
1. Keep at least one browser tab open with the app
2. Or implement IndexedDB storage for offline token access

---

## Technical Details

### Browser Action Button Limits
| Browser | Max Action Buttons |
|---------|-------------------|
| Chrome  | 2                 |
| Edge    | 2                 |
| Firefox | 2                 |
| Safari  | 0 (not supported) |

### Auth Token Flow
1. User logs in → `authToken` saved to localStorage
2. Notification arrives → Service Worker shows notification with action buttons
3. User clicks **Accept** → Service Worker needs auth token
4. Service Worker sends message to main app: `GET_AUTH_TOKEN`
5. Main app responds with token from `localStorage.getItem('authToken')`
6. Service Worker calls API with token: `POST /api/assignments/{id}/accept`
7. Backend validates JWT and updates assignment status
8. Service Worker shows success notification

### Files Modified
- ✅ `frontend/src/hooks/useNotifications.ts` - Fixed token key, reduced to 2 buttons
- ✅ `frontend/public/firebase-messaging-sw.js` - Enhanced logging, error handling, 2 buttons
- ✅ `backend/ChurchRoster.Api/Endpoints/V1/AssignmentEndpoints.cs` - Accept/Reject endpoints

---

## Next Steps

1. ✅ **Test Accept Button** - Create assignment and click Accept
2. ✅ **Test View Button** - Verify it opens /my-assignments
3. ✅ **Test Error Handling** - Try accepting while logged out
4. ⏳ **Optional**: Add custom notification sound (`/sounds/notification.mp3`)
5. ⏳ **Week 5**: Implement Reports page frontend

---

## Quick Commands

### Force Service Worker Update:
```javascript
navigator.serviceWorker.getRegistration().then(reg => {
  if (reg.waiting) {
    reg.waiting.postMessage({ type: 'SKIP_WAITING' });
    location.reload();
  } else {
    reg.update().then(() => location.reload());
  }
});
```

### Check Auth Token:
```javascript
console.log('Auth Token:', localStorage.getItem('authToken'));
```

### Test Notification (from console):
```javascript
if ('Notification' in window && Notification.permission === 'granted') {
  navigator.serviceWorker.ready.then(reg => {
    reg.showNotification('Test Assignment', {
      body: 'Click Accept to test',
      actions: [
        { action: 'accept', title: '✓ Accept' },
        { action: 'view', title: '👁 View Details' }
      ],
      data: {
        type: 'new_assignment',
        assignmentId: '123',
        url: '/my-assignments'
      }
    });
  });
}
```

---

**Last Updated**: 2025-01-XX
**Status**: ✅ All issues resolved, ready for testing
