const registerPlans = () => import('./views/register-plans.vue');
const billingCheckout = () => import('./views/billing-checkout.vue');

/** @type {import('vue-router').RouteRecordRaw[]} */
const iamAuthRoutes = [
    {
        path: 'login-health-entity',
        name: 'iam-login-health-entity',
        redirect: { name: 'login' },
        meta: { title: 'Login', requiresAuth: false },
    },
    {
        path: 'login-operational-staff',
        name: 'iam-login-operational-staff',
        redirect: { name: 'login' },
        meta: { title: 'Login', requiresAuth: false },
    },
    {
        path: 'register-health-entity',
        name: 'iam-register-health-entity',
        redirect: { name: 'iam-register', query: { role: 'health-entity' } },
        meta: { title: 'Register', requiresAuth: false },
    },
    {
        path: 'register-operational-staff',
        name: 'iam-register-operational-staff',
        redirect: { name: 'iam-register', query: { role: 'operational-staff' } },
        meta: { title: 'Register', requiresAuth: false },
    },
    {
        path: 'register-plans',
        name: 'iam-register-plans',
        component: registerPlans,
        meta: { title: 'Plans', requiresAuth: false },
    },
    {
        path: 'billing',
        name: 'iam-billing',
        component: billingCheckout,
        meta: { title: 'Payment', requiresAuth: false },
    },
];

export default iamAuthRoutes;
