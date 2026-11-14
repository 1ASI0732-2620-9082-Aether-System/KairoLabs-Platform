import { defineConfig } from 'vite';
import { fileURLToPath } from 'node:url';




// ConfiguraciÃ³n exclusiva de integraciÃ³n. No importa vite.config.js ni api.js.
const base = 'http://127.0.0.1:5187/api/v1';
export default defineConfig({
    define: Object.fromEntries(Object.entries({
            VITE_USE_MOCKS: 'false',
            VITE_API_BASE_URL: base,
            VITE_MEDITRACK_SENSOR_US_AD_API: base,
            VITE_MEDITRACK_SENSOR_DV_API: base,
        }).map(([key, value]) => [`import.meta.env.${key}`, JSON.stringify(value)])),
    build: {
        outDir: '.integration',
        emptyOutDir: true,
        lib: {
            entry: fileURLToPath(new URL('./integration-entry.js', import.meta.url)),
            formats: ['es'],
            fileName: () => 'gateways.mjs',
        },
        rollupOptions: { external: ['axios'] },
        minify: false,
    },
});
