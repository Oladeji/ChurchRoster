import { defineConfig } from 'vite'
import type { Plugin } from 'vite'
import react from '@vitejs/plugin-react'
import { VitePWA } from 'vite-plugin-pwa'
import fs from 'fs'
import path from 'path'

// Plugin to inject .env values into Service Worker at build time.
// Accepts the resolved API URL so it correctly uses .env.production on Vercel.
function injectEnvToServiceWorker(apiUrl: string): Plugin {
  return {
    name: 'inject-env-to-sw',
    apply: 'build',
    closeBundle() {
      const publicDir = path.resolve(__dirname, 'public')
      const distDir = path.resolve(__dirname, 'dist')

      const filesToProcess = ['firebase-messaging-sw.js', 'config.js']

      filesToProcess.forEach(filename => {
        const sourcePath = path.join(publicDir, filename)
        const destPath = path.join(distDir, filename)

        if (fs.existsSync(sourcePath)) {
          let content = fs.readFileSync(sourcePath, 'utf-8')

          if (filename === 'firebase-messaging-sw.js') {
            content = content.replace(
              /let API_URL = ['"].*?['"];.*?\/\/ Fallback default/,
              `let API_URL = '${apiUrl}'; // Injected from .env VITE_API_URL at build time`
            )
          } else if (filename === 'config.js') {
            content = content.replace(
              /return ['"].*?['"];.*?\/\/ Fallback/i,
              `return '${apiUrl}'; // Injected from .env VITE_API_URL at build time`
            )
          }

          fs.writeFileSync(destPath, content, 'utf-8')
          console.log(`✅ ${filename} API URL injected: ${apiUrl}`)
        }
      })
    }
  }
}

// https://vite.dev/config/
export default defineConfig(() => {
  // Always use the value set in the Vercel dashboard (process.env).
  // Falls back to localhost only for local development when no OS env var is set.
  const apiUrl = process.env.VITE_API_URL ?? 'https://localhost:7288/api'

  return {
  plugins: [
    react(),
    VitePWA({
      strategies: 'generateSW',
      registerType: 'autoUpdate',
      injectRegister: null,
      manifest: false,
      devOptions: { enabled: false },
      workbox: {
        globPatterns: ['**/*.{js,css,html,ico,png,svg,woff2}'],
        globIgnores: ['firebase-messaging-sw.js'],
        swDest: 'dist/sw.js',
        navigateFallback: 'index.html',
      },
    }),
    injectEnvToServiceWorker(apiUrl)
  ],
  server: {
    port: 3000
  }
  }
})
