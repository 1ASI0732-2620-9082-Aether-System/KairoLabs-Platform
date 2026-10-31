/**
 * Command that carries the data required to register an operational staff
 * (operator) account bound to an existing health entity code.
 *
 * @class RegisterOperationalStaffCommand
 */
export class RegisterOperationalStaffCommand {
    /**
     * @param {Object} params - Command attributes.
     * @param {string} params.name - Operator full name.
     * @param {string} params.email - Account email.
     * @param {string} params.password - Account password.
     * @param {string} params.entityCode - Health entity code the operator joins.
     * @param {string} [params.dni=''] - National ID document number.
     * @param {string} [params.phone=''] - Contact phone.
     * @param {string} [params.jobTitle=''] - Job title.
     */
    constructor({ name, email, password, entityCode, dni = '', phone = '', jobTitle = '' }) {
        this.name = name;
        this.email = email;
        this.password = password;
        this.entityCode = String(entityCode ?? '').trim().toUpperCase();
        this.dni = dni;
        this.phone = phone;
        this.jobTitle = jobTitle;
    }
}
