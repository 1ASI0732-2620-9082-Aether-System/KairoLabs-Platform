/**
 * Command that carries the data required to register a health entity (admin)
 * account. The registration is completed later once a plan and payment are provided.
 *
 * @class RegisterHealthEntityCommand
 */
export class RegisterHealthEntityCommand {
    /**
     * @param {Object} params - Command attributes.
     * @param {string} params.name - Administrator full name.
     * @param {string} params.email - Account email.
     * @param {string} params.password - Account password.
     * @param {string} params.entityName - Health entity (organization) display name.
     */
    constructor({ name, email, password, entityName }) {
        this.name = name;
        this.email = email;
        this.password = password;
        this.entityName = entityName;
    }

    /** @returns {boolean} Whether all required fields are present. */
    isComplete() {
        return Boolean(this.name && this.email && this.password && this.entityName);
    }
}
