/**
 * User value-carrying entity within the IAM bounded context.
 * Represents an authenticated platform user (admin or operator).
 *
 * @class User
 */
export class User {
    /**
     * @param {Object} params - Entity attributes.
     * @param {number|null} [params.id=null] - Unique user identifier.
     * @param {string} [params.name=''] - Full name.
     * @param {string} [params.dni=''] - National ID document number.
     * @param {string} [params.email=''] - Email address.
     * @param {string} [params.phone=''] - Contact phone.
     * @param {string} [params.job_title=''] - Job title.
     * @param {string} [params.entry_date=''] - Employment entry date.
     * @param {string} [params.role=''] - Role (ADMIN | OPERATOR).
     * @param {string} [params.password=''] - Hashed password (never exposed in UI).
     * @param {string} [params.photo=''] - Avatar URL.
     * @param {string} [params.created_at=''] - Record creation timestamp.
     * @param {string} [params.entity_code=''] - Linked health entity code (admins / verified operators).
     */
    constructor({
        id = null,
        name = '',
        dni = '',
        email = '',
        phone = '',
        job_title = '',
        entry_date = '',
        role = '',
        password = '',
        photo = '',
        created_at = '',
        entity_code = '',
    } = {}) {
        this.id = id;
        this.name = name;
        this.dni = dni;
        this.email = email;
        this.phone = phone;
        this.job_title = job_title;
        this.entry_date = entry_date;
        this.role = role;
        this.password = password;
        this.photo = photo;
        this.created_at = created_at;
        this.entity_code = entity_code;
    }
}
