import { createApp } from 'vue'
import './style.css'
import App from './app.vue'
import PrimeVue from 'primevue/config';
import { definePreset } from '@primeuix/themes';
import Material from '@primeuix/themes/material';
import 'primeflex/primeflex.css';
import 'primeicons/primeicons.css';
import {
    Button,
    Card,
    Checkbox,
    Column,
    ConfirmationService,
    ConfirmDialog, DataTable, Dialog,
    DialogService,
    Drawer, FileUpload, FloatLabel, IconField, InputIcon, InputNumber, Menu, Rating, Row, Select, SelectButton, Tag,
    Textarea, Toast,
    ToastService, Toolbar, Tooltip
} from "primevue";
import i18n from "./i18n.js";
import router from "./router.js";
import pinia from "./pinia.js";

const KairoLabsPreset = definePreset(Material, {
    semantic: {
        primary: {
            50: '#fff4ed',
            100: '#ffe4d4',
            200: '#ffc4a8',
            300: '#ff9d71',
            400: '#ff753d',
            500: '#F37021',
            600: '#e05a12',
            700: '#ba4510',
            800: '#943814',
            900: '#783114',
            950: '#431507',
        },
        colorScheme: {
            light: {
                surface: {
                    0: '#ffffff',
                    50: '#f8fafc',
                    100: '#f1f5f9',
                    200: '#e2e8f0',
                    300: '#cbd5e1',
                    400: '#94a3b8',
                    500: '#64748b',
                    600: '#475569',
                    700: '#334155',
                    800: '#1e293b',
                    900: '#112433',
                    950: '#0c1a24',
                },
            },
        },
    },
});

createApp(App)
    .use(i18n)
    .use(router)
    .use(pinia)
    .use(PrimeVue, {
        theme: {
            preset: KairoLabsPreset,
            options: {
                // App is always light; system dark mode was making Select overlays black
                darkModeSelector: false,
            },
        },
        ripple: true,
    })
    .use(ConfirmationService)
    .use(DialogService)
    .use(ToastService)
    .component('pv-button', Button)
    .component('pv-card', Card)
    .component('pv-column', Column)
    .component('pv-confirm-dialog', ConfirmDialog)
    .component('pv-checkbox', Checkbox)
    .component('pv-data-table', DataTable)
    .component('pv-dialog', Dialog)
    .component('pv-drawer', Drawer)
    .component('pv-file-upload', FileUpload)
    .component('pv-float-label', FloatLabel)
    .component('pv-icon-field', IconField)
    .component('pv-input-icon', InputIcon)
    .component('pv-input-number', InputNumber)
    .component('pv-menu', Menu)
    .component('pv-rating', Rating)
    .component('pv-row', Row)
    .component('pv-select', Select)
    .component('pv-select-button', SelectButton)
    .component('pv-tag', Tag)
    .component('pv-textarea', Textarea)
    .component('pv-toolbar', Toolbar)
    .component('pv-toast', Toast)
    .directive('tooltip', Tooltip)
    .mount('#app')