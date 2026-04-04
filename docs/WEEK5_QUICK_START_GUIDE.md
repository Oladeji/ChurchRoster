# 🚀 Week 5 Quick Start Guide
**Get Firebase Push Notifications & PDF Reports Working in 30 Minutes**

---

## Part 1: Firebase Setup (15 minutes)

### Step 1: Create Firebase Project
1. Go to https://console.firebase.google.com/
2. Click "Add project"
3. Name: `church-roster-system`
4. Disable Google Analytics (simpler)
5. Click "Create project"

### Step 2: Add Web App
1. Click Web icon (</>) in Firebase Console
2. App nickname: `church-roster-frontend`
3. Click "Register app"
4. **COPY** the firebaseConfig object - you'll need this!

### Step 3: Enable Cloud Messaging
1. Go to Project Settings (gear icon) → "Cloud Messaging" tab
2. Click "Generate key pair" under "Web Push certificates"
3. **COPY** the VAPID key

### Step 4: Download Service Account
1. Go to Project Settings → "Service accounts" tab
2. Click "Generate new private key"
3. Save the JSON file to `backend/firebase-adminsdk.json`
4. **IMPORTANT**: Add `firebase-adminsdk.json` to `.gitignore`

---

## Part 2: Configure Backend (5 minutes)

### Update appsettings.Development.json

```json
{
  "Firebase": {
    "ProjectId": "roster-system",
    "ServiceAccountPath": "firebase-adminsdk.json"
  }
}
```

### Verify Build

```bash
# Close Visual Studio first!
cd backend/ChurchRoster.Api
dotnet build
```

Expected: ✅ Build succeeded

---

## Part 3: Configure Frontend (10 minutes)

### Step 1: Install Firebase

```bash
cd frontend
npm install firebase
```

### Step 2: Create .env File

Create `frontend/.env`:

```env
# Replace with YOUR values from Step 2!
VITE_FIREBASE_API_KEY=AIzaSyXXXXXXXXXXXXXXXX
VITE_FIREBASE_AUTH_DOMAIN=church-roster-system.firebaseapp.com
VITE_FIREBASE_PROJECT_ID=church-roster-system
VITE_FIREBASE_STORAGE_BUCKET=church-roster-system.appspot.com
VITE_FIREBASE_MESSAGING_SENDER_ID=123456789012
VITE_FIREBASE_APP_ID=1:123456789012:web:abcdef1234567890
VITE_FIREBASE_VAPID_KEY=BNfXXXXXXXXXXXXXXXXXXX  # From Step 3!
```

### Step 3: Copy Frontend Code

The following files are documented in `WEEK5_PUSH_NOTIFICATIONS_IMPLEMENTATION.md`:

1. `frontend/src/config/firebase.config.ts`
2. `frontend/src/hooks/useNotifications.ts`
3. `frontend/public/firebase-messaging-sw.js` (⚠️ Replace Firebase config!)
4. Update `frontend/src/pages/Dashboard.tsx` (add notification prompt)
5. Update `frontend/index.html` (register service worker)

---

## Part 4: Test It! (10 minutes)

### Backend Test

```bash
cd backend/ChurchRoster.Api
dotnet run
```

Visit: http://localhost:5000/scalar/v1

Test Endpoints:
- `GET /api/reports/monthly-roster?year=2025&month=4`
- `POST /api/members/device-token` (with JWT auth)

### Frontend Test

```bash
cd frontend
npm run dev
```

Visit: http://localhost:3001

1. Login as a member
2. Click "Enable Notifications" on dashboard
3. Grant permission in browser
4. Login as admin in another browser
5. Create assignment for the member
6. Member should receive notification! 🎉

---

## 🎉 Done!

You now have:
- ✅ Push notifications working
- ✅ PDF report generation working
- ✅ Device token management
- ✅ Professional notification experience

---

## 🔧 Next: Complete Frontend

The backend is 100% ready. You just need to:

1. **Copy the frontend code** from the implementation guides
2. **Replace Firebase config** in service worker
3. **Create Reports page** (see `WEEK5_PDF_REPORTS_IMPLEMENTATION.md`)
4. **Test everything**

**All code is provided in the documentation!**

---

## 📚 Full Documentation

- **`WEEK5_FIREBASE_SETUP_GUIDE.md`** - Detailed Firebase setup
- **`WEEK5_PUSH_NOTIFICATIONS_IMPLEMENTATION.md`** - Full notification code
- **`WEEK5_PDF_REPORTS_IMPLEMENTATION.md`** - Full report code
- **`WEEK5_IMPLEMENTATION_SUMMARY.md`** - Complete overview

---

## 🆘 Quick Troubleshooting

| Problem | Solution |
|---------|----------|
| Build fails | Close Visual Studio, run `dotnet clean`, then `dotnet build` |
| Firebase errors | Check `firebase-adminsdk.json` path in appsettings.json |
| Notifications not working | Verify VAPID key in `.env` matches Firebase Console |
| PDFs not downloading | Check backend logs, verify assignments exist for date range |

---

**Have fun! 🎊** You're almost done with Week 5!
