// Lazy-loaded components
const plansSelection = () => import('./views/plans-selection.vue');

/**
 * Route definitions for the Subscriptions bounded context.
 * All paths are relative to the `/subscriptions` parent route.
 *
 * @type {import('vue-router').RouteRecordRaw[]}
 */
const subscriptionsRoutes = [
    {
        path: 'plans',
        name: 'plans',
        component: plansSelection,
        meta: { title: 'Plans' }
    }
];

export default subscriptionsRoutes;