/**
 * Admin profile entity within the IAM bounded context.
 * Links a user to a health entity (organization).
 *
 * @class Admin
 */
export class Admin {
    /**
     * @param {Object} params - Entity attributes.
     * @param {number|null} [params.id=null] - Admin record identifier.
     * @param {string} [params.entity_name=''] - Organization display name.
     * @param {string} [params.entity_code=''] - Organization code.
     * @param {string} [params.schedule=''] - Operating schedule.
     * @param {number|null} [params.users_id=null] - Linked user identifier.
     */
    constructor({
        id = null,
        entity_name = '',
        entity_code = '',
        schedule = '',
        user_id = null,
        users_id = null,   // backward compat with mock data
    } = {}) {
        this.id = id;
        this.entity_name = entity_name;
        this.entity_code = entity_code;
        this.schedule = schedule;
        this.user_id = user_id ?? users_id;
    }
}
