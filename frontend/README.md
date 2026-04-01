# Church Roster System - Frontend

This is the frontend application for the Church Ministry Rostering System built with React, TypeScript, and Vite.

## Project Structure

```
frontend/
├── public/
│   ├── manifest.json          # PWA manifest
│   └── icons/                 # PWA icons
├── src/
│   ├── components/            # Reusable React components
│   │   └── ProtectedRoute.tsx
│   ├── pages/                 # Page components
│   │   ├── Login.tsx
│   │   └── Dashboard.tsx
│   ├── services/              # API service layer
│   │   ├── api.service.ts
│   │   ├── auth.service.ts
│   │   ├── assignment.service.ts
│   │   ├── member.service.ts
│   │   ├── task.service.ts
│   │   ├── skill.service.ts
│   │   └── firebase.service.ts
│   ├── hooks/                 # Custom React hooks
│   │   ├── useAssignments.ts
│   │   ├── useMembers.ts
│   │   └── useTasks.ts
│   ├── context/               # React Context providers
│   │   └── AuthContext.tsx
│   ├── types/                 # TypeScript type definitions
│   │   └── index.ts
│   ├── utils/                 # Utility functions
│   │   └── helpers.ts
│   ├── styles/                # CSS styles
│   │   └── main.css
│   ├── App.tsx                # Main App component
│   └── main.tsx               # Application entry point
├── vite.config.ts             # Vite configuration with PWA
├── package.json               # Dependencies
└── .env.example               # Environment variables template
```

## Getting Started

### Prerequisites

- Node.js 18+ and npm
- Backend API running (see backend README)

### Installation

1. Install dependencies:
   ```bash
   npm install
   ```

2. Create environment file:
   ```bash
   cp .env.example .env
   ```

3. Update `.env` with your configuration:
   ```env
   VITE_API_URL=http://localhost:5000/api
   VITE_FIREBASE_API_KEY=your_firebase_api_key
   # ... other Firebase config
   ```

### Development

Run the development server:
```bash
npm run dev
```

The app will be available at `http://localhost:3000`

### Build

Build for production:
```bash
npm run build
```

Preview production build:
```bash
npm run preview
```

## Features

- ✅ **React 19** with TypeScript
- ✅ **Vite** for fast development and builds
- ✅ **PWA Support** - Installable on mobile devices
- ✅ **Firebase Cloud Messaging** - Push notifications
- ✅ **React Router** - Client-side routing
- ✅ **Axios** - HTTP client with interceptors
- ✅ **Context API** - State management
- ✅ **Custom Hooks** - Reusable logic
- ✅ **TypeScript** - Type safety

## PWA Installation

### iOS
1. Open the app in Safari
2. Tap the Share button
3. Select "Add to Home Screen"
4. Tap "Add"

### Android
1. Open the app in Chrome
2. Tap the menu (⋮)
3. Select "Install App" or "Add to Home Screen"
4. Tap "Install"

## Key Components

### Services
- **api.service.ts**: Base API client with JWT token management
- **auth.service.ts**: Authentication (login, register, logout)
- **assignment.service.ts**: Assignment CRUD operations
- **member.service.ts**: Member management
- **task.service.ts**: Task management
- **skill.service.ts**: Skill management
- **firebase.service.ts**: Push notifications

### Context
- **AuthContext**: Global authentication state

### Hooks
- **useAssignments**: Fetch and manage assignments
- **useMembers**: Fetch and manage members
- **useTasks**: Fetch and manage tasks

## Environment Variables

| Variable | Description |
|----------|-------------|
| `VITE_API_URL` | Backend API URL |
| `VITE_FIREBASE_API_KEY` | Firebase API key |
| `VITE_FIREBASE_AUTH_DOMAIN` | Firebase auth domain |
| `VITE_FIREBASE_PROJECT_ID` | Firebase project ID |
| `VITE_FIREBASE_STORAGE_BUCKET` | Firebase storage bucket |
| `VITE_FIREBASE_MESSAGING_SENDER_ID` | Firebase messaging sender ID |
| `VITE_FIREBASE_APP_ID` | Firebase app ID |
| `VITE_FIREBASE_MEASUREMENT_ID` | Firebase measurement ID |
| `VITE_FIREBASE_VAPID_KEY` | Firebase VAPID key for push notifications |

## Deployment

### Vercel (Recommended)

1. Push code to GitHub
2. Import project in Vercel
3. Set environment variables
4. Deploy

Vercel will automatically:
- Build the app
- Enable HTTPS
- Provide a custom domain

### Other Platforms

Build the app:
```bash
npm run build
```

Deploy the `dist` folder to:
- Netlify
- GitHub Pages
- Firebase Hosting
- Any static hosting service

## Browser Support

- Chrome 90+
- Firefox 88+
- Safari 14+
- Edge 90+

## License

This project is for church ministry use.
import reactDom from 'eslint-plugin-react-dom'

