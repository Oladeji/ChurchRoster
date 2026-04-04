import { useEffect, useState } from 'react';
import { requestNotificationPermission, onMessageListener } from '../config/firebase.config';
import apiService from '../services/api.service';

interface NotificationPayload {
  notification?: {
    title: string;
    body: string;
  };
}

// Extend ServiceWorkerRegistration to include showNotification with actions
interface ExtendedNotificationOptions extends NotificationOptions {
  actions?: Array<{ action: string; title: string; icon?: string }>;
}

// Notification sound helper
const playNotificationSound = () => {
  try {
    // Create audio element for custom notification sound
    const audio = new Audio('/sounds/notification.mp3');
    audio.volume = 0.5; // 50% volume
    audio.play().catch(err => {
      console.warn('Could not play notification sound:', err);
      // Fallback to system beep if custom sound fails
      try {
        const beep = new Audio('data:audio/wav;base64,UklGRnoGAABXQVZFZm10IBAAAAABAAEAQB8AAEAfAAABAAgAZGF0YQoGAACBhYqFbF1fdJivrJBhNjVgodDbq2EcBj+a2/LDciUFLIHO8tiJNwgZaLvt559NEAxQp+PwtmMcBjiR1/LMeSwFJHfH8N2QQAoUXrTp66hVFApGn+DyvmwhBjOR0fPTgjMGJHLP8N2RQAoUXrTp66hVFAwGRp/g8r5sIQY0kdHy04IzBiRyz/DdkUAKFF606+uoVRQMBkaf4PK+bCEGNJHR8tOCMwYkcs/w3ZFAA');
        beep.play().catch(() => {});
      } catch {
        // Silent fallback
      }
    });
  } catch (error) {
    console.warn('Notification sound error:', error);
  }
};

