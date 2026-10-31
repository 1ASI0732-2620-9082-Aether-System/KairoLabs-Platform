import { readAuthSession } from './auth-session.js';

/**
 * Navigation guard that protects routes flagged with `meta.requiresAuth`
 * for anonymous visitors, redirecting them to the login screen.
 *
 * @param {import('vue-router').RouteLocationNormalized} to - Target route.
 * @param {import('vue-router').RouteLocationNormalized} from - Current route.
 * @returns {boolean | { name: string }} `true` when navigation is allowed, otherwise a redirect target.
 */
export const authenticationGuard = (to, from) => {
    const requiresAuth = to.matched.some((record) => record.meta?.requiresAuth);
    const isAnonymous = !readAuthSession();
    if (requiresAuth && isAnonymous) {
        return { name: 'login' };
    }
    return true;
};
