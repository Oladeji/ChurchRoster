import { initializeApp } from 'firebase/app';
import { getMessaging, getToken, onMessage, Messaging } from 'firebase/messaging';

const firebaseConfig = {
  apiKey: import.meta.env.VITE_FIREBASE_API_KEY,
  authDomain: import.meta.env.VITE_FIREBASE_AUTH_DOMAIN,
  projectId: import.meta.env.VITE_FIREBASE_PROJECT_ID,
  storageBucket: import.meta.env.VITE_FIREBASE_STORAGE_BUCKET,
  messagingSenderId: import.meta.env.VITE_FIREBASE_MESSAGING_SENDER_ID,
  appId: import.meta.env.VITE_FIREBASE_APP_ID,
  measurementId: import.meta.env.VITE_FIREBASE_MEASUREMENT_ID,
};

const vapidKey = import.meta.env.VITE_FIREBASE_VAPID_KEY;

class FirebaseService {
  private messaging: Messaging | null = null;

  initialize() {
    try {
      const app = initializeApp(firebaseConfig);
      this.messaging = getMessaging(app);
      console.log('Firebase initialized successfully');
    } catch (error) {
      console.error('Error initializing Firebase:', error);
    }
  }

  async requestPermission(): Promise<string | null> {
    try {
      const permission = await Notification.requestPermission();

      if (permission === 'granted' && this.messaging) {
        console.log('Notification permission granted.');
        const token = await getToken(this.messaging, { vapidKey });
        console.log('FCM Token:', token);
        return token;
      } else {
        console.log('Notification permission denied.');
        return null;
      }
    } catch (error) {
      console.error('Error requesting notification permission:', error);
      return null;
    }
  }

  onMessageListener(): Promise<any> {
    return new Promise((resolve) => {
      if (this.messaging) {
        onMessage(this.messaging, (payload) => {
          console.log('Message received:', payload);
          resolve(payload);
        });
      }
    });
  }
}

export default new FirebaseService();
