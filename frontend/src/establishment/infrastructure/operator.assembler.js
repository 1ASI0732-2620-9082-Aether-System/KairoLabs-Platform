import { Operator } from '../domain/model/operator.entity.js';

/**
 * Maps operator infrastructure resources into domain entities.
 *
 * @class OperatorAssembler
 */
export class OperatorAssembler {
    static toEntityFromResource(resource) {
        return new Operator({ ...resource });
    }

    static toEntitiesFromResponse(response) {
        if (response.status !== 200) {
            console.error(`${response.status} ${response.statusText}`);
            return [];
        }
        const resources = response.data instanceof Array
            ? response.data
            : response.data?.operators ?? response.data?.data ?? [];
        return resources.map((resource) => this.toEntityFromResource(resource));
    }
}
