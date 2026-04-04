// Firebase Messaging Service Worker
importScripts('https://www.gstatic.com/firebasejs/10.7.1/firebase-app-compat.js');
importScripts('https://www.gstatic.com/firebasejs/10.7.1/firebase-messaging-compat.js');

// Initialize Firebase with your project configuration
firebase.initializeApp({
  apiKey: "AIzaSyDqsjLP5mH285iUHgFZpdT9PHV1s6zJb9k",
  authDomain: "roster-system-f42cf.firebaseapp.com",
  projectId: "roster-system-f42cf",
  storageBucket: "roster-system-f42cf.firebasestorage.app",
  messagingSenderId: "197640018435",
  appId: "1:197640018435:web:113f29d0494b0fe189cc36"
});

const messaging = firebase.messaging();

// Store API URL - will be set by main app from .env
let API_URL = 'https://localhost:7288/api'; // Fallback default

// Handle background messages (when app is closed or in background)
messaging.onBackgroundMessage((payload) => {
  console.log('[SW] Background message received:', payload);

  const notificationTitle = payload.notification?.title || 'Church Roster';
  const notificationData = payload.data || {};

  // Build notification options with action buttons
  const notificationOptions = {
    body: payload.notification?.body || 'You have a new notification',
    icon: payload.notification?.icon || '/icons/icon-192x192.png',
    badge: '/icons/badge-72x72.png',
    tag: 'church-roster-' + (notificationData.assignmentId || Date.now()),
    data: {
      ...notificationData,
      url: notificationData.assignmentId ? `/my-assignments` : '/',
      timestamp: Date.now()
    },
    requireInteraction: true,
    silent: false,
    vibrate: [200, 100, 200], // Vibration pattern for mobile devices
    actions: []
  };

  // Add action buttons for assignment notifications
  // Note: Most browsers only support 2 action buttons maximum
  if (notificationData.type === 'new_assignment' && notificationData.assignmentId) {
    notificationOptions.actions = [
      { 
        action: 'accept', 
        title: '✓ Accept',
        icon: '/icons/accept-icon.png'
      },
      { 
        action: 'view', 
        title: '👁 View Details'
      }
    ];
  } else {
    notificationOptions.actions = [
      { action: 'view', title: 'View' }
    ];
  }

  return self.registration.showNotification(notificationTitle, notificationOptions);
});

// Handle notification click events
self.addEventListener('notificationclick', (event) => {
  console.log('[SW] ========== NOTIFICATION CLICKED ==========');
  console.log('[SW] Action:', event.action);
  console.log('[SW] Notification Data:', event.notification.data);
  console.log('[SW] Notification Tag:', event.notification.tag);

  event.notification.close();

  const notificationData = event.notification.data || {};
  const assignmentId = notificationData.assignmentId;

  console.log('[SW] Extracted Assignment ID:', assignmentId);
  console.log('[SW] Assignment ID Type:', typeof assignmentId);

  // Handle different actions
  if (event.action === 'accept' && assignmentId) {
    console.log('[SW] ✅ ACCEPT ACTION TRIGGERED for assignment:', assignmentId);
    // Accept assignment via API
    event.waitUntil(
      handleAcceptAssignment(assignmentId)
        .then(() => {
          console.log('[SW] ✅ Accept completed, opening app...');
          // Open the app to show the updated assignment
          return clients.openWindow('/my-assignments');
        })
        .catch(error => {
          console.error('[SW] ❌ Accept failed:', error);
          // Still open the app so user can accept manually
          return clients.openWindow('/my-assignments');
        })
    );
  } else if (event.action === 'accept' && !assignmentId) {
    console.error('[SW] ❌ ACCEPT ACTION but NO ASSIGNMENT ID!');
    console.error('[SW] Full notification data:', JSON.stringify(notificationData));
  } else if (event.action === 'view' || !event.action) {
    // Open the relevant page
    const url = notificationData.url || '/my-assignments';
    event.waitUntil(
      clients.matchAll({ type: 'window', includeUncontrolled: true })
        .then(windowClients => {
          // Check if there's already a window open
          for (let client of windowClients) {
            if (client.url.includes(self.location.origin) && 'focus' in client) {
              // Focus existing window and navigate
              return client.focus().then(() => {
                return client.navigate(url);
              });
            }
          }
          // No window open, open a new one
          return clients.openWindow(url);
        })
    );
  } else if (event.action === 'dismiss') {
    // Just close the notification (already done above)
    console.log('[SW] Notification dismissed');
  }
});

