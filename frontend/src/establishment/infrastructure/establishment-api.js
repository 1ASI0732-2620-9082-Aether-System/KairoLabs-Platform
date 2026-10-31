import { BaseApi } from '../../shared/infrastructure/base-api.js';
import { BaseEndpoint } from '../../shared/infrastructure/base-endpoint.js';
import { isMockMode } from '../../shared/infrastructure/mocks/mock-config.js';
import { MockApi } from '../../shared/infrastructure/mocks/mock-api.service.js';

const establishmentsEndpointPath =
    import.meta.env.VITE_ESTABLISHMENT_ENDPOINT_PATH || '/establishments';
const operatorsEndpointPath = import.meta.env.VITE_OPERATOR_ENDPOINT_PATH || '/operators';

/**
 * Infrastructure gateway for the Establishment bounded context.
 *
 * @class EstablishmentApi
 * @extends BaseApi
 */
export class EstablishmentApi extends BaseApi {
    #establishmentsEndpoint;
    #operatorsEndpoint;

    constructor() {
        super(import.meta.env.VITE_MEDITRACK_SENSOR_SUB_EST_API);
        this.#establishmentsEndpoint = new BaseEndpoint(this, establishmentsEndpointPath);
        this.#operatorsEndpoint = new BaseEndpoint(this, operatorsEndpointPath);
    }

    getEstablishments() {
        if (isMockMode()) return MockApi.getEstablishments();
        return this.#establishmentsEndpoint.getAll();
    }

    getEstablishmentById(id) {
        if (isMockMode()) {
            return MockApi.getEstablishments().then((res) => ({
                ...res,
                data: (res.data ?? []).find((e) => Number(e.id) === Number(id)) ?? null,
            }));
        }
        return this.#establishmentsEndpoint.getById(id);
    }

    createEstablishment(resource) {
        if (isMockMode()) return MockApi.createEstablishment(resource);
        const adminId = resource.admin_id;
        const { admin_id: _adminId, ...body } = resource;
        return this.http.post(`/admins/${adminId}/establishments`, body);
    }

    updateEstablishment(resource) {
        return this.#establishmentsEndpoint.update(resource.id, resource);
    }

    deleteEstablishment(id) {
        return this.#establishmentsEndpoint.delete(id);
    }

    getOperators() {
        if (isMockMode()) return MockApi.getOperators();
        return this.#operatorsEndpoint.getAll();
    }

    getOperatorById(id) {
        if (isMockMode()) {
            return MockApi.getOperators().then((res) => ({
                ...res,
                data: (res.data ?? []).find((o) => Number(o.id) === Number(id)) ?? null,
            }));
        }
        return this.#operatorsEndpoint.getById(id);
    }

    createOperator(resource) {
        if (isMockMode()) return MockApi.createOperator(resource);
        const establishmentId = resource.establishment_id;
        const { establishment_id: _estId, alerts_answered: _alerts, ...body } = resource;
        return this.http.post(`/establishments/${establishmentId}/operators`, {
            schedule: body.schedule,
            users_id: body.users_id,
        });
    }

    updateOperator(resource) {
        if (isMockMode()) return MockApi.updateOperator(resource.id, resource);
        const establishmentId = resource.establishment_id;
        return this.http.put(`/establishments/${establishmentId}/operators/${resource.id}`, {
            schedule: resource.schedule,
        });
    }

    incrementOperatorAlert(id) {
        return this.#operatorsEndpoint.http.put(`${operatorsEndpointPath}/${id}/alert-answered`);
    }

    deleteOperator(id) {
        if (isMockMode()) return MockApi.deleteOperator(id);
        return this.#operatorsEndpoint.delete(id);
    }
}
