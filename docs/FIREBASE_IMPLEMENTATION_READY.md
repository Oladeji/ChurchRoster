# 🔔 Firebase Push Notifications - Frontend Implementation
**Quick Implementation Guide**

---

## ✅ Prerequisites Complete

- ✅ Firebase package installed (`firebase@11.2.0`)
- ✅ vite-plugin-pwa removed (incompatible with Vite 8)
- ✅ Build successful

---

## 📋 Implementation Steps

### **Step 1: Create Firebase Configuration**

Create `frontend/src/config/firebase.config.ts`:

```typescript
import { initializeApp } from 'firebase/app';
import { getMessaging, getToken, onMessage, Messaging } from 'firebase/messaging';

const firebaseConfig = {
  apiKey: import.meta.env.VITE_FIREBASE_API_KEY,
  authDomain: import.meta.env.VITE_FIREBASE_AUTH_DOMAIN,
  projectId: import.meta.env.VITE_FIREBASE_PROJECT_ID,
  storageBucket: import.meta.env.VITE_FIREBASE_STORAGE_BUCKET,
  messagingSenderId: import.meta.env.VITE_FIREBASE_MESSAGING_SENDER_ID,
  appId: import.meta.env.VITE_FIREBASE_APP_ID
};

// Initialize Firebase
const app = initializeApp(firebaseConfig);

// Initialize Firebase Cloud Messaging
let messaging: Messaging | null = null;

try {
  messaging = getMessaging(app);
} catch (error) {
  console.warn('Firebase Messaging not supported in this browser:', error);
}

export { app, messaging };

export const requestNotificationPermission = async (): Promise<string | null> => {
  if (!messaging) {
    console.warn('Firebase Messaging not initialized');
    return null;
  }

  try {
    const permission = await Notification.requestPermission();

    if (permission === 'granted') {
      const token = await getToken(messaging, {
        vapidKey: import.meta.env.VITE_FIREBASE_VAPID_KEY
      });

      console.log('FCM Token:', token);
      return token;
    } else {
      console.log('Notification permission denied');
      return null;
    }
  } catch (error) {
    console.error('Error getting notification permission:', error);
    return null;
  }
};

export const onMessageListener = (): Promise<any> => {
  if (!messaging) {
    return Promise.reject('Firebase Messaging not initialized');
  }

  return new Promise((resolve) => {
    onMessage(messaging!, (payload) => {
      console.log('Message received:', payload);
      resolve(payload);
    });
  });
};
```

---

### **Step 2: Create Notification Hook**

Create `frontend/src/hooks/useNotifications.ts`:

```typescript
import { useEffect, useState } from 'react';
import { requestNotificationPermission, onMessageListener } from '../config/firebase.config';
import apiService from '../services/api.service';

export const useNotifications = () => {
  const [notificationPermission, setNotificationPermission] = useState<NotificationPermission>('default');
  const [deviceToken, setDeviceToken] = useState<string | null>(null);

  useEffect(() => {
    // Check current permission status
    if ('Notification' in window) {
      setNotificationPermission(Notification.permission);
    }

    // Listen for foreground messages
    onMessageListener()
      .then((payload: any) => {
        console.log('Foreground notification received:', payload);

        // Show custom in-app notification
        if (payload.notification) {
          new Notification(payload.notification.title, {
            body: payload.notification.body,
            icon: '/icons/icon-192x192.png'
          });
        }
      })
      .catch((error) => console.error('Error listening for messages:', error));
  }, []);

  const requestPermission = async () => {
    const token = await requestNotificationPermission();

    if (token) {
      setDeviceToken(token);
      setNotificationPermission('granted');

      // Save token to backend
      try {
        await apiService.post('/members/device-token', { deviceToken: token });
        console.log('Device token saved to backend');
      } catch (error) {
        console.error('Failed to save device token:', error);
      }
    } else {
      setNotificationPermission('denied');
    }
  };

  return {
    notificationPermission,
    deviceToken,
    requestPermission
  };
};
```

---

### **Step 3: Create Firebase Messaging Service Worker**

Create `frontend/public/firebase-messaging-sw.js`:

```javascript
// Firebase Messaging Service Worker
importScripts('https://www.gstatic.com/firebasejs/10.7.1/firebase-app-compat.js');
importScripts('https://www.gstatic.com/firebasejs/10.7.1/firebase-messaging-compat.js');

// ⚠️ REPLACE WITH YOUR FIREBASE CONFIG VALUES!
firebase.initializeApp({
  apiKey: "YOUR_API_KEY_HERE",
  authDomain: "YOUR_AUTH_DOMAIN_HERE",
  projectId: "YOUR_PROJECT_ID_HERE",
  storageBucket: "YOUR_STORAGE_BUCKET_HERE",
  messagingSenderId: "YOUR_MESSAGING_SENDER_ID_HERE",
  appId: "YOUR_APP_ID_HERE"
});

const messaging = firebase.messaging();

// Handle background messages
messaging.onBackgroundMessage((payload) => {
  console.log('Background message received:', payload);

  const notificationTitle = payload.notification?.title || 'Church Roster';
  const notificationOptions = {
    body: payload.notification?.body || 'You have a new notification',
    icon: '/icons/icon-192x192.png',
    badge: '/icons/badge-72x72.png',
    data: payload.data,
    requireInteraction: true,
    actions: [
      { action: 'view', title: 'View' },
      { action: 'dismiss', title: 'Dismiss' }
    ]
  };

  self.registration.showNotification(notificationTitle, notificationOptions);
});

// Handle notification click
self.addEventListener('notificationclick', (event) => {
  console.log('Notification clicked:', event);
  event.notification.close();

  if (event.action === 'view' || !event.action) {
    // Open the app
    event.waitUntil(
      clients.openWindow(event.notification.data?.link || '/my-assignments')
    );
  }
});
```

