// Lazy-loaded components
const assignOperator = () => import('./views/assign-operator.vue');
const establishmentForm = () => import('./views/establishment-form.vue');
const mapOfEstablishments = () => import('./views/map-of-establishments.vue');
const viewEstablishments = () => import('./views/view-establishments.vue');
const viewOperators = () => import('./views/view-operators.vue');
const viewOperatorDetail = () => import('./views/view-operator-detail.vue');
const viewEstablishmentDetail = () => import('./views/view-establishment-detail.vue');
const viewEstablishmentTeam = () => import('./views/view-establishment-team.vue');
const viewEstablishmentOperatorDetail = () => import('./views/view-establishment-operator-detail.vue');
const viewEstablishmentDeviceDetail = () => import('./views/view-establishment-device-detail.vue');

/**
 * Route definitions for the Establishment bounded context.
 * All paths are relative to the `/establishment` parent route.
 *
 * @type {import('vue-router').RouteRecordRaw[]}
 */
const establishmentRoutes = [
    {
        path: 'establishments/new',
        name: 'establishment-new',
        component: establishmentForm,
        meta: { title: 'New Establishment' }
    },
    {
        path: 'establishments/:establishmentId/operators/:operatorId',
        name: 'establishment-operator-detail',
        component: viewEstablishmentOperatorDetail,
        meta: { title: 'Operator detail' }
    },
    {
        path: 'establishments/:establishmentId/devices/:deviceId',
        name: 'establishment-device-detail',
        component: viewEstablishmentDeviceDetail,
        meta: { title: 'Device detail' }
    },
    {
        path: 'establishments/:establishmentId/team',
        name: 'establishment-team',
        component: viewEstablishmentTeam,
        meta: { title: 'Establishment team' }
    },
    {
        path: 'establishments/:establishmentId',
        name: 'establishment-detail',
        component: viewEstablishmentDetail,
        meta: { title: 'Establishment detail' }
    },
    {
        path: 'establishments',
        name: 'establishments',
        component: viewEstablishments,
        meta: { title: 'Establishments' }
    },
    {
        path: 'assign-operator',
        name: 'assign-operator',
        component: assignOperator,
        meta: { title: 'Assign Operator' }
    },
    {
        path: 'map',
        name: 'establishment-map',
        component: mapOfEstablishments,
        meta: { title: 'Map of Establishments' }
    },
    {
        path: 'operators/:operatorId',
        name: 'operator-profile',
        component: viewOperatorDetail,
        meta: { title: 'Operator detail' },
    },
    {
        path: 'operators',
        name: 'operators',
        component: viewOperators,
        meta: { title: 'Operators' },
    },
];

export default establishmentRoutes;