// Handle notification close event
self.addEventListener('notificationclose', (event) => {
  console.log('[SW] Notification closed:', event.notification.tag);

  // Optional: Track dismissals
  const notificationData = event.notification.data || {};
  if (notificationData.assignmentId) {
    // Could send analytics or track that user dismissed the notification
    console.log('[SW] User dismissed assignment notification:', notificationData.assignmentId);
  }
});

// Helper function to accept assignment via API
async function handleAcceptAssignment(assignmentId) {
  try {
    console.log('[SW] Attempting to accept assignment:', assignmentId);

    // Get the auth token from IndexedDB or cookies
    const token = await getAuthToken();

    if (!token) {
      console.error('[SW] ❌ No auth token found, cannot auto-accept assignment');

      // Show error notification
      await self.registration.showNotification('Authentication Required', {
        body: 'Please log in to accept assignments',
        icon: '/icons/icon-192x192.png',
        tag: 'auth-required',
        requireInteraction: false
      });
      return;
    }

    console.log('[SW] Auth token found, sending accept request...');

    // Use the API URL received from the main app (from .env: VITE_API_URL)
    console.log('[SW] Using API URL:', API_URL);

    const response = await fetch(`${API_URL}/assignments/${assignmentId}/accept`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    });

    if (!response.ok) {
      const errorText = await response.text();
      console.error('[SW] ❌ Failed to accept assignment:', response.status, errorText);
      throw new Error(`Failed to accept assignment: ${response.status} ${response.statusText}`);
    }

    console.log('[SW] ✅ Assignment accepted successfully');

    // Show a success notification
    await self.registration.showNotification('Assignment Accepted! ✓', {
      body: 'You have successfully accepted the assignment!',
      icon: '/icons/icon-192x192.png',
      badge: '/icons/badge-72x72.png',
      tag: 'assignment-accepted-' + assignmentId,
      requireInteraction: false,
      silent: true
    });

    return response.json();
  } catch (error) {
    console.error('[SW] ❌ Error accepting assignment:', error);

    // Show error notification
    await self.registration.showNotification('Failed to Accept', {
      body: 'Could not accept assignment. Please try again.',
      icon: '/icons/icon-192x192.png',
      tag: 'assignment-error',
      requireInteraction: false
    });

    throw error;
  }
}

// Helper function to get auth token from storage
async function getAuthToken() {
  // Try to get token from the main app via MessageChannel
  try {
    console.log('[SW] Requesting auth token from main app...');

    const allClients = await clients.matchAll({ includeUncontrolled: true });

    if (allClients.length === 0) {
      console.warn('[SW] ⚠️ No active clients found to get auth token from');
      return null;
    }

    console.log('[SW] Found', allClients.length, 'active client(s), requesting token...');

    const client = allClients[0];
    return new Promise((resolve) => {
      const messageChannel = new MessageChannel();

      // Set timeout in case client doesn't respond
      const timeout = setTimeout(() => {
        console.error('[SW] ⚠️ Timeout waiting for auth token from client');
        resolve(null);
      }, 5000);

      messageChannel.port1.onmessage = (event) => {
        clearTimeout(timeout);
        const token = event.data.token;
        if (token) {
          console.log('[SW] ✅ Auth token received from client');
        } else {
          console.warn('[SW] ⚠️ Client responded but no token found');
        }
        resolve(token);
      };

      client.postMessage({ type: 'GET_AUTH_TOKEN' }, [messageChannel.port2]);
    });
  } catch (error) {
    console.error('[SW] ❌ Error getting auth token:', error);
    return null;
  }
}

// Handle messages from the main app
self.addEventListener('message', (event) => {
  if (event.data && event.data.type === 'SKIP_WAITING') {
    console.log('[SW] Received SKIP_WAITING message, activating new version');
    self.skipWaiting();
  } else if (event.data && event.data.type === 'SET_API_URL') {
    // Receive API URL from main app (from .env file)
    API_URL = event.data.apiUrl;
    console.log('[SW] ✅ API URL updated from main app:', API_URL);
    console.log('[SW] API URL source: .env VITE_API_URL');
  }
});
