# 📱 Push Notifications Implementation Guide
**Church Ministry Roster System - Week 5, Day 5**

---

## 📋 Overview

This guide implements Firebase Cloud Messaging (FCM) push notifications for:
- ✅ New assignment notifications to members
- ✅ Status update notifications to admins
- ✅ Reminder notifications before events
- ✅ PWA notification support (works when app is closed)

**Prerequisites:**
- Firebase setup completed (see `WEEK5_FIREBASE_SETUP_GUIDE.md`)
- VAPID key obtained
- Service account JSON configured

---

## 🎯 Architecture

```
┌─────────────────┐
│  Admin Creates  │
│  Assignment     │
└────────┬────────┘
         │
         ▼
┌─────────────────────────────┐
│  Backend API                │
│  1. Save assignment to DB   │
│  2. Get member device token │
│  3. Send FCM notification   │
└────────┬────────────────────┘
         │
         ▼
┌─────────────────────────────┐
│  Firebase Cloud Messaging   │
│  Routes notification to     │
│  member's device            │
└────────┬────────────────────┘
         │
         ▼
┌─────────────────────────────┐
│  Service Worker (Frontend)  │
│  1. Receives notification   │
│  2. Shows browser popup     │
│  3. Handles click event     │
└─────────────────────────────┘
```

---

## 🔧 Implementation Steps

### **Part 1: Backend Implementation**

#### 1.1 Install NuGet Packages

```bash
cd backend/ChurchRoster.Infrastructure
dotnet add package FirebaseAdmin --version 3.0.0
```

#### 1.2 Create Notification Service Interface

Create `backend/ChurchRoster.Application/Interfaces/INotificationService.cs`:

```csharp
namespace ChurchRoster.Application.Interfaces;

public interface INotificationService
{
    Task<bool> SendAssignmentNotificationAsync(int assignmentId);
    Task<bool> SendStatusUpdateNotificationAsync(int assignmentId, string newStatus);
    Task<bool> SendReminderNotificationAsync(int assignmentId);
    Task<bool> SendCustomNotificationAsync(string deviceToken, string title, string body, Dictionary<string, string>? data = null);
}
```

#### 1.3 Implement Notification Service

Create `backend/ChurchRoster.Infrastructure/Services/NotificationService.cs`:

