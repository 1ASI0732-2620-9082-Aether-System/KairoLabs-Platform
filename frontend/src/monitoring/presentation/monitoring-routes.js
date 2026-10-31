// Lazy-loaded components
const viewDevices = () => import('./views/view-devices.vue');
const deviceForm = () => import('./views/device-form.vue');
const viewDeviceDetail = () => import('./views/view-device-detail.vue');
const controlCenter = () => import('./views/monitoring-dashboard.vue');

/**
 * Route definitions for the Monitoring bounded context.
 * All paths are relative to the `/monitoring` parent route.
 *
 * @type {import('vue-router').RouteRecordRaw[]}
 */
const monitoringRoutes = [
    {
        path: 'devices/new',
        name: 'device-new',
        component: deviceForm,
        meta: { title: 'Add Device' },
    },
    {
        path: 'devices/:deviceId',
        name: 'device-detail',
        component: viewDeviceDetail,
        meta: { title: 'Device detail' },
    },
    {
        path: 'devices',
        name: 'devices',
        component: viewDevices,
        meta: { title: 'Devices' },
    },
    {
        path: 'control-center',
        name: 'control-center',
        component: controlCenter,
        meta: { title: 'Control Center' }
    }
];

export default monitoringRoutes;