**⚠️ IMPORTANT**: Replace `YOUR_API_KEY_HERE`, etc. with your actual Firebase config values from `.env`!

---

### **Step 4: Update Dashboard to Request Permission**

Update `frontend/src/pages/Dashboard.tsx`:

```typescript
import { useNotifications } from '../hooks/useNotifications';

const Dashboard: React.FC = () => {
  const { notificationPermission, requestPermission } = useNotifications();

  // ... existing code ...

  return (
    <div style={containerStyle}>
      {/* Show notification prompt if not granted */}
      {notificationPermission !== 'granted' && (
        <div style={{
          background: '#fef3c7',
          border: '1px solid #f59e0b',
          borderRadius: '8px',
          padding: '16px',
          marginBottom: '24px',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between'
        }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
            <span style={{ fontSize: '24px' }}>🔔</span>
            <div>
              <p style={{ margin: 0, fontWeight: '600', color: '#92400e' }}>
                Enable Notifications
              </p>
              <p style={{ margin: 0, fontSize: '14px', color: '#92400e' }}>
                Get notified when you're assigned to tasks
              </p>
            </div>
          </div>
          <button
            onClick={requestPermission}
            style={{
              background: '#f59e0b',
              color: 'white',
              border: 'none',
              borderRadius: '6px',
              padding: '8px 16px',
              cursor: 'pointer',
              fontWeight: '600'
            }}
          >
            Enable
          </button>
        </div>
      )}

      {/* Rest of your dashboard content */}
    </div>
  );
};
```

---

### **Step 5: Update index.html to Register Service Worker**

Update `frontend/index.html` - add this script before the closing `</body>` tag:

```html
<!DOCTYPE html>
<html lang="en">
  <head>
    <meta charset="UTF-8" />
    <link rel="icon" type="image/svg+xml" href="/vite.svg" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <link rel="manifest" href="/manifest.json" />
    <title>Church Roster</title>
  </head>
  <body>
    <div id="root"></div>
    <script type="module" src="/src/main.tsx"></script>

    <!-- Register Firebase Messaging Service Worker -->
    <script>
      if ('serviceWorker' in navigator) {
        navigator.serviceWorker.register('/firebase-messaging-sw.js')
          .then((registration) => {
            console.log('Service Worker registered:', registration);
          })
          .catch((error) => {
            console.error('Service Worker registration failed:', error);
          });
      }
    </script>
  </body>
</html>
```

---

### **Step 6: Create .env File**

Create `frontend/.env`:

```env
# Replace with YOUR actual Firebase values!
VITE_FIREBASE_API_KEY=AIzaSyXXXXXXXXXXXXXXXXXX
VITE_FIREBASE_AUTH_DOMAIN=church-roster-system.firebaseapp.com
VITE_FIREBASE_PROJECT_ID=church-roster-system
VITE_FIREBASE_STORAGE_BUCKET=church-roster-system.appspot.com
VITE_FIREBASE_MESSAGING_SENDER_ID=123456789012
VITE_FIREBASE_APP_ID=1:123456789012:web:abcdef1234567890
VITE_FIREBASE_VAPID_KEY=BNfXXXXXXXXXXXXXXXXXXXXXXX
```

**⚠️ IMPORTANT**: Get these values from your Firebase Console!

---

## 🧪 Testing

### 1. Start Backend
```bash
cd backend/ChurchRoster.Api
dotnet run
```

### 2. Start Frontend
```bash
cd frontend
npm run dev
```

### 3. Test Flow
1. Open http://localhost:3000
2. Login as a member
3. Click "Enable Notifications" on dashboard
4. Grant permission in browser
5. Open another browser/incognito as admin
6. Create assignment for the member
7. Member should receive notification! 🎉

---

## 🚀 What's Working

✅ **Firebase installed** (`firebase@11.2.0`)  
✅ **Build successful**  
✅ **No dependency conflicts**  
✅ **Ready to implement**

---

## 📝 Next Steps

1. ✅ **Follow this guide** to create the 6 files above
2. ✅ **Get Firebase config** from Firebase Console (see `WEEK5_FIREBASE_SETUP_GUIDE.md`)
3. ✅ **Replace placeholders** in service worker and .env
4. ✅ **Test notifications**

---

## 🆘 Troubleshooting

| Issue | Solution |
|-------|----------|
| Service worker not registering | Check browser console, ensure file is in `public/` folder |
| Permission not requesting | Check if HTTPS (required for notifications) or localhost |
| Token not saving | Verify `/api/members/device-token` endpoint is accessible |
| No notifications received | Check device token saved in database, verify Firebase config |

---

**You're ready to implement push notifications!** 🎉
