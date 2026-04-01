# Quick Start Guide - Frontend Testing

## Prerequisites

- Backend API is running (locally or on Render)
- Node.js 18+ installed
- npm or yarn installed

## Step 1: Navigate to Frontend

```bash
cd frontend
```

## Step 2: Install Dependencies (if not already done)

```bash
npm install
```

## Step 3: Configure Environment

Create/update `.env` file:

**For Production Backend (Render):**
```env
VITE_API_URL=https://churchroster.onrender.com/api
```

**For Local Backend:**
```env
VITE_API_URL=http://localhost:5000/api
```

## Step 4: Start Development Server

```bash
npm run dev
```

The app will start at **http://localhost:3000**

## Step 5: Test Login

Open **http://localhost:3000** in your browser.

### Test Accounts

**Admin Account:**
- Email: `admin@church.com`
- Password: `Admin123!`

**Member Account:**
- Email: `john.doe@church.com`
- Password: `Member123!`

## Expected Behavior

### Login Page
1. Should see a beautiful login form with gradient background
2. Enter credentials and click "Login"
3. Should redirect to `/dashboard` on success
4. Should show error message if credentials are wrong

### Admin Dashboard
1. Welcome message with admin name
2. Logout button in top right
3. Four dashboard cards:
   - 👥 Members
   - 📋 Assignments
   - 📅 Calendar
   - 📊 Reports

### Member Dashboard
1. Welcome message with member name
2. Logout button in top right
3. Two dashboard cards:
   - 📝 My Assignments
   - 📅 Calendar

### Protected Routes
1. Try accessing `/dashboard` without logging in
2. Should automatically redirect to `/login`
3. After login, should access `/dashboard` successfully

### Logout
1. Click "Logout" button
2. Should redirect to `/login`
3. Token should be removed from localStorage

## Troubleshooting

### Can't connect to backend
- Check `VITE_API_URL` in `.env`
- Verify backend is running
- Check browser console for errors
- Test backend directly: `https://churchroster.onrender.com/api/health` (if health endpoint exists)

### Login fails with CORS error
- Backend must allow frontend origin in CORS settings
- Check backend `CorsSettings` in `appsettings.json`
- Add `http://localhost:3000` to allowed origins

### Page is blank
- Check browser console for JavaScript errors
- Run `npm run build` to check for TypeScript errors
- Clear browser cache and refresh

### Styles not loading
- Check that `index.css` and `App.css` are imported
- Clear Vite cache: `rm -rf node_modules/.vite`
- Restart dev server

## Build for Production

```bash
npm run build
```

Should complete successfully with:
- ✓ TypeScript compilation
- ✓ Vite build
- ✓ PWA files generated
- ✓ Output in `dist/` folder

## Preview Production Build

```bash
npm run preview
```

Will start at **http://localhost:4173**

## Test PWA Installation

### Chrome Desktop
1. Open the app
2. Look for install icon in address bar (⊕)
3. Click "Install"
4. App opens in standalone window

### Mobile (Chrome/Safari)
1. Open the app in mobile browser
2. **iOS Safari**: Tap Share → "Add to Home Screen"
3. **Android Chrome**: Tap menu (⋮) → "Install App"
4. App icon appears on home screen
5. Open app from home screen

## Development Tips

### Hot Module Replacement
- Save any file in `src/`
- Changes appear instantly in browser
- No need to refresh

### TypeScript Errors
- Fix all TypeScript errors before building
- Run `npm run build` to check
- Use `npm run lint` for code quality

### API Service
- All API calls go through `api.service.ts`
- Token automatically injected in requests
- 401 responses auto-redirect to login

### State Management
- Authentication state in `AuthContext`
- Use `useAuth()` hook in components
- User info, isAuthenticated, isAdmin available

## Next Steps

Once Week 3 is working:
1. Week 4: Build Calendar component
2. Week 4: Build Assignment modal
3. Week 5: Add push notifications
4. Week 6: Polish and deploy

## Success Checklist

- [ ] Frontend starts without errors
- [ ] Login page renders correctly
- [ ] Can log in with test accounts
- [ ] Dashboard shows correct role-based view
- [ ] Logout works and redirects to login
- [ ] Protected routes redirect unauthenticated users
- [ ] Build command completes successfully
- [ ] PWA can be installed

If all checkboxes are ✅, Week 3 is complete! 🎉
