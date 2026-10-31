import { SignInResource } from './sign-in.resource.js';

/**
 * Maps authentication endpoint responses into normalised IAM session objects.
 *
 * @class SignInAssembler
 */
export class SignInAssembler {
    /**
     * Parses a raw sign-in response into a {@link SignInResource}.
     * @param {import('axios').AxiosResponse<Object>} response - HTTP response from the sign-in endpoint.
     * @returns {SignInResource|null} Parsed resource when successful; otherwise null.
     */
    static toResourceFromResponse(response) {
        if (response.status !== 200) {
            console.error(`${response.status}, ${response.statusText}`);
            return null;
        }
        return new SignInResource(response.data);
    }

    /**
     * Builds a portal session from a sign-in response, validating that the
     * authenticated role matches the requested segment.
     *
     * @param {import('axios').AxiosResponse<Object>} response - HTTP response from the sign-in endpoint.
     * @param {'health-entity'|'operational-staff'} segment - Portal the user signed in through.
     * @returns {Object|null} Normalised session, or null when credentials/segment are invalid.
     */
    static toSessionFromResponse(response, segment) {
        const resource = this.toResourceFromResponse(response);
        if (!resource || !resource.token) return null;

        const { user, token } = resource;
        const backendRole = String(user.role ?? '').toLowerCase(); // 'admin' | 'operator'

        const inferredSegment =
            backendRole === 'admin'
                ? 'health-entity'
                : backendRole === 'operator'
                    ? 'operational-staff'
                    : null;
        if (!inferredSegment) return null;
        if (segment && segment !== inferredSegment) return null;

        return {
            userId: user.id,
            email: user.email,
            name: user.name,
            role: backendRole.toUpperCase(),
            segment: inferredSegment,
            token,
            adminId: null,
            entityCode: null,
            entityName: null,
            operatorId: null,
            establishmentId: null,
            notAssigned: true,
        };
    }
}
