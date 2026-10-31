import { BaseApi } from '../../shared/infrastructure/base-api.js';
import { BaseEndpoint } from '../../shared/infrastructure/base-endpoint.js';
import { isMockMode } from '../../shared/infrastructure/mocks/mock-config.js';
import { MockApi } from '../../shared/infrastructure/mocks/mock-api.service.js';

const subscriptionsEndpointPath =
    import.meta.env.VITE_SUBSCRIPTIONS_ENDPOINT_PATH || '/subscriptions';

/**
 * Infrastructure gateway for the Subscriptions bounded context.
 *
 * @class SubscriptionsApi
 * @extends BaseApi
 */
export class SubscriptionsApi extends BaseApi {
    #subscriptionsEndpoint;

    constructor() {
        super(import.meta.env.VITE_MEDITRACK_SENSOR_SUB_EST_API);
        this.#subscriptionsEndpoint = new BaseEndpoint(this, subscriptionsEndpointPath);
    }

    getSubscriptions() {
        if (isMockMode()) return MockApi.getSubscriptions();
        return this.#subscriptionsEndpoint.getAll();
    }

    getSubscriptionById(id) {
        if (isMockMode()) {
            return MockApi.getSubscriptions().then((res) => ({
                ...res,
                data: (res.data ?? []).find((s) => Number(s.id) === Number(id)) ?? null,
            }));
        }
        return this.#subscriptionsEndpoint.getById(id);
    }

    createSubscription(resource) {
        if (isMockMode()) {
            return Promise.resolve({
                status: 200,
                statusText: 'OK',
                data: { id: Date.now(), ...resource, status: 'ACTIVE' },
            });
        }
        const adminId = resource.admin_id;
        const { admin_id: _adminId, ...body } = resource;
        return this.http.post(`/admins/${adminId}/subscriptions`, body);
    }

    updateSubscription(resource) {
        if (isMockMode()) {
            return Promise.resolve({ status: 200, statusText: 'OK', data: resource });
        }
        return this.#subscriptionsEndpoint.update(resource.id, resource);
    }

    deleteSubscription(id) {
        if (isMockMode()) {
            return Promise.resolve({ status: 200, statusText: 'OK', data: null });
        }
        return this.#subscriptionsEndpoint.delete(id);
    }
}
