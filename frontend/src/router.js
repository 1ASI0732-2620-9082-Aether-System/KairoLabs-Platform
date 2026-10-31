import { createRouter, createWebHistory } from "vue-router";
import Layout from "./shared/presentation/components/layout.vue";
import Login from "./iam/presentation/views/login.vue";
import Home from "./shared/presentation/views/home.vue";
import establishmentRoutes from "./establishment/presentation/establishment-routes.js";
import monitoringRoutes from "./monitoring/presentation/monitoring-routes.js";
import logisticsRoutes from "./logistics/presentation/logistics-routes.js";
import subscriptionsRoutes from "./subscriptions/presentation/subscriptions-routes.js";
import iamRoutes from "./iam/presentation/iam-routes.js";
import iamAuthRoutes from "./iam/presentation/iam-auth-routes.js";
import { authenticationGuard } from "./iam/infrastructure/authentication.guard.js";

const pageNotFound = () => import('./shared/presentation/views/page-not-found.vue');
const iamAuthLayout = () => import('./iam/presentation/views/auth-layout.vue');
const authSessionLayout = () => import('./iam/presentation/views/auth-session-layout.vue');
const register = () => import('./iam/presentation/views/register.vue');

const routes = [
    {
        path: '/',
        component: authSessionLayout,
        meta: { requiresAuth: false },
        children: [
            {
                path: '',
                redirect: { name: 'login' },
            },
            {
                path: 'login',
                name: 'login',
                component: Login,
                meta: { title: 'Login', requiresAuth: false },
            },
            {
                path: 'iam/auth/register',
                name: 'iam-register',
                component: register,
                meta: { title: 'Register', requiresAuth: false },
            },
        ],
    },
    {
        path: '/iam/auth',
        component: iamAuthLayout,
        children: iamAuthRoutes,
        meta: { requiresAuth: false }
    },
    {
        path: '/',
        component: Layout,
        children: [
            { path: 'home', name: 'home', component: Home, meta: { title: 'Home' } },
            { path: 'home-health-entity', redirect: { name: 'home-health-entity' } },
            { path: 'home-operational-staff', redirect: { name: 'home-operational-staff' } },
            { path: 'profile', redirect: () => {
                try {
                    const raw = sessionStorage.getItem('meditrack_auth_session');
                    const session = raw ? JSON.parse(raw) : null;
                    const profileId = session?.userId ?? session?.id ?? 'me';
                    return { name: 'profile', params: { profileId: String(profileId) } };
                } catch {
                    return { name: 'profile', params: { profileId: 'me' } };
                }
            }},
            { path: 'establishment', name: 'establishment', children: establishmentRoutes },
            { path: 'monitoring', name: 'monitoring', children: monitoringRoutes },
            { path: 'logistics', name: 'logistics', children: logisticsRoutes },
            { path: 'subscriptions', name: 'subscriptions', children: subscriptionsRoutes },
            { path: 'iam', name: 'iam', children: iamRoutes },
            { path: '', redirect: '/login' }
        ]
    },
    { path: '/:pathMatch(.*)*', name: 'not-found', component: pageNotFound, meta: { title: 'Page Not Found' } }
];

const router = createRouter({
    history: createWebHistory(import.meta.env.BASE_URL),
    routes: routes,
});

router.beforeEach((to, from) => {
    let baseTitle = 'KairoLabs';
    document.title = `${baseTitle} - ${to.meta['title']}`;
    return authenticationGuard(to, from);
});

export default router;