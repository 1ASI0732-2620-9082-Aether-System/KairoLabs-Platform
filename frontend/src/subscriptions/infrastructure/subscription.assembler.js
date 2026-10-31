import { Subscription } from '../domain/model/subscription.entity.js';

/**
 * Maps Subscriptions infrastructure resources into domain entities.
 *
 * @class SubscriptionAssembler
 */
export class SubscriptionAssembler {
    /**
     * @param {Object} resource
     * @returns {Subscription}
     */
    static toEntityFromResource(resource) {
        return new Subscription({ ...resource });
    }

    /**
     * @param {import('axios').AxiosResponse<Array<Object>|Object>} response
     * @returns {Subscription[]}
     */
    static toEntitiesFromResponse(response) {
        if (response.status !== 200) {
            console.error(`${response.status} ${response.statusText}`);
            return [];
        }
        const resources = response.data instanceof Array
            ? response.data
            : response.data?.subscriptions ?? response.data?.data ?? [];
        return resources.map((resource) => this.toEntityFromResource(resource));
    }
}