export const useNotifications = () => {
  const [notificationPermission, setNotificationPermission] = useState<NotificationPermission>('default');
  const [deviceToken, setDeviceToken] = useState<string | null>(null);

  useEffect(() => {
    // Check current permission status on mount
    const checkPermission = () => {
      if ('Notification' in window) {
        setNotificationPermission(Notification.permission);
      }
    };

    checkPermission();

    // Send API URL to Service Worker and store in localStorage
    const sendConfigToServiceWorker = async () => {
      try {
        if (!('serviceWorker' in navigator)) {
          console.warn('[NOTIFICATIONS] Service Worker not supported');
          return;
        }

        const apiUrl = import.meta.env.VITE_API_URL || 'https://localhost:7288/api';

        // Store in localStorage for test pages to access
        localStorage.setItem('apiUrl', apiUrl);
        console.log('[NOTIFICATIONS] API URL stored in localStorage:', apiUrl);

        // Wait for Service Worker to be ready before sending config
        const registration = await navigator.serviceWorker.ready;

        if (registration.active) {
          // Send API URL to Service Worker
          registration.active.postMessage({
            type: 'SET_API_URL',
            apiUrl: apiUrl
          });

          console.log('[NOTIFICATIONS] ✅ API URL configured:', apiUrl);
          console.log('[NOTIFICATIONS] Source: .env VITE_API_URL');
        } else {
          console.warn('[NOTIFICATIONS] ⚠️ No active Service Worker to send config to');
        }
      } catch (error) {
        console.error('[NOTIFICATIONS] ❌ Error sending config to Service Worker:', error);
      }
    };

    // Delay config sending slightly to ensure SW is fully ready
    setTimeout(sendConfigToServiceWorker, 100);

    // Listen for messages from service worker (for auth token requests)
    const handleServiceWorkerMessage = (event: MessageEvent) => {
      if (event.data.type === 'GET_AUTH_TOKEN') {
        // Get the auth token from localStorage (key is 'authToken')
        const token = localStorage.getItem('authToken');
        console.log('[NOTIFICATIONS] Service Worker requested auth token:', token ? 'Found' : 'Not found');
        event.ports[0].postMessage({ token });
      }
    };

    navigator.serviceWorker?.addEventListener('message', handleServiceWorkerMessage);

    // Listen for foreground messages - this will continuously listen
    const unsubscribe = onMessageListener(async (payload: NotificationPayload) => {
      console.log('[NOTIFICATIONS] 📨 Foreground notification received:', payload);

      // Show notification with action buttons using Service Worker
      if (payload.notification && Notification.permission === 'granted') {
        const notificationData = (payload as { data?: Record<string, string> }).data || {};

        console.log('[NOTIFICATIONS] Notification permission granted, proceeding to show notification');
        console.log('[NOTIFICATIONS] Notification data:', notificationData);

        // Play notification sound
        playNotificationSound();

        // Manually trigger vibration if supported
        if ('vibrate' in navigator) {
          navigator.vibrate([200, 100, 200]);
        }

        // Use Service Worker to show notification (supports action buttons)
        try {
          console.log('[NOTIFICATIONS] Waiting for Service Worker to be ready...');
          const registration = await navigator.serviceWorker.ready;
          console.log('[NOTIFICATIONS] Service Worker is ready');

          // Build actions based on notification type
          // Note: Most browsers only support 2 action buttons maximum
          const actions: Array<{ action: string; title: string }> = [];
          if (notificationData.type === 'new_assignment' && notificationData.assignmentId) {
            actions.push(
              { action: 'accept', title: '✓ Accept' },
              { action: 'view', title: '👁 View Details' }
            );
          } else {
            actions.push(
              { action: 'view', title: '👁 View' }
            );
          }

          console.log('[NOTIFICATIONS] Built actions:', actions);
          console.log('[NOTIFICATIONS] Calling registration.showNotification...');

          await registration.showNotification(payload.notification.title, {
            body: payload.notification.body,
            icon: '/icons/icon-192x192.png',
            badge: '/icons/badge-72x72.png',
            tag: 'church-roster-' + (notificationData.assignmentId || Date.now()),
            requireInteraction: true,
            data: {
              ...notificationData,
              url: notificationData.assignmentId ? '/my-assignments' : '/',
              timestamp: Date.now()
            },
            actions: actions
          } as ExtendedNotificationOptions);

          console.log('[NOTIFICATIONS] ✅ Notification shown successfully with', actions.length, 'action buttons');
        } catch (error) {
          console.error('[NOTIFICATIONS] ❌ Failed to show notification via SW:', error);
          console.error('[NOTIFICATIONS] Error details:', error instanceof Error ? error.message : String(error));

          // Fallback to regular notification (no action buttons)
          try {
            console.log('[NOTIFICATIONS] Attempting fallback to regular notification...');
            new Notification(payload.notification.title, {
              body: payload.notification.body,
              icon: '/icons/icon-192x192.png',
              badge: '/icons/badge-72x72.png',
              tag: 'church-roster-' + (notificationData.assignmentId || Date.now()),
              requireInteraction: true,
              data: notificationData,
            });
            console.log('[NOTIFICATIONS] ✅ Fallback notification shown (no action buttons)');
          } catch (fallbackError) {
            console.error('[NOTIFICATIONS] ❌ Fallback notification also failed:', fallbackError);
          }
        }
      } else if (payload.notification && Notification.permission !== 'granted') {
        console.error('[NOTIFICATIONS] ❌ Cannot show notification - permission not granted');
        console.log('[NOTIFICATIONS] Current permission:', Notification.permission);
        console.log('[NOTIFICATIONS] Please grant notification permission');
      } else {
        console.warn('[NOTIFICATIONS] ⚠️ Notification payload missing notification field:', payload);
      }
    });

    // Cleanup listeners on unmount
    return () => {
      if (unsubscribe) {
        unsubscribe();
      }
      navigator.serviceWorker?.removeEventListener('message', handleServiceWorkerMessage);
    };
  }, []);

  const requestPermission = async () => {
    console.log('[NOTIFICATIONS] Requesting permission...');
    const token = await requestNotificationPermission();

    if (token) {
      console.log('[NOTIFICATIONS] ✅ Permission granted, device token received:', token.substring(0, 30) + '...');
      setDeviceToken(token);
      setNotificationPermission('granted');

      // Save token to backend
      try {
        console.log('[NOTIFICATIONS] Saving device token to backend...');
        const response = await apiService.post('/members/device-token', { deviceToken: token });
        console.log('[NOTIFICATIONS] ✅ Device token saved to backend successfully', response);
      } catch (error) {
        const err = error as { message?: string; response?: { data?: unknown; status?: number; statusText?: string } };
        console.error('[NOTIFICATIONS] ❌ Failed to save device token:', err);
        console.error('[NOTIFICATIONS] Error details:', {
          message: err.message,
          response: err.response?.data,
          status: err.response?.status,
          statusText: err.response?.statusText
        });

        // Show user-friendly error
        if (err.response?.status === 401) {
          console.error('[NOTIFICATIONS] ⚠️ Not authenticated - please log in again');
        } else if (err.response?.status === 400) {
          console.error('[NOTIFICATIONS] ⚠️ Bad request:', err.response?.data);
        }
      }
    } else {
      console.log('[NOTIFICATIONS] ❌ Permission denied or failed');
      setNotificationPermission('denied');
    }
  };

  return {
    notificationPermission,
    deviceToken,
    requestPermission
  };
};
