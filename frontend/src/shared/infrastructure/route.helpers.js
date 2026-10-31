import { readAuthSession } from '../../iam/infrastructure/auth-session.js';

/**
 * Resolves the authenticated user's profile route params.
 * @returns {{ name: string, params: { profileId: string } }}
 */
export function getProfileRoute() {
    const session = readAuthSession();
    const profileId = session?.userId ?? session?.id ?? 'me';
    return { name: 'profile', params: { profileId: String(profileId) } };
}