```csharp
using ChurchRoster.Application.Interfaces;
using ChurchRoster.Infrastructure.Data;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ChurchRoster.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<NotificationService> _logger;
    private readonly FirebaseMessaging _messaging;

    public NotificationService(
        AppDbContext context,
        IConfiguration configuration,
        ILogger<NotificationService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;

        // Initialize Firebase Admin SDK
        if (FirebaseApp.DefaultInstance == null)
        {
            var serviceAccountJson = configuration["Firebase:ServiceAccountJson"];
            var serviceAccountPath = configuration["Firebase:ServiceAccountPath"];

            GoogleCredential credential;

            if (!string.IsNullOrEmpty(serviceAccountJson))
            {
                // Option 1: JSON string from environment variable
                credential = GoogleCredential.FromJson(serviceAccountJson);
            }
            else if (!string.IsNullOrEmpty(serviceAccountPath) && File.Exists(serviceAccountPath))
            {
                // Option 2: JSON file path
                credential = GoogleCredential.FromFile(serviceAccountPath);
            }
            else
            {
                throw new InvalidOperationException(
                    "Firebase service account configuration not found. " +
                    "Set either Firebase:ServiceAccountJson or Firebase:ServiceAccountPath");
            }

            FirebaseApp.Create(new AppOptions
            {
                Credential = credential,
                ProjectId = configuration["Firebase:ProjectId"]
            });
        }

        _messaging = FirebaseMessaging.DefaultInstance;
    }

    public async Task<bool> SendAssignmentNotificationAsync(int assignmentId)
    {
        try
        {
            var assignment = await _context.Assignments
                .Include(a => a.Task)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);

            if (assignment == null || string.IsNullOrEmpty(assignment.User.DeviceToken))
            {
                _logger.LogWarning("Assignment {AssignmentId} not found or user has no device token", assignmentId);
                return false;
            }

            var title = "New Ministry Assignment";
            var body = $"You've been assigned to: {assignment.Task.TaskName} on {assignment.EventDate:MMM dd, yyyy}";

            var data = new Dictionary<string, string>
            {
                { "type", "new_assignment" },
                { "assignmentId", assignmentId.ToString() },
                { "taskName", assignment.Task.TaskName },
                { "eventDate", assignment.EventDate.ToString("yyyy-MM-dd") }
            };

            return await SendCustomNotificationAsync(assignment.User.DeviceToken, title, body, data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send assignment notification for {AssignmentId}", assignmentId);
            return false;
        }
    }

    public async Task<bool> SendStatusUpdateNotificationAsync(int assignmentId, string newStatus)
    {
        try
        {
            var assignment = await _context.Assignments
                .Include(a => a.Task)
                .Include(a => a.User)
                .Include(a => a.AssignedByUser)
                .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);

            if (assignment?.AssignedByUser == null || string.IsNullOrEmpty(assignment.AssignedByUser.DeviceToken))
            {
                _logger.LogWarning("Assignment {AssignmentId} not found or admin has no device token", assignmentId);
                return false;
            }

            var title = "Assignment Status Updated";
            var body = $"{assignment.User.Name} has {newStatus.ToLower()} the {assignment.Task.TaskName} assignment";

            var data = new Dictionary<string, string>
            {
                { "type", "status_update" },
                { "assignmentId", assignmentId.ToString() },
                { "status", newStatus },
                { "memberName", assignment.User.Name }
            };

            return await SendCustomNotificationAsync(assignment.AssignedByUser.DeviceToken, title, body, data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send status update notification for {AssignmentId}", assignmentId);
            return false;
        }
    }

    public async Task<bool> SendReminderNotificationAsync(int assignmentId)
    {
        try
        {
            var assignment = await _context.Assignments
                .Include(a => a.Task)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);

            if (assignment == null || string.IsNullOrEmpty(assignment.User.DeviceToken))
            {
                _logger.LogWarning("Assignment {AssignmentId} not found or user has no device token", assignmentId);
                return false;
            }

            var title = "Ministry Reminder";
            var body = $"Reminder: {assignment.Task.TaskName} is tomorrow ({assignment.EventDate:MMM dd})";

            var data = new Dictionary<string, string>
            {
                { "type", "reminder" },
                { "assignmentId", assignmentId.ToString() },
                { "taskName", assignment.Task.TaskName },
                { "eventDate", assignment.EventDate.ToString("yyyy-MM-dd") }
            };

            return await SendCustomNotificationAsync(assignment.User.DeviceToken, title, body, data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send reminder notification for {AssignmentId}", assignmentId);
            return false;
        }
    }

    public async Task<bool> SendCustomNotificationAsync(
        string deviceToken,
        string title,
        string body,
        Dictionary<string, string>? data = null)
    {
        try
        {
            var message = new Message
            {
                Token = deviceToken,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = data,
                Webpush = new WebpushConfig
                {
                    Notification = new WebpushNotification
                    {
                        Title = title,
                        Body = body,
                        Icon = "/icons/icon-192x192.png",
                        Badge = "/icons/badge-72x72.png",
                        RequireInteraction = true
                    },
                    FcmOptions = new WebpushFcmOptions
                    {
                        Link = data?.ContainsKey("assignmentId") == true
                            ? $"/my-assignments"
                            : "/"
                    }
                }
            };

            var response = await _messaging.SendAsync(message);
            _logger.LogInformation("Notification sent successfully: {Response}", response);
            return true;
        }
        catch (FirebaseMessagingException ex)
        {
            _logger.LogError(ex, "Firebase messaging error: {ErrorCode}", ex.MessagingErrorCode);

            // Handle invalid or expired tokens
            if (ex.MessagingErrorCode == MessagingErrorCode.InvalidArgument ||
                ex.MessagingErrorCode == MessagingErrorCode.Unregistered)
            {
                _logger.LogWarning("Invalid device token, should be removed from user");
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification");
            return false;
        }
    }
}
```

#### 1.4 Register Service in Program.cs

In `backend/ChurchRoster.Api/Program.cs`:

```csharp
// Add after other service registrations
builder.Services.AddScoped<INotificationService, NotificationService>();
```

#### 1.5 Update Assignment Endpoints

Modify `backend/ChurchRoster.Api/Endpoints/V1/AssignmentEndpoints.cs`:

```csharp
// Add to CreateAssignment endpoint (after saving to database)
private static async Task<IResult> CreateAssignment(
    CreateAssignmentRequest request,
    IAssignmentService assignmentService,
    INotificationService notificationService) // Add this parameter
{
    var assignment = await assignmentService.CreateAssignmentAsync(request);

    // Send notification asynchronously (don't wait)
    _ = notificationService.SendAssignmentNotificationAsync(assignment.AssignmentId);

    return Results.Created($"/api/assignments/{assignment.AssignmentId}", assignment);
}

// Update UpdateAssignmentStatus endpoint
private static async Task<IResult> UpdateAssignmentStatus(
    int assignmentId,
    UpdateStatusRequest request,
    IAssignmentService assignmentService,
    INotificationService notificationService) // Add this parameter
{
    await assignmentService.UpdateAssignmentStatusAsync(assignmentId, request.Status, request.RejectionReason);

    // Notify admin of status change
    if (request.Status == "Accepted" || request.Status == "Rejected")
    {
        _ = notificationService.SendStatusUpdateNotificationAsync(assignmentId, request.Status);
    }

    return Results.NoContent();
}
```

