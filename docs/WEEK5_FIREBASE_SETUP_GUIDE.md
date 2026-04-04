# 🔥 Firebase Setup Guide for Push Notifications
**Church Ministry Roster System - Week 5, Day 4**

---

## 📋 Overview

This guide walks you through setting up Firebase Cloud Messaging (FCM) for push notifications in the Church Roster system. By the end, you'll have:

- ✅ Firebase project created
- ✅ VAPID keys for web push
- ✅ Service account JSON for backend
- ✅ Frontend configured to receive notifications
- ✅ Backend configured to send notifications

**Estimated Time:** 30-45 minutes

---

## 🎯 Step 1: Create Firebase Project

### 1.1 Go to Firebase Console

1. Open your browser and go to: **https://console.firebase.google.com/**
2. Click **"Add project"** or **"Create a project"**

### 1.2 Configure Project Settings

**Screen 1: Project Name**
- **Project Name:** `church-roster-system` (or your preferred name)
- Click **Continue**

**Screen 2: Google Analytics** (Optional)
- Toggle **OFF** if you don't need analytics (simpler setup)
- Or toggle **ON** and select/create Analytics account
- Click **Create project**

**Wait for project creation** (15-30 seconds)

Once done, click **"Continue"** to enter your project dashboard.

---

## 🌐 Step 2: Add Web App to Firebase

### 2.1 Register Your Web App

1. From Firebase Console dashboard, click the **Web icon** (</> symbol)
   - Or go to **Project Settings** → **General** → **Your apps** → **Add app** → **Web**

2. **Register app:**
   - **App nickname:** `church-roster-frontend`
   - ✅ **Check** "Also set up Firebase Hosting" (optional - we're using Vercel but doesn't hurt)
   - Click **Register app**

3. **Copy Firebase SDK Configuration**

You'll see code like this:

```javascript
const firebaseConfig = {
  apiKey: "AIzaSyXXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
  authDomain: "church-roster-system.firebaseapp.com",
  projectId: "church-roster-system",
  storageBucket: "church-roster-system.appspot.com",
  messagingSenderId: "123456789012",
  appId: "1:123456789012:web:abcdef1234567890"
};
```

**⚠️ SAVE THIS CONFIGURATION** - you'll need it for frontend `.env` file

4. Click **"Continue to console"**

---

## 🔔 Step 3: Enable Cloud Messaging

### 3.1 Enable FCM API

1. In Firebase Console, go to **Project Settings** (gear icon ⚙️ top left)
2. Click the **"Cloud Messaging"** tab
3. You should see **"Cloud Messaging API (Legacy) ENABLED"**
   - If it says "disabled", click **"Enable"**

### 3.2 Generate VAPID Key (Web Push Certificate)

**Still in Cloud Messaging tab:**

1. Scroll down to **"Web configuration"** section
2. Under **"Web Push certificates"**, click **"Generate key pair"**
3. A new VAPID key will be generated

**Example:**
```
Key: BNfXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
```

**⚠️ COPY THIS VAPID KEY** - you'll need it for frontend configuration

---

## 🔑 Step 4: Generate Service Account Key (Backend)

### 4.1 Create Service Account

1. In Firebase Console, go to **Project Settings** → **Service accounts** tab
2. Click **"Generate new private key"**
3. A dialog appears: **"Generate new private key"**
   - Click **"Generate key"**
4. A JSON file downloads automatically (e.g., `church-roster-system-firebase-adminsdk-xxxxx.json`)

**⚠️ KEEP THIS FILE SECURE** - it grants full access to your Firebase project

**Example JSON structure:**
```json
{
  "type": "service_account",
  "project_id": "church-roster-system",
  "private_key_id": "abc123...",
  "private_key": "-----BEGIN PRIVATE KEY-----\nMIIEvQIBA...",
  "client_email": "firebase-adminsdk-xxxxx@church-roster-system.iam.gserviceaccount.com",
  "client_id": "123456789012345678901",
  "auth_uri": "https://accounts.google.com/o/oauth2/auth",
  "token_uri": "https://oauth2.googleapis.com/token",
  "auth_provider_x509_cert_url": "https://www.googleapis.com/oauth2/v1/certs",
  "client_x509_cert_url": "https://www.googleapis.com/..."
}
```

---

## 📝 Step 5: Configure Frontend (.env)

### 5.1 Create/Update Frontend `.env` File

In your `frontend/` folder, create or update `.env`:

