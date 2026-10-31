/**
 * Infrastructure resource returned by the authentication endpoint.
 * Wraps the raw `{ user, token }` payload of a successful sign-in.
 *
 * @class SignInResource
 */
export class SignInResource {
    /**
     * @param {Object} params - Resource payload.
     * @param {Object} params.user - Authenticated user record.
     * @param {string} params.token - Bearer token.
     */
    constructor({ user, token }) {
        this.user = user ?? {};
        this.token = token ?? null;
    }
}
