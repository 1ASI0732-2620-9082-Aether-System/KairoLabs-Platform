// Lazy-loaded components
const viewTransports = () => import('./views/view-transports.vue');
const transportForm = () => import('./views/transport-form.vue');
const viewTransportDetail = () => import('./views/view-transport-detail.vue');

/**
 * Route definitions for the Logistics bounded context.
 * All paths are relative to the `/logistics` parent route.
 *
 * @type {import('vue-router').RouteRecordRaw[]}
 */
const logisticsRoutes = [
    {
        path: 'transports/new',
        name: 'transport-new',
        component: transportForm,
        meta: { title: 'Add Transport' },
    },
    {
        path: 'transports/:transportId',
        name: 'transport-detail',
        component: viewTransportDetail,
        meta: { title: 'Transport detail' },
    },
    {
        path: 'transports',
        name: 'transports',
        component: viewTransports,
        meta: { title: 'Transports' },
    },
];

export default logisticsRoutes;
