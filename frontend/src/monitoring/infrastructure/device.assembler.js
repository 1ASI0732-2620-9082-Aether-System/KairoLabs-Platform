import { Device } from '../domain/model/device.entity.js';

/**
 * Maps Monitoring infrastructure device resources into domain entities.
 *
 * @class DeviceAssembler
 */
export class DeviceAssembler {
    static toEntityFromResource(resource) {
        return new Device({ ...resource });
    }

    static toEntitiesFromResponse(response) {
        if (response.status !== 200) {
            console.error(`${response.status} ${response.statusText}`);
            return [];
        }
        const resources = response.data instanceof Array
            ? response.data
            : response.data?.devices ?? response.data?.data ?? [];
        return resources.map((resource) => this.toEntityFromResource(resource));
    }
}
