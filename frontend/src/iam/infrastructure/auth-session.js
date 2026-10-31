/**
 * IAM authentication session (sessionStorage).
 * @module auth-session
 */

export const AUTH_SESSION_KEY = 'meditrack_auth_session';
export const PENDING_REGISTRATION_KEY = 'meditrack_pending_registration';
export const PENDING_PLAN_KEY = 'meditrack_pending_plan';

export function readAuthSession() {
    try {
        const raw = sessionStorage.getItem(AUTH_SESSION_KEY);
        return raw ? JSON.parse(raw) : null;
    } catch {
        return null;
    }
}

export function writeAuthSession(session) {
    if (!session) {
        sessionStorage.removeItem(AUTH_SESSION_KEY);
        return;
    }
    sessionStorage.setItem(AUTH_SESSION_KEY, JSON.stringify(session));
}

export function readPendingRegistration() {
    try {
        const raw = sessionStorage.getItem(PENDING_REGISTRATION_KEY);
        return raw ? JSON.parse(raw) : null;
    } catch {
        return null;
    }
}

export function writePendingRegistration(data) {
    if (!data) {
        sessionStorage.removeItem(PENDING_REGISTRATION_KEY);
        return;
    }
    sessionStorage.setItem(PENDING_REGISTRATION_KEY, JSON.stringify(data));
}

export function readPendingPlan() {
    try {
        const raw = sessionStorage.getItem(PENDING_PLAN_KEY);
        return raw ? JSON.parse(raw) : null;
    } catch {
        return null;
    }
}

export function writePendingPlan(plan) {
    if (!plan) {
        sessionStorage.removeItem(PENDING_PLAN_KEY);
        return;
    }
    sessionStorage.setItem(PENDING_PLAN_KEY, JSON.stringify(plan));
}

export function clearRegistrationFlow() {
    sessionStorage.removeItem(PENDING_REGISTRATION_KEY);
    sessionStorage.removeItem(PENDING_PLAN_KEY);
}
