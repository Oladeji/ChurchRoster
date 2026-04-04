# Notification Sound

This directory should contain a notification sound file named `notification.mp3`.

## How to add a custom sound:

1. Find or create a pleasant notification sound (2-3 seconds recommended)
2. Save it as `notification.mp3` in this directory
3. Keep the file size small (< 50KB recommended)

## Free sound resources:
- Zapsplat: https://www.zapsplat.com/sound-effect-category/notification/
- Freesound: https://freesound.org/search/?q=notification
- Notification Sounds: https://notificationsounds.com/

## Fallback:
If `notification.mp3` is not found, the app will:
1. Try to use a system beep sound
2. Fall back to silent (browser default sound if enabled)

## Alternative format:
You can also use `notification.ogg` or `notification.wav` if you prefer.
Just update the file extension in `useNotifications.ts`.
