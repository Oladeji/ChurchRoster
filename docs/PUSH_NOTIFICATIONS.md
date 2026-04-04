# Push Notifications - Complete Implementation Guide

## ✅ Features Implemented

### 1. **Foreground Notifications** (App is Open)
- Notifications appear when the user is actively using the app
- Custom notification sound plays
- Click handler to navigate to relevant page
- Visual notification with icon and badge

### 2. **Background Notifications** (App is Closed/Minimized)
- Handled by Service Worker (`firebase-messaging-sw.js`)
- Notifications appear even when browser is minimized
- Persist until user interacts with them (`requireInteraction: true`)

### 3. **Notification Click Handlers**
- **Click notification body**: Opens app to `/my-assignments`
- **Click "Accept" button**: Accepts assignment via API and opens app
- **Click "View Details" button**: Opens app to assignment details
- **Click "Dismiss" button**: Closes notification

### 4. **Action Buttons**
For **new assignment** notifications:
- ✓ **Accept** - Directly accepts the assignment
- 👁 **View Details** - Opens the assignments page
- ✕ **Dismiss** - Closes the notification

For **other** notifications:
- **View** - Opens the relevant page
- **Dismiss** - Closes the notification

### 5. **Custom Notification Sound**
- Located in: `frontend/public/sounds/notification.mp3`
- Fallback to system beep if custom sound fails
- Volume set to 50% to avoid startling users
- Only plays for foreground notifications

### 6. **Vibration (Mobile Devices)**
- Pattern: `[200ms, 100ms, 200ms]` (vibrate, pause, vibrate)
- Works on mobile browsers that support the Vibration API

---

## 📂 File Structure

### Backend
```
backend/ChurchRoster.Application/Services/
├── NotificationService.cs              # Firebase message sending
│   ├── SendAssignmentNotificationAsync()
│   ├── SendStatusUpdateNotificationAsync()
│   └── SendCustomNotificationAsync()

backend/ChurchRoster.Api/Endpoints/V1/
└── AssignmentEndpoints.cs              # API endpoints
    ├── POST /api/assignments/{id}/accept  # Quick accept from notification
    └── POST /api/assignments/{id}/reject  # Quick reject from notification
```

### Frontend
```
frontend/public/
├── firebase-messaging-sw.js           # Service Worker (background notifications)
└── sounds/
    └── notification.mp3               # Custom notification sound

frontend/src/
├── config/firebase.config.ts          # Firebase initialization
│   ├── requestNotificationPermission()
│   └── onMessageListener()            # Continuous message listener
└── hooks/useNotifications.ts          # React hook
    ├── Foreground message handling
    ├── Notification sound playback
    └── Click handlers
```

---

## 🔔 Notification Flow

### **1. Assignment Created**
```mermaid
Backend (Assignment Created)
    → Firebase Admin SDK sends FCM message
    → Firebase Cloud Messaging
    → User's browser receives message

If app is OPEN (foreground):
    → onMessageListener() callback fires
    → Custom sound plays
    → Browser Notification API shows notification
    → User clicks → Navigate to /my-assignments

If app is CLOSED (background):
    → Service Worker onBackgroundMessage() fires
    → Service Worker shows notification with action buttons
    → User clicks "Accept" → API call to /api/assignments/{id}/accept
    → App opens to /my-assignments
```

### **2. Notification Data Structure**
```javascript
{
  notification: {
    title: "New Ministry Assignment",
    body: "You've been assigned to: Lead Prayer Meeting on Apr 27, 2026",
    icon: "/icons/icon-192x192.png"
  },
  data: {
    type: "new_assignment",
    assignmentId: "26",
    taskName: "Lead Prayer Meeting",
    eventDate: "2026-04-27"
  }
}
```

---

## 🚀 How to Use

### **For Users**
1. **Grant Permission**: Click "Enable Notifications" in the yellow banner
2. **Receive Notifications**: 
   - When assigned to a task
   - When assignment status changes
3. **Quick Actions**:
   - Click notification to view details
   - Click "Accept" to immediately accept assignment
   - Click "Dismiss" to ignore

### **For Developers**

#### **Testing Notifications**
1. Start backend: `dotnet run --project backend/ChurchRoster.Api`
2. Start frontend: `cd frontend && npm run dev`
3. Login as a user
4. Click "Enable Notifications" 
5. Create an assignment for yourself (as admin)
6. Check browser notification appears

#### **Testing Background Notifications**
1. Complete steps above
2. **Minimize** the browser window or switch to another app
3. Create another assignment
4. Notification should appear in system notification center
5. Click "Accept" button → Assignment accepted automatically

#### **Adding Custom Sound**
1. Download a notification sound (2-3 seconds, < 50KB)
2. Save as `frontend/public/sounds/notification.mp3`
3. Test: Create assignment → Sound should play

