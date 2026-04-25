import { initializeApp } from 'firebase/app';
import { getMessaging, getToken, onMessage } from 'firebase/messaging';
import type { Messaging } from 'firebase/messaging';

interface NotificationPayload {
  notification?: {
    title: string;
    body: string;
  };
}

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

    if (permission !== 'granted') {
      console.log('Notification permission denied');
      return null;
    }

    // On iOS PWA, Firebase MUST receive the exact SW registration that handles
    // push messages. Passing it explicitly prevents silent token failures when
    // multiple service workers are registered (e.g. firebase-messaging-sw.js + sw.js).
    let swRegistration: ServiceWorkerRegistration | undefined;
    if ('serviceWorker' in navigator) {
      const registrations = await navigator.serviceWorker.getRegistrations();
      swRegistration = registrations.find(r =>
        r.active?.scriptURL.includes('firebase-messaging-sw.js')
      );
      if (!swRegistration) {
        // Not registered yet — register it now and wait for it to activate
        swRegistration = await navigator.serviceWorker.register('/firebase-messaging-sw.js');
        await navigator.serviceWorker.ready;
      }
    }

    const token = await getToken(messaging, {
      vapidKey: import.meta.env.VITE_FIREBASE_VAPID_KEY,
      serviceWorkerRegistration: swRegistration,
    });

    console.log('FCM Token obtained:', token ? token.substring(0, 20) + '...' : 'null');
    return token || null;
  } catch (error) {
    console.error('Error getting notification permission:', error);
    return null;
  }
};

export const onMessageListener = (callback: (payload: NotificationPayload) => void): (() => void) | null => {
  if (!messaging) {
    console.warn('Firebase Messaging not initialized');
    return null;
  }

  // Register the continuous listener
  const unsubscribe = onMessage(messaging, (payload) => {
    console.log('Message received:', payload);
    callback(payload as NotificationPayload);
  });

  // Return unsubscribe function for cleanup
  return unsubscribe;
};
