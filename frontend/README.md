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
