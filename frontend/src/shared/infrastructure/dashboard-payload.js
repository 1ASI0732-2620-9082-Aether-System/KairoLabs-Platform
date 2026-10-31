/**
 * Composes dashboard payload from bounded-context application stores (presentation shell only).
 * Keeps shared/infrastructure free of bounded-context API configuration.
 *
 * @module use-dashboard-payload
 */
import useIamStore from '../../iam/application/iam.store.js';
import useSubscriptionsStore from '../../subscriptions/application/subscriptions.store.js';
import useEstablishmentStore from '../../establishment/application/establishment.store.js';
import useMonitoringStore from '../../monitoring/application/monitoring.store.js';
import useLogisticsStore from '../../logistics/application/logistics.store.js';

function entitiesToPlain(entities) {
    return (entities || []).map((e) => ({ ...e }));
}

/**
 * @returns {Promise<{ users: Object[], admins: Object[], subscriptions: Object[], establishments: Object[], devices: Object[], operators: Object[], transports: Object[] }>}
 */
export async function fetchDashboardPayload() {
    const iamStore = useIamStore();
    const subscriptionsStore = useSubscriptionsStore();
    const establishmentStore = useEstablishmentStore();
    const monitoringStore = useMonitoringStore();
    const logisticsStore = useLogisticsStore();

    const [users, admins, subscriptions, establishments, devices, operators, transports] =
        await Promise.all([
            iamStore.fetchUsersAsync(),
            iamStore.fetchAdminsAsync(),
            subscriptionsStore.fetchSubscriptionsAsync(),
            establishmentStore.fetchEstablishmentsAsync(),
            monitoringStore.fetchDevicesAsync(),
            establishmentStore.fetchOperatorsAsync(),
            logisticsStore.fetchTransportsAsync(),
        ]);

    return {
        users: entitiesToPlain(users),
        admins: entitiesToPlain(admins),
        subscriptions: entitiesToPlain(subscriptions),
        establishments: entitiesToPlain(establishments),
        devices: entitiesToPlain(devices),
        operators: entitiesToPlain(operators),
        transports: entitiesToPlain(transports),
    };
}
