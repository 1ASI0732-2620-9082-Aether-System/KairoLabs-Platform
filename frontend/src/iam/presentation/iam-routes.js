// Lazy-loaded components
const profile = () => import('./views/profile.vue');
const notAssigned = () => import('./views/not-assigned-establishment.vue');
const homeHealthEntity = () => import('../../shared/presentation/views/home-health-entity.vue');
const homeOperationalStaff = () => import('../../shared/presentation/views/home-operational-staff.vue');

/**
 * Route definitions for the IAM bounded context (authenticated area).
 * All paths are relative to the `/iam` parent route.
 *
 * @type {import('vue-router').RouteRecordRaw[]}
 */
const iamRoutes = [
    {
        path: 'home/health-entity',
        name: 'home-health-entity',
        component: homeHealthEntity,
        meta: { title: 'Home', requiresAuth: true },
    },
    {
        path: 'home/operational-staff',
        name: 'home-operational-staff',
        component: homeOperationalStaff,
        meta: { title: 'Home', requiresAuth: true },
    },
    {
        path: 'profile/:profileId',
        name: 'profile',
        component: profile,
        meta: { title: 'Profile', requiresAuth: true },
    },
    {
        path: 'not-assigned',
        name: 'iam-not-assigned',
        component: notAssigned,
        meta: { title: 'Not assigned', requiresAuth: true },
    },
];

export default iamRoutes;