```bash
# Firebase Configuration
VITE_FIREBASE_API_KEY=AIzaSyXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
VITE_FIREBASE_AUTH_DOMAIN=church-roster-system.firebaseapp.com
VITE_FIREBASE_PROJECT_ID=church-roster-system
VITE_FIREBASE_STORAGE_BUCKET=church-roster-system.appspot.com
VITE_FIREBASE_MESSAGING_SENDER_ID=123456789012
VITE_FIREBASE_APP_ID=1:123456789012:web:abcdef1234567890
VITE_FIREBASE_VAPID_KEY=BNfXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

# API URL (update to your backend URL)
VITE_API_URL=http://localhost:5000/api
```

**📌 How to fill this in:**
- Use the `firebaseConfig` values from **Step 2.1**
- Use the VAPID key from **Step 3.2**

### 5.2 Add `.env` to `.gitignore`

**⚠️ IMPORTANT:** Never commit `.env` to GitHub!

In `frontend/.gitignore`, ensure you have:
```
.env
.env.local
.env.production
```

---

## 🔧 Step 6: Configure Backend (appsettings.json)

### 6.1 Store Service Account JSON

**Option A: Environment Variable (Recommended for Production)**

1. Copy the entire JSON content from the downloaded file
2. Minify it (remove line breaks): https://codebeautify.org/jsonminifier
3. Store as environment variable:

**For Local Development (`appsettings.Development.json`):**
```json
{
  "Firebase": {
    "ServiceAccountJson": "{\"type\":\"service_account\",\"project_id\":\"church-roster-system\",...}"
  }
}
```

**For Render Deployment:**
- Go to Render Dashboard → Your Service → Environment
- Add environment variable:
  - **Key:** `Firebase__ServiceAccountJson`
  - **Value:** (paste minified JSON)

**Option B: File Path (Local Development Only)**

1. Save the downloaded JSON file to `backend/firebase-adminsdk.json`
2. Add to `.gitignore`:
   ```
   firebase-adminsdk.json
   ```
3. Update `appsettings.Development.json`:
   ```json
   {
     "Firebase": {
       "ServiceAccountPath": "firebase-adminsdk.json"
     }
   }
   ```

### 6.2 Update appsettings.json

Add Firebase configuration:

```json
{
  "Firebase": {
    "ProjectId": "church-roster-system",
    "ServiceAccountJson": "",
    "ServiceAccountPath": ""
  }
}
```

---

## ✅ Step 7: Verify Setup

### 7.1 Checklist

Before proceeding to implementation, verify you have:

- [ ] Firebase project created
- [ ] Web app registered in Firebase
- [ ] Cloud Messaging API enabled
- [ ] VAPID key generated and copied
- [ ] Service account JSON downloaded
- [ ] Frontend `.env` configured with Firebase keys
- [ ] Backend `appsettings.json` configured
- [ ] `.env` and `firebase-adminsdk.json` added to `.gitignore`
- [ ] Environment variables ready for Render deployment

### 7.2 Common Issues

| Issue | Solution |
|-------|----------|
| "Cloud Messaging API is disabled" | Go to Project Settings → Cloud Messaging → Enable API |
| "VAPID key not found" | Generate new key pair in Web Push certificates section |
| Service account JSON not downloading | Check popup blockers, try different browser |
| CORS errors in browser console | Ensure Firebase domain is added to allowed origins |

---

## 🚀 Step 8: Next Steps

Now that Firebase is configured, proceed to:

1. **Install Firebase packages** (frontend & backend)
2. **Implement service worker** for receiving notifications
3. **Create notification service** in backend
4. **Test push notifications** locally

Refer to `WEEK5_PUSH_NOTIFICATIONS_IMPLEMENTATION.md` for code implementation.

---

## 📚 Additional Resources

- **Firebase Console:** https://console.firebase.google.com
- **FCM Documentation:** https://firebase.google.com/docs/cloud-messaging
- **Web Push Protocol:** https://web.dev/push-notifications-overview/
- **VAPID Keys Explained:** https://blog.mozilla.org/services/2016/08/23/sending-vapid-identified-webpush-notifications-via-mozillas-push-service/

---

## 🆘 Need Help?

If you encounter issues:

1. Check Firebase Console → **Usage & Billing** (ensure free tier is active)
2. Verify **Cloud Messaging API** is enabled in Google Cloud Console
3. Check browser console for detailed error messages
4. Test with Firebase's test notification feature first

---

**Last Updated:** January 2025  
**Document Version:** 1.0  
**Next Document:** `WEEK5_PUSH_NOTIFICATIONS_IMPLEMENTATION.md`
