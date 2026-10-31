import { Admin } from '../domain/model/admin.entity.js';

/**
 * Maps IAM infrastructure admin resources into domain entities.
 *
 * @class AdminAssembler
 */
export class AdminAssembler {
    /**
     * @param {Object} resource - Admin resource payload from the API.
     * @returns {Admin} Mapped admin entity.
     */
    static toEntityFromResource(resource) {
        return new Admin({ ...resource });
    }

    /**
     * @param {import('axios').AxiosResponse<Array<Object>|Object>} response - HTTP response.
     * @returns {Admin[]} Collection of admin entities.
     */
    static toEntitiesFromResponse(response) {
        if (response.status !== 200) {
            console.error(`${response.status} ${response.statusText}`);
            return [];
        }
        const resources = response.data instanceof Array
            ? response.data
            : response.data?.admins ?? response.data?.data ?? [];
        return resources.map((resource) => this.toEntityFromResource(resource));
    }
}
