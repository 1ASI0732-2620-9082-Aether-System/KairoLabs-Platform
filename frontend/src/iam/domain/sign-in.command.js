/**
 * Command used by the IAM application layer to request authentication for a
 * given portal segment (health entity admin or operational staff).
 *
 * @class SignInCommand
 */
export class SignInCommand {
    /**
     * @param {Object} params - Command attributes.
     * @param {string} params.email - Email credential.
     * @param {string} params.password - Password credential.
     * @param {'health-entity'|'operational-staff'} params.segment - Portal the user signs in through.
     */
    constructor({ email, password, segment }) {
        this.email = String(email ?? '').trim().toLowerCase();
        this.password = password;
        this.segment = segment;
    }
}