---

## 🔧 Configuration

### **Backend** (`appsettings.json`)
```json
{
  "App": {
    "FrontendUrl": "http://localhost:3000"
  },
  "Firebase": {
    "ServiceAccountJson": "{ ... }" // Firebase Admin SDK credentials
  }
}
```

### **Frontend** (`.env`)
```env
VITE_FIREBASE_API_KEY=AIzaSy...
VITE_FIREBASE_AUTH_DOMAIN=roster-system-f42cf.firebaseapp.com
VITE_FIREBASE_PROJECT_ID=roster-system-f42cf
VITE_FIREBASE_STORAGE_BUCKET=roster-system-f42cf.firebasestorage.app
VITE_FIREBASE_MESSAGING_SENDER_ID=197640018435
VITE_FIREBASE_APP_ID=1:197640018435:web:113f29d0494b0fe189cc36
VITE_FIREBASE_VAPID_KEY=BNr... // Web Push certificate
```

---

## 📝 API Endpoints

### **Accept Assignment from Notification**
```http
POST /api/assignments/{assignmentId}/accept
Authorization: Bearer {jwt_token}

Response: 200 OK
{
  "assignmentId": 26,
  "status": "Accepted",
  ...
}
```

### **Reject Assignment from Notification**
```http
POST /api/assignments/{assignmentId}/reject
Authorization: Bearer {jwt_token}

Response: 200 OK
{
  "assignmentId": 26,
  "status": "Rejected",
  "rejectionReason": "Declined from notification",
  ...
}
```

---

## 🛠️ Troubleshooting

### **Notifications Not Appearing**
1. Check browser console for errors
2. Verify notification permission: `Notification.permission === "granted"`
3. Check device token saved: Look for `[NOTIFICATIONS] ✅ Device token saved`
4. Check backend logs for Firebase send errors

### **Sound Not Playing**
1. Verify `frontend/public/sounds/notification.mp3` exists
2. Check browser console for audio errors
3. Some browsers block audio without user interaction
4. Volume might be muted in system settings

### **Action Buttons Not Working**
1. Action buttons only work in Service Worker notifications (background)
2. Foreground notifications (app open) don't show buttons in most browsers
3. Check Service Worker is registered: `navigator.serviceWorker.controller`

### **"Accept" Button Not Working**
1. Check auth token is available in Service Worker
2. Verify `/api/assignments/{id}/accept` endpoint is accessible
3. Check CORS settings allow requests from Service Worker
4. Look for errors in Service Worker console: `chrome://serviceworker-internals/`

---

## 🎨 Customization

### **Change Notification Sound**
Replace `frontend/public/sounds/notification.mp3` with your preferred sound file.

### **Adjust Sound Volume**
Edit `useNotifications.ts`:
```typescript
audio.volume = 0.5; // Change to 0.0-1.0
```

### **Change Vibration Pattern**
Edit `firebase-messaging-sw.js`:
```javascript
vibrate: [200, 100, 200] // [vibrate, pause, vibrate] in milliseconds
```

### **Modify Action Buttons**
Edit `firebase-messaging-sw.js`:
```javascript
actions: [
  { action: 'accept', title: '✓ Accept', icon: '/icons/accept-icon.png' },
  { action: 'view', title: '👁 View Details' },
  { action: 'custom', title: '⭐ Custom Action' } // Add custom actions
]
```

---

## 📊 Browser Compatibility

| Feature | Chrome | Firefox | Safari | Edge |
|---------|--------|---------|--------|------|
| Foreground Notifications | ✅ | ✅ | ✅ | ✅ |
| Background Notifications | ✅ | ✅ | ❌* | ✅ |
| Action Buttons | ✅ | ✅ | ❌ | ✅ |
| Custom Sound | ✅ | ✅ | ✅ | ✅ |
| Vibration | ✅ | ✅ | ❌ | ✅ |

*Safari supports background notifications on macOS 13+ and iOS 16.4+

---

## 🔐 Security Notes

1. **Device tokens** are stored securely in the database
2. **JWT authentication** required for accept/reject endpoints
3. **Service Worker** only accepts messages from authorized Firebase project
4. **HTTPS required** in production for Service Workers and notifications

---

## 📈 Future Enhancements

- [ ] Notification preferences (sound on/off, types to receive)
- [ ] In-app notification center/history
- [ ] Batch notifications (digest mode)
- [ ] Rich notifications with images
- [ ] Notification scheduling (send at specific times)
- [ ] User-specific notification sounds
- [ ] Desktop app integration (Electron)

---

## 📞 Support

For issues or questions:
1. Check browser console for errors
2. Review backend logs for Firebase errors  
3. Verify Firebase configuration matches project settings
4. Test with different browsers

---

**Status**: ✅ Fully Implemented (Week 5 - Push Notifications Complete)
