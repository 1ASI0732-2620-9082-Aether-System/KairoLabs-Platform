import { BaseApi } from '../../shared/infrastructure/base-api.js';
import { BaseEndpoint } from '../../shared/infrastructure/base-endpoint.js';
import { isMockMode } from '../../shared/infrastructure/mocks/mock-config.js';
import { MockApi } from '../../shared/infrastructure/mocks/mock-api.service.js';

const transportsEndpointPath =
    import.meta.env.VITE_LOGISTICS_ENDPOINT_PATH || '/transports';

/**
 * Infrastructure gateway for the Logistics bounded context.
 *
 * @class LogisticsApi
 * @extends BaseApi
 */
export class LogisticsApi extends BaseApi {
    #transportsEndpoint;

    constructor() {
        super(import.meta.env.VITE_MEDITRACK_SENSOR_OP_TR_API);
        this.#transportsEndpoint = new BaseEndpoint(this, transportsEndpointPath);
    }

    getTransports() {
        if (isMockMode()) return MockApi.getTransports();
        return this.#transportsEndpoint.getAll();
    }

    getTransportById(id) {
        if (isMockMode()) {
            return MockApi.getTransports().then((res) => ({
                ...res,
                data: (res.data ?? []).find((t) => Number(t.id) === Number(id)) ?? null,
            }));
        }
        return this.#transportsEndpoint.getById(id);
    }

    createTransport(resource) {
        if (isMockMode()) return MockApi.createTransport(resource);
        const establishmentId = resource.establishment_id;
        const { establishment_id: _estId, ...body } = resource;
        return this.http.post(`/establishments/${establishmentId}/transports`, body);
    }

    updateTransport(resource) {
        return this.#transportsEndpoint.update(resource.id, resource);
    }

    updateSensorData(id, sensorData, establishmentId) {
        if (isMockMode()) {
            return Promise.resolve({ status: 200, statusText: 'OK', data: { id, ...sensorData } });
        }
        if (establishmentId) {
            return this.http.put(`/establishments/${establishmentId}/transports/${id}/sensor-data`, sensorData);
        }
        return this.#transportsEndpoint.http.put(`${transportsEndpointPath}/${id}/sensor-data`, sensorData);
    }

    deleteTransport(id, establishmentId) {
        if (isMockMode()) {
            return Promise.resolve({ status: 200, statusText: 'OK', data: null });
        }
        if (establishmentId) {
            return this.http.delete(`/establishments/${establishmentId}/transports/${id}`);
        }
        return this.#transportsEndpoint.delete(id);
    }
}
