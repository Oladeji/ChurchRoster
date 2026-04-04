// Config file for public pages (test-accept.html, health-check.html)
// This file is generated from .env during build

// Read from localStorage where main app stores it, or use default
const getApiUrl = () => {
  // Try to get from localStorage (set by main app)
  const storedUrl = localStorage.getItem('apiUrl');
  if (storedUrl) {
    return storedUrl;
  }

  // Fallback to default (should match VITE_API_URL in .env)
  return 'https://localhost:7288/api';
};

// Export for use in test pages
window.APP_CONFIG = {
  API_URL: getApiUrl()
};

console.log('[CONFIG] API URL:', window.APP_CONFIG.API_URL);
