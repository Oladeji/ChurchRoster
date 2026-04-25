// Re-export the shared Firebase messaging utilities.
// Do NOT call initializeApp() here — the app is already initialized in firebase.config.ts.
export { requestNotificationPermission, onMessageListener } from '../config/firebase.config';
