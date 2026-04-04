# 🔊 Notification Sounds

## Quick Setup

Add a notification sound file named `notification.mp3` to this directory.

## Recommended Specifications

- **Format**: MP3 (or OGG/WAV)
- **Duration**: 1-3 seconds
- **File Size**: < 50KB
- **Bitrate**: 128kbps or lower
- **Style**: Pleasant, non-intrusive chime or bell

## Free Sound Resources

### 1. **Zapsplat** (Free with attribution)
- URL: https://www.zapsplat.com/sound-effect-category/notification/
- Quality: Professional
- Formats: MP3, WAV
- License: Free with credit

### 2. **Freesound** (Creative Commons)
- URL: https://freesound.org/search/?q=notification
- Quality: Varies
- Formats: Multiple
- License: CC BY or CC0

### 3. **Notification Sounds** (Free)
- URL: https://notificationsounds.com/
- Quality: Good
- Formats: MP3
- License: Free for personal/commercial

### 4. **Pixabay** (Royalty-free)
- URL: https://pixabay.com/sound-effects/search/notification/
- Quality: High
- Formats: MP3
- License: Royalty-free

## Creating Your Own Sound

### Using Audacity (Free)
1. Download Audacity: https://www.audacityteam.org/
2. Generate → Tone → Sine wave, 800 Hz, 0.3 seconds
3. Effect → Fade Out (last 0.1 seconds)
4. Export as MP3
5. Keep under 50KB

### Quick Online Tool
- URL: https://www.beepbox.co/
- Create a simple melody
- Export as WAV
- Convert to MP3 using: https://cloudconvert.com/wav-to-mp3

## Current Status

- [ ] `notification.mp3` - **NOT FOUND** (will use fallback beep)

## Testing

After adding the file:
1. Restart the frontend dev server
2. Create an assignment
3. Listen for the sound when notification appears

If the sound doesn't play:
- Check browser console for errors
- Verify file is exactly named `notification.mp3`
- Try a different format (notification.ogg or notification.wav)
- Update the filename in `src/hooks/useNotifications.ts`

## Fallback Behavior

If no sound file is found, the app will:
1. Try to play a system beep (data URI audio)
2. Fall back to silent mode
3. Browser may still play its default notification sound (if enabled)

---

**Tip**: Test the sound volume before committing. It should be pleasant but not startling!