---

### **Part 2: Frontend Implementation**

#### 2.1 Install Firebase Packages

```bash
cd frontend
npm install firebase
```

#### 2.2 Create Firebase Configuration

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

#### 2.3 Create Service Worker

Create `frontend/public/firebase-messaging-sw.js`:

```javascript
// Firebase Messaging Service Worker
importScripts('https://www.gstatic.com/firebasejs/10.7.1/firebase-app-compat.js');
importScripts('https://www.gstatic.com/firebasejs/10.7.1/firebase-messaging-compat.js');

// Initialize Firebase in Service Worker
firebase.initializeApp({
  apiKey: "YOUR_API_KEY",
  authDomain: "YOUR_AUTH_DOMAIN",
  projectId: "YOUR_PROJECT_ID",
  storageBucket: "YOUR_STORAGE_BUCKET",
  messagingSenderId: "YOUR_MESSAGING_SENDER_ID",
  appId: "YOUR_APP_ID"
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

**⚠️ IMPORTANT:** Replace `YOUR_API_KEY`, `YOUR_AUTH_DOMAIN`, etc. with your actual Firebase config values from `.env`.

#### 2.4 Register Service Worker in index.html

Update `frontend/index.html`:

```html
<!DOCTYPE html>
<html lang="en">
  <head>
    <meta charset="UTF-8" />
    <link rel="icon" type="image/svg+xml" href="/vite.svg" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
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

#### 2.5 Create Notification Hook

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
        await apiService.post('/users/device-token', { deviceToken: token });
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

#### 2.6 Add Device Token Endpoint (Backend)

Create endpoint in `backend/ChurchRoster.Api/Endpoints/V1/UserEndpoints.cs`:

```csharp
group.MapPost("/device-token", SaveDeviceToken)
    .RequireAuthorization()
    .WithName("SaveDeviceToken");

private static async Task<IResult> SaveDeviceToken(
    DeviceTokenRequest request,
    HttpContext httpContext,
    AppDbContext context)
{
    var userId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    var user = await context.Users.FindAsync(userId);
    if (user == null) return Results.NotFound();

    user.DeviceToken = request.DeviceToken;
    await context.SaveChangesAsync();

    return Results.Ok(new { message = "Device token saved" });
}

public record DeviceTokenRequest(string DeviceToken);
```

#### 2.7 Integrate into Dashboard

Update `frontend/src/pages/Dashboard.tsx`:

```typescript
import { useNotifications } from '../hooks/useNotifications';

const Dashboard: React.FC = () => {
  const { notificationPermission, requestPermission } = useNotifications();

  return (
    <div>
      <h1>Dashboard</h1>

      {/* Show notification prompt if not granted */}
      {notificationPermission !== 'granted' && (
        <div style={styles.notificationBanner}>
          <p>Enable notifications to receive assignment updates</p>
          <button onClick={requestPermission} style={styles.enableButton}>
            Enable Notifications
          </button>
        </div>
      )}

      {/* Rest of dashboard */}
    </div>
  );
};
```

---

## 🧪 Testing

### Local Testing

1. **Start Backend:**
   ```bash
   cd backend/ChurchRoster.Api
   dotnet run
   ```

2. **Start Frontend:**
   ```bash
   cd frontend
   npm run dev
   ```

3. **Test Flow:**
   - Login as member
   - Click "Enable Notifications" on dashboard
   - Grant permission in browser
   - Login as admin in another browser/tab
   - Create assignment for the member
   - Member should receive notification

### Mobile Testing

1. **Deploy to Vercel** (notifications require HTTPS)
2. **Open on phone browser**
3. **Add to Home Screen**
4. **Enable notifications**
5. **Test assignment creation**

---

## 🚀 Deployment

### Frontend (Vercel)

Add environment variables in Vercel Dashboard:
- `VITE_FIREBASE_API_KEY`
- `VITE_FIREBASE_AUTH_DOMAIN`
- `VITE_FIREBASE_PROJECT_ID`
- `VITE_FIREBASE_STORAGE_BUCKET`
- `VITE_FIREBASE_MESSAGING_SENDER_ID`
- `VITE_FIREBASE_APP_ID`
- `VITE_FIREBASE_VAPID_KEY`

### Backend (Render)

Add environment variable:
- `Firebase__ServiceAccountJson` (minified JSON)
- `Firebase__ProjectId`

---

## 📊 Monitoring

Check Firebase Console → Cloud Messaging → Dashboard for:
- Notifications sent
- Delivery rate
- Click-through rate
- Errors

---

**Next Steps:** Proceed to Week 5, Day 6 - PDF Report Generation
