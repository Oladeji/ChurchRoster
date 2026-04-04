import { defineConfig } from 'vite'
import type { Plugin } from 'vite'
import react from '@vitejs/plugin-react'
import fs from 'fs'
import path from 'path'

// Plugin to inject .env values into Service Worker at build time
function injectEnvToServiceWorker(): Plugin {
  return {
    name: 'inject-env-to-sw',
    apply: 'build', // Only run during build
    closeBundle() {
      const publicDir = path.resolve(__dirname, 'public')
      const distDir = path.resolve(__dirname, 'dist')

      // Files to copy and inject
      const filesToProcess = [
        'firebase-messaging-sw.js',
        'config.js'
      ]

      filesToProcess.forEach(filename => {
        const sourcePath = path.join(publicDir, filename)
        const destPath = path.join(distDir, filename)

        if (fs.existsSync(sourcePath)) {
          let content = fs.readFileSync(sourcePath, 'utf-8')

          // Get API URL from environment
          const apiUrl = process.env.VITE_API_URL || 'https://localhost:7288/api'

          if (filename === 'firebase-messaging-sw.js') {
            // Replace the API_URL placeholder
            content = content.replace(
              /let API_URL = ['"].*?['"];.*?\/\/ Fallback default/,
              `let API_URL = '${apiUrl}'; // Injected from .env VITE_API_URL at build time`
            )
          } else if (filename === 'config.js') {
            // Replace the fallback in config.js
            content = content.replace(
              /return ['"].*?['"];.*?\/\/ Fallback/i,
              `return '${apiUrl}'; // Injected from .env VITE_API_URL at build time`
            )
          }

          // Copy to dist with injected values
          fs.writeFileSync(destPath, content, 'utf-8')
          console.log(`✅ ${filename} copied and API URL injected: ${apiUrl}`)
        }
      })
    }
  }
}

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    react(),
    injectEnvToServiceWorker()
  ],
  server: {
    port: 3000
  }
})