export default defineConfig([
  globalIgnores(['dist']),
  {
    files: ['**/*.{ts,tsx}'],
    extends: [
      // Other configs...
      // Enable lint rules for React
      reactX.configs['recommended-typescript'],
      // Enable lint rules for React DOM
      reactDom.configs.recommended,
    ],
    languageOptions: {
      parserOptions: {
        project: ['./tsconfig.node.json', './tsconfig.app.json'],
        tsconfigRootDir: import.meta.dirname,
      },
      // other options...
    },
  },
])
```


---

## 📅 Project Progress

### ✅ Week 1: Backend Foundation (COMPLETE)
- [x] .NET 10 Web API project structure
- [x] Entity Framework Core with PostgreSQL
- [x] Database schema and migrations
- [x] JWT authentication endpoints
- [x] Clean Architecture implementation
- [x] Central package management
- [x] Docker configuration

**Status:** ✅ Completed | **Date:** March 2026

---

### ✅ Week 2: Backend Core Features (COMPLETE)
- [x] Member Management CRUD endpoints (8 endpoints)
- [x] Skills Management endpoints (9 endpoints)
- [x] Task Catalog endpoints (8 endpoints)
- [x] Assignment endpoints with validation (9 endpoints)
- [x] Business rules implementation (5 rules)
- [x] Deployed to Render
- [x] Database seeded with test data

**Status:** ✅ Completed | **Date:** March 2026

---

### ✅ Week 3: Frontend Foundation (COMPLETE)
- [x] React 19.2 + TypeScript + Vite 8.0 setup
- [x] PWA configuration (manifest, service worker)
- [x] Login page with JWT authentication
- [x] Admin dashboard with 4 feature cards
- [x] Member dashboard with 2 feature cards
- [x] Protected routes with role-based access
- [x] API integration layer (all services)
- [x] Custom React hooks for data fetching
- [x] Responsive UI design
- [x] Build successful (zero errors)

**Status:** ✅ Completed | **Date:** March 12, 2026

**Deliverables:**
- ✅ Installable PWA
- ✅ Authentication flow working
- ✅ Role-based dashboards
- ✅ Complete API integration
- ✅ Comprehensive documentation

**Documentation:**
- [Week 3 Summary](docs/WEEK3_SUMMARY.md)
- [Week 3 Complete Details](docs/WEEK3_COMPLETE.md)
- [Frontend Quick Start](docs/FRONTEND_QUICK_START.md)
- [Backend-Frontend Integration](docs/BACKEND_FRONTEND_INTEGRATION.md)
- [Validation Checklist](docs/WEEK3_VALIDATION_CHECKLIST.md)

---

### 🔄 Week 4: Calendar & Assignment UI (NEXT)
- [ ] Calendar component (month view)
- [ ] Assignment creation modal
- [ ] Qualification filtering
- [ ] Task status indicators
- [ ] Member task list view
- [ ] Accept/Reject workflow

**Status:** 🔜 Planned

---

### ⏳ Week 5: Notifications & Reports (FUTURE)
- [ ] Firebase push notifications
- [ ] Email notifications (Brevo)
- [ ] Printable reports (PDF)
- [ ] Notification preferences

**Status:** ⏳ Pending

---

### ⏳ Week 6: Polish & Launch (FUTURE)
- [ ] User testing
- [ ] Bug fixes
- [ ] Security audit
- [ ] Performance optimization
- [ ] Production deployment
- [ ] User training

**Status:** ⏳ Pending

---

## 🎯 Overall Progress: 50% Complete

**Weeks Completed:** 3 of 6  
**Backend:** 100% (All APIs ready)  
**Frontend:** 50% (Foundation complete, UI components next)  
**Database:** 100% (Schema, migrations, seed data)  
**Documentation:** 90% (Comprehensive guides available)

---

## 🚀 Current Status

### What's Working ✅
1. **Backend API** - All 34 endpoints deployed and operational
2. **Authentication** - Login/Register with JWT tokens
3. **Database** - PostgreSQL on Supabase with seeded data
4. **Frontend Auth** - Login page, protected routes, role-based access
5. **PWA** - Installable on mobile devices
6. **API Integration** - Complete service layer connecting frontend to backend
7. **Docker** - Backend containerized and deployed on Render

### What's Next 🔜
1. **Calendar UI** - Month view with assignments
2. **Assignment Modal** - Create and assign tasks
3. **Member Management UI** - CRUD operations for members
4. **Task Management UI** - View and manage tasks
5. **Notifications** - Push and email notifications

### How to Test Current Progress

**1. Test Backend API:**
`ash
# Using deployed backend
curl https://churchroster.onrender.com/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{\"email\":\"admin@church.com\",\"password\":\"Admin123!\"}'
`

**2. Test Frontend:**
`ash
cd frontend
npm install
npm run dev
# Open http://localhost:3000
# Login: admin@church.com / Admin123!
`

**3. Test PWA:**
- Open app in Chrome
- Click install icon in address bar
- App opens in standalone window

---

## 📞 Support & Resources

### Documentation
- 📘 [Full Requirements Document](docs/Requirements_Document.md)
- 🏗️ [Technical Architecture](docs/Technical_Architecture.md)
- 🛠️ [Development Guide](docs/Development_Guide.md)
- ✅ [Week 3 Summary](docs/WEEK3_SUMMARY.md)

### Quick Links
- Backend API: https://churchroster.onrender.com
- API Documentation: https://churchroster.onrender.com/scalar/v1
- Source Code: [GitHub Repository]
- Issue Tracker: [GitHub Issues]

### Contact
For questions or issues:
1. Check the [documentation](docs/)
2. Review [troubleshooting guides](docs/WEEK3_VALIDATION_CHECKLIST.md)
3. Contact project administrator

---

## 🎉 Success Milestones

- ✅ **Week 1:** Backend foundation established
- ✅ **Week 2:** All CRUD operations implemented
- ✅ **Week 3:** Frontend foundation complete
- 🎯 **Week 4:** Calendar and assignment UI (In progress)
- 🎯 **Week 5:** Notifications and reports
- 🎯 **Week 6:** Production launch

**Next Major Milestone:** Week 4 - Calendar & Assignment UI Implementation

---

*Last Updated: March 12, 2026*  
*Version: 3.0 - Week 3 Complete*  
*Project: Church Ministry Rostering System*
