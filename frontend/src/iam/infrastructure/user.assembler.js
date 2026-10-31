import { User } from '../domain/model/user.entity.js';

/**
 * Maps IAM infrastructure user resources into domain entities.
 *
 * @class UserAssembler
 */
export class UserAssembler {
    /**
     * @param {Object} resource - User resource payload from the API.
     * @returns {User} Mapped user entity.
     */
    static toEntityFromResource(resource) {
        const password = resource.password ?? resource.passsword ?? '';
        return new User({ ...resource, password });
    }

    /**
     * @param {import('axios').AxiosResponse<Array<Object>|Object>} response - HTTP response.
     * @returns {User[]} Collection of user entities.
     */
    static toEntitiesFromResponse(response) {
        if (response.status !== 200) {
            console.error(`${response.status} ${response.statusText}`);
            return [];
        }
        const resources = response.data instanceof Array
            ? response.data
            : response.data?.users ?? response.data?.data ?? [];
        return resources.map((resource) => this.toEntityFromResource(resource));
    }
}
