import { Establishment } from '../domain/model/establishment.entity.js';

/**
 * Maps Establishment infrastructure resources into domain entities.
 *
 * @class EstablishmentAssembler
 */
export class EstablishmentAssembler {
    static toEntityFromResource(resource) {
        return new Establishment({ ...resource });
    }

    static toEntitiesFromResponse(response) {
        if (response.status !== 200) {
            console.error(`${response.status} ${response.statusText}`);
            return [];
        }
        const resources = response.data instanceof Array
            ? response.data
            : response.data?.establishments ?? response.data?.data ?? [];
        return resources.map((resource) => this.toEntityFromResource(resource));
    }
}
