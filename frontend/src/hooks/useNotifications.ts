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

// Module-level guard: only one foreground message listener should be active at a time.
let _listenerActive = false;

// Notification sound helper
const playNotificationSound = () => {
  try {
    const audio = new Audio('/sounds/notification.mp3');
    audio.volume = 0.5;
    audio.play().catch(err => {
      console.warn('Could not play notification sound:', err);
    });
  } catch (error) {
    console.warn('Notification sound error:', error);
  }
};

// Save (or refresh) the FCM device token to the backend.
// Called on mount (auto-refresh) and when the user explicitly grants permission.
const saveTokenToBackend = async (token: string) => {
  try {
    await apiService.post('/members/device-token', { deviceToken: token });
    console.log('[NOTIFICATIONS] ✅ Device token saved/refreshed in backend');
  } catch (error) {
    const err = error as { response?: { status?: number; data?: unknown } };
    console.error('[NOTIFICATIONS] ❌ Failed to save device token:', err.response?.data ?? error);
  }
};

export const useNotifications = () => {
  const [notificationPermission, setNotificationPermission] = useState<NotificationPermission>('default');
  const [deviceToken, setDeviceToken] = useState<string | null>(null);

  useEffect(() => {
    if ('Notification' in window) {
      setNotificationPermission(Notification.permission);
    }

    // ─── Auto-refresh FCM token ───────────────────────────────────────────────
    // Firebase rotates tokens periodically. Refresh and re-save on every mount
    // so the backend always has a valid token.
    const autoRefreshToken = async () => {
      if (!('Notification' in window) || Notification.permission !== 'granted') return;
      if (!localStorage.getItem('authToken')) return; // not logged in yet
      try {
        const token = await requestNotificationPermission();
        if (token) {
          setDeviceToken(token);
          await saveTokenToBackend(token);
        }
      } catch (err) {
        console.warn('[NOTIFICATIONS] Auto-refresh token failed:', err);
      }
    };
    autoRefreshToken();

    // ─── Service Worker config ────────────────────────────────────────────────
    const sendConfigToServiceWorker = async () => {
      try {
        if (!('serviceWorker' in navigator)) return;
        const apiUrl = import.meta.env.VITE_API_URL || 'https://localhost:7288/api';
        localStorage.setItem('apiUrl', apiUrl);
        const registration = await navigator.serviceWorker.ready;
        registration.active?.postMessage({ type: 'SET_API_URL', apiUrl });
      } catch (error) {
        console.error('[NOTIFICATIONS] ❌ Error sending config to Service Worker:', error);
      }
    };
    setTimeout(sendConfigToServiceWorker, 100);

    // ─── Auth-token bridge for service worker ─────────────────────────────────
    const handleServiceWorkerMessage = (event: MessageEvent) => {
      if (event.data.type === 'GET_AUTH_TOKEN') {
        const token = localStorage.getItem('authToken');
        event.ports[0].postMessage({ token });
      }
    };
    navigator.serviceWorker?.addEventListener('message', handleServiceWorkerMessage);

    // ─── Foreground message listener (one instance across the whole app) ───────
    let unsubscribe: (() => void) | null = null;
    if (!_listenerActive) {
      _listenerActive = true;
      unsubscribe = onMessageListener(async (payload: NotificationPayload) => {
        console.log('[NOTIFICATIONS] 📨 Foreground message received:', payload);

        if (!payload.notification || Notification.permission !== 'granted') {
          console.warn('[NOTIFICATIONS] ⚠️ Cannot show notification:', !payload.notification ? 'no notification field' : 'permission not granted');
          return;
        }

        const notificationData = (payload as { data?: Record<string, string> }).data || {};
        playNotificationSound();
        if ('vibrate' in navigator) navigator.vibrate([200, 100, 200]);

        const actions: Array<{ action: string; title: string }> =
          notificationData.type === 'new_assignment' && notificationData.assignmentId
            ? [{ action: 'accept', title: '✓ Accept' }, { action: 'view', title: '👁 View Details' }]
            : [{ action: 'view', title: '👁 View' }];

        try {
          const registration = await navigator.serviceWorker.ready;
          await registration.showNotification(payload.notification.title, {
            body: payload.notification.body,
            icon: '/icons/icon-192x192.png',
            badge: '/icons/badge-72x72.png',
            tag: 'church-roster-' + (notificationData.assignmentId || Date.now()),
            requireInteraction: true,
            data: { ...notificationData, url: notificationData.assignmentId ? '/my-assignments' : '/', timestamp: Date.now() },
            actions,
          } as ExtendedNotificationOptions);
          console.log('[NOTIFICATIONS] ✅ Notification shown');
        } catch {
          // Fallback: basic Notification API
          try {
            new Notification(payload.notification.title, {
              body: payload.notification.body,
              icon: '/icons/icon-192x192.png',
              tag: 'church-roster-' + (notificationData.assignmentId || Date.now()),
              requireInteraction: true,
            });
          } catch (fallbackError) {
            console.error('[NOTIFICATIONS] ❌ Fallback notification failed:', fallbackError);
          }
        }
      });
    }

    return () => {
      navigator.serviceWorker?.removeEventListener('message', handleServiceWorkerMessage);
      if (unsubscribe) {
        unsubscribe();
        _listenerActive = false;
      }
    };
  }, []);

  const requestPermission = async () => {
    console.log('[NOTIFICATIONS] Requesting permission...');
    const token = await requestNotificationPermission();

    if (token) {
      console.log('[NOTIFICATIONS] ✅ Permission granted, device token received');
      setDeviceToken(token);
      setNotificationPermission('granted');
      await saveTokenToBackend(token);
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
