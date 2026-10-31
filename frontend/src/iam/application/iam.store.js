/**
 * Application service store for the IAM bounded context.
 * It coordinates authentication and registration use cases, exposes profile
 * state and keeps a UI-facing view of users and admins.
 *
 * @module useIamStore
 */
import { defineStore } from 'pinia';
import { computed, ref } from 'vue';
import { IamApi } from '../infrastructure/iam-api.js';
import { UserAssembler } from '../infrastructure/user.assembler.js';
import { AdminAssembler } from '../infrastructure/admin.assembler.js';
import { SignInAssembler } from '../infrastructure/sign-in.assembler.js';
import {
    readAuthSession,
    writeAuthSession,
    clearRegistrationFlow,
    readPendingRegistration,
    writePendingRegistration,
    readPendingPlan,
    writePendingPlan,
} from '../infrastructure/auth-session.js';
import { SignInCommand } from '../domain/sign-in.command.js';
import { RegisterHealthEntityCommand } from '../domain/register-health-entity.command.js';
import { RegisterOperationalStaffCommand } from '../domain/register-operational-staff.command.js';
import { Subscription } from '../../subscriptions/domain/model/subscription.entity.js';
import {
    validateDemoPayment,
    simulateDemoPaymentProcessing,
} from '../../subscriptions/domain/model/demo-payment.gateway.js';

const iamApi = new IamApi();

/**
 * Authenticates against the real backend and enriches the session with the
 * admin/operator profile linked to the user.
 * @param {SignInCommand} command - Sign-in command.
 * @returns {Promise<{ok: true, session: Object} | {ok: false, error: string}>}
 */
async function signInReal(command) {
    const response = await iamApi.signIn(command);
    const session = SignInAssembler.toSessionFromResponse(response, command.segment);
    if (!session) return { ok: false, error: 'invalidCredentials' };

    // Persist the base session so the gateway interceptor authenticates enrichment calls.
    writeAuthSession(session);

    if (session.role === 'ADMIN') {
        try {
            const admins = (await iamApi.getAdmins()).data ?? [];
            const admin = admins.find((a) => Number(a.user_id ?? a.users_id) === Number(session.userId));
            if (admin) {
                session.adminId = admin.id;
                session.entityCode = admin.entity_code;
                session.entityName = admin.entity_name;
            }
        } catch {
            // non-critical — profile loads lazily via loadProfileFromSession
        }
    }

    if (session.role === 'OPERATOR') {
        try {
            const ops = (await iamApi.getOperators()).data ?? [];
            const op = ops.find((o) => Number(o.users_id) === Number(session.userId));
            if (op) {
                session.operatorId = op.id;
                session.establishmentId = op.establishment_id ?? null;
                session.notAssigned = !op.establishment_id;
            }
        } catch {
            // non-critical
        }
    }

    writeAuthSession(session);
    return { ok: true, session };
}

