// frontend/vite.config.ts
// Configuración de Vite para DemoCopilot.
// El proxy redirige /api/* al backend ASP.NET Core.
//
// IMPORTANTE: usar https con secure:false cuando el backend corre con dev cert.
// Si se usa http, el backend responde con un redirect 307 y se pierden los headers.

import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'https://localhost:7279',  // puerto HTTPS del backend ASP.NET Core
        changeOrigin: true,
        secure: false,                     // acepta el certificado de desarrollo de .NET
      },
      '/scalar': {
        target: 'https://localhost:7279',  // también proxea Scalar API docs
        changeOrigin: true,
        secure: false,
      },
    },
  },
});
