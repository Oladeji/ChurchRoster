import { defineConfig, minimalPreset } from '@vite-pwa/assets-generator/config';

export default defineConfig({
  preset: {
    ...minimalPreset,
    apple: {
      sizes: [180],
      padding: 0.2,
      resizeOptions: { background: '#4F46E5', fit: 'contain' },
    },
    maskable: {
      sizes: [512],
      padding: 0.2,
      resizeOptions: { background: '#4F46E5', fit: 'contain' },
    },
    transparent: {
      sizes: [192, 512],
      padding: 0.05,
      resizeOptions: { fit: 'contain' },
    },
  },
  images: ['public/favicon.svg'],
});