const useIamStore = defineStore('iam', () => {
    const users = ref([]);
    const admins = ref([]);
    const errors = ref([]);
    const usersLoaded = ref(false);
    const adminsLoaded = ref(false);

    const profileUser = ref(null);
    const profileEstablishment = ref(null);
    const profilePlanApi = ref('BASIC');
    const profileEntityCode = ref('');
    const profileEntityName = ref('');

    const usersCount = computed(() => (usersLoaded.value ? users.value.length : 0));
    const adminsCount = computed(() => (adminsLoaded.value ? admins.value.length : 0));

    // ─── Queries ────────────────────────────────────────────────────────────────

    async function fetchUsersAsync() {
        try {
            const response = await iamApi.getUsers();
            users.value = UserAssembler.toEntitiesFromResponse(response);
            usersLoaded.value = true;
            return users.value;
        } catch (error) {
            errors.value.push(error);
            return [];
        }
    }

    function fetchUsers() {
        fetchUsersAsync();
    }

    async function fetchAdminsAsync() {
        try {
            const response = await iamApi.getAdmins();
            admins.value = AdminAssembler.toEntitiesFromResponse(response);
            adminsLoaded.value = true;
            return admins.value;
        } catch (error) {
            errors.value.push(error);
            return [];
        }
    }

    function fetchAdmins() {
        fetchAdminsAsync();
    }

    async function loadProfileFromSession() {
        const session = readAuthSession();
        await Promise.all([fetchUsersAsync(), fetchAdminsAsync()]);

        const user =
            (session?.userId && users.value.find((u) => Number(u.id) === Number(session.userId))) ||
            users.value.find((u) => u.email === session?.email) ||
            users.value.find((u) => u.role === (session?.role ?? 'ADMIN')) ||
            users.value[0] ||
            null;

        profileUser.value = user;

        const admin =
            admins.value.find((a) => Number(a.user_id ?? a.users_id) === Number(user?.id)) ||
            admins.value.find((a) => a.entity_code === session?.entityCode) ||
            admins.value[0] ||
            null;

        profileEntityCode.value = admin?.entity_code ?? session?.entityCode ?? '';
        profileEntityName.value = admin?.entity_name ?? session?.entityName ?? '';

        profileEstablishment.value = admin
            ? { establishment_name: admin.entity_name }
            : null;

        profilePlanApi.value = session?.plan ?? 'BASIC';
    }

    function getUserById(id) {
        const idNum = parseInt(id, 10);
        return users.value.find((user) => user.id === idNum);
    }

    function getAdminById(id) {
        const idNum = parseInt(id, 10);
        return admins.value.find((admin) => admin.id === idNum);
    }

    // ─── Authentication use cases ─────────────────────────────────────────────────

    /**
     * Signs in without choosing a portal first; the backend role decides the home.
     * @param {string} email
     * @param {string} password
     * @returns {Promise<Object>} Result with session and destination route name.
     */
    async function login(email, password) {
        const command = new SignInCommand({ email, password, segment: null });
        if (!command.email || !command.password) {
            return { ok: false, error: 'invalidCredentials' };
        }

        try {
            const result = await signInReal(command);
            if (!result.ok) return result;
            const isAdmin = result.session.role === 'ADMIN';
            localStorage.setItem('userRole', isAdmin ? 'health-entity' : 'operational-staff');
            return {
                ok: true,
                session: result.session,
                notAssigned: result.session.notAssigned,
                home: isAdmin ? 'home-health-entity' : 'home-operational-staff',
            };
        } catch (err) {
            const status = err?.response?.status;
            if (status === 401 || status === 400) return { ok: false, error: 'invalidCredentials' };
            return { ok: false, error: 'network' };
        }
    }

    /**
     * Signs a health-entity administrator in.
     * @param {string} email
     * @param {string} password
     * @returns {Promise<{ok: true, session: Object} | {ok: false, error: string}>}
     */
    async function loginHealthEntity(email, password) {
        const command = new SignInCommand({ email, password, segment: 'health-entity' });
        if (!command.email || !command.password) {
            return { ok: false, error: 'invalidCredentials' };
        }

        try {
            const result = await signInReal(command);
            if (!result.ok) return result;
            localStorage.setItem('userRole', 'health-entity');
            return { ok: true, session: result.session };
        } catch (err) {
            const status = err?.response?.status;
            if (status === 401 || status === 400) return { ok: false, error: 'invalidCredentials' };
            return { ok: false, error: 'network' };
        }
    }

    /**
     * Signs an operational-staff operator in.
     * @param {string} email
     * @param {string} password
     * @returns {Promise<Object>} Result carrying the session and assignment status.
     */
    async function loginOperationalStaff(email, password) {
        const command = new SignInCommand({ email, password, segment: 'operational-staff' });
        if (!command.email || !command.password) {
            return { ok: false, error: 'invalidCredentials' };
        }

        try {
            const result = await signInReal(command);
            if (!result.ok) return result;
            localStorage.setItem('userRole', 'operational-staff');
            return { ok: true, session: result.session, notAssigned: result.session.notAssigned };
        } catch (err) {
            const status = err?.response?.status;
            if (status === 401 || status === 400) return { ok: false, error: 'invalidCredentials' };
            return { ok: false, error: 'network' };
        }
    }

    /**
     * Stores the first step of a health-entity registration in the pending flow.
     * @param {Object} form - Registration form data.
     * @returns {Promise<{ok: boolean, error?: string}>}
     */
    async function startHealthEntityRegistration(form) {
        const command = new RegisterHealthEntityCommand(form ?? {});
        if (!command.isComplete()) {
            return { ok: false, error: 'required' };
        }
        writePendingRegistration({
            segment: 'health-entity',
            name: command.name,
            email: command.email,
            password: command.password,
            entityName: command.entityName,
        });
        return { ok: true };
    }

    /**
     * Persists the plan chosen during the registration flow.
     * @param {string} catalogPlanId - Catalog plan identifier (basic | premium | enterprise).
     */
    function saveRegistrationPlan(catalogPlanId) {
        writePendingPlan({
            catalogPlanId,
            planApiValue: Subscription.catalogIdToApiPlan(catalogPlanId),
            flow: 'registration',
        });
    }

    /**
     * Demo payment (UI only) then health-entity sign-up on the real API.
     * @param {Object} payment - Card fields from the billing screen (mock).
     * @returns {Promise<Object>} Result of the registration.
     */
    async function completeHealthEntityRegistration(payment) {
        const paymentValidation = validateDemoPayment(payment);
        if (!paymentValidation.valid) {
            return { ok: false, errors: paymentValidation.errors, phase: 'payment' };
        }

        const pending = readPendingRegistration();
        const plan = readPendingPlan();
        if (!pending || pending.segment !== 'health-entity') {
            return { ok: false, error: 'missingRegistration', phase: 'registration' };
        }
        if (!plan?.planApiValue) {
            return { ok: false, error: 'missingPlan', phase: 'registration' };
        }

        await simulateDemoPaymentProcessing();

        try {
            const today = new Date().toISOString().split('T')[0];
            await iamApi.createUser({
                    name: pending.name,
                    dni: '00000000',
                    email: pending.email,
                    phone: '',
                    job_title: 'Administrador',
                    entry_date: today,
                    role: 'Admin',
                    password: pending.password,
                    photo: '',
                    entity_name: pending.entityName,
                });
            } catch (err) {
                console.error('Registration failed:', err?.response?.data ?? err.message);
                const status = err?.response?.status;
                const message = String(err?.response?.data?.error ?? err?.response?.data?.detail ?? '').toLowerCase();
                if (status === 400 && message.includes('email already exists')) {
                    return { ok: false, error: 'emailExists', phase: 'registration', paymentApproved: true };
                }
                if (status === 400 && message.includes('admin')) {
                    return { ok: false, error: 'registrationFailed', phase: 'registration', paymentApproved: true };
                }
                if (status === 500) {
                    return { ok: false, error: 'serverError', phase: 'registration', paymentApproved: true };
                }
                return { ok: false, error: 'network', phase: 'registration', paymentApproved: true };
            }

        clearRegistrationFlow();
        return { ok: true, email: pending.email, paymentApproved: true };
    }

    /**
     * Registers an operational-staff operator against an existing entity code.
     * @param {Object} form - Registration form data.
     * @returns {Promise<Object>} Result carrying the joined entity name.
     */
    async function registerOperationalStaff(form) {
        const command = new RegisterOperationalStaffCommand(form ?? {});
        if (!command.name || !command.email || !command.password) {
            return { ok: false, error: 'required' };
        }
        if (!command.entityCode) return { ok: false, error: 'entityCodeRequired' };

        try {
            const admins = (await iamApi.getAdmins()).data ?? [];
            const matchedAdmin = admins.find(
                (a) => String(a.entity_code || '').toUpperCase() === command.entityCode.toUpperCase(),
            );
            if (!matchedAdmin) return { ok: false, error: 'invalidEntityCode' };

            const today = new Date().toISOString().split('T')[0];
            await iamApi.createUser({
                name: command.name,
                dni: command.dni || '00000000',
                email: command.email,
                phone: command.phone || '',
                job_title: command.jobTitle || 'Operador',
                entry_date: today,
                role: 'Operator',
                password: command.password,
                photo: '',
                entity_code: matchedAdmin.entity_code,
            });

            return { ok: true, entityName: matchedAdmin.entity_name };
        } catch (err) {
            const status = err?.response?.status;
            if (status === 400) return { ok: false, error: 'emailExists' };
            return { ok: false, error: 'network' };
        }
    }

    /** Clears the active session and any in-progress registration flow. */
    function logout() {
        clearRegistrationFlow();
        writeAuthSession(null);
        localStorage.removeItem('userRole');
    }

    return {
        users,
        admins,
        errors,
        usersLoaded,
        adminsLoaded,
        usersCount,
        adminsCount,
        profileUser,
        profileEstablishment,
        profilePlanApi,
        profileEntityCode,
        profileEntityName,
        fetchUsers,
        fetchUsersAsync,
        fetchAdmins,
        fetchAdminsAsync,
        loadProfileFromSession,
        getUserById,
        getAdminById,
        login,
        loginHealthEntity,
        loginOperationalStaff,
        startHealthEntityRegistration,
        saveRegistrationPlan,
        completeHealthEntityRegistration,
        registerOperationalStaff,
        logout,
    };
});

export default useIamStore;
