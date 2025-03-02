import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-react';

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [plugin()],
    server: {
        port: 53121,
        proxy: {
            '^/api': {
                target: 'http://localhost:5089',
                changeOrigin: true,
                secure: false,
            }
        }
    }
})
