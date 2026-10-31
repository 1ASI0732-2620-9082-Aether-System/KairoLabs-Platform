import { AUTH_SESSION_KEY } from './auth-session.js';

/**
 * Adds the IAM bearer token to outbound requests when a session is active.
 * The token is read from the persisted auth session so every bounded-context
 * gateway that extends {@link BaseApi} is authenticated automatically.
 *
 * @param {import('axios').InternalAxiosRequestConfig} config - Axios request configuration.
 * @returns {import('axios').InternalAxiosRequestConfig} Updated request configuration.
 */
export const iamInterceptor = (config) => {
    try {
        const raw = sessionStorage.getItem(AUTH_SESSION_KEY);
        const session = raw ? JSON.parse(raw) : null;
        if (session?.token) {
            config.headers['Authorization'] = `Bearer ${session.token}`;
        }
    } catch {
        // ignore parse errors — request proceeds unauthenticated
    }
    return config;
};
