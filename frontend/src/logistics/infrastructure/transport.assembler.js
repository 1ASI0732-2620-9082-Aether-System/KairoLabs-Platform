import { Transport } from '../domain/model/transport.entity.js';

/**
 * Maps Logistics infrastructure transport resources into domain entities.
 *
 * @class TransportAssembler
 */
export class TransportAssembler {
    static toEntityFromResource(resource) {
        return new Transport({ ...resource });
    }

    static toEntitiesFromResponse(response) {
        if (response.status !== 200) {
            console.error(`${response.status} ${response.statusText}`);
            return [];
        }
        const resources = response.data instanceof Array
            ? response.data
            : response.data?.transports ?? response.data?.data ?? [];
        return resources.map((resource) => this.toEntityFromResource(resource));
    }
}
