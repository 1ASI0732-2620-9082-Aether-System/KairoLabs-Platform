import { BaseApi } from '../../shared/infrastructure/base-api.js';
import { BaseEndpoint } from '../../shared/infrastructure/base-endpoint.js';
import { isMockMode } from '../../shared/infrastructure/mocks/mock-config.js';
import { MockApi } from '../../shared/infrastructure/mocks/mock-api.service.js';

const devicesEndpointPath =
    import.meta.env.VITE_MONITORING_ENDPOINT_PATH || '/devices';

/**
 * Infrastructure gateway for the Monitoring bounded context.
 *
 * @class MonitoringApi
 * @extends BaseApi
 */
export class MonitoringApi extends BaseApi {
    #devicesEndpoint;

    constructor() {
        super(import.meta.env.VITE_MEDITRACK_SENSOR_DV_API);
        this.#devicesEndpoint = new BaseEndpoint(this, devicesEndpointPath);
    }

    getDevices() {
        if (isMockMode()) return MockApi.getDevices();
        return this.#devicesEndpoint.getAll();
    }

    getDeviceById(id) {
        if (isMockMode()) {
            return MockApi.getDevices().then((res) => ({
                ...res,
                data: (res.data ?? []).find((d) => Number(d.id) === Number(id)) ?? null,
            }));
        }
        return this.#devicesEndpoint.getById(id);
    }

    createDevice(resource) {
        if (isMockMode()) return MockApi.createDevice(resource);
        const establishmentId = resource.establishment_id;
        const { establishment_id: _estId, ...body } = resource;
        return this.http.post(`/establishments/${establishmentId}/devices`, body);
    }

    updateDevice(resource) {
        return this.#devicesEndpoint.update(resource.id, resource);
    }

    updateSensorData(id, sensorData, establishmentId) {
        if (isMockMode()) {
            return Promise.resolve({ status: 200, statusText: 'OK', data: { id, ...sensorData } });
        }
        if (establishmentId) {
            return this.http.put(`/establishments/${establishmentId}/devices/${id}/sensor-data`, sensorData);
        }
        return this.#devicesEndpoint.http.put(`${devicesEndpointPath}/${id}/sensor-data`, sensorData);
    }

    deleteDevice(id, establishmentId) {
        if (isMockMode()) {
            return Promise.resolve({ status: 200, statusText: 'OK', data: null });
        }
        if (establishmentId) {
            return this.http.delete(`/establishments/${establishmentId}/devices/${id}`);
        }
        return this.#devicesEndpoint.delete(id);
    }
}
