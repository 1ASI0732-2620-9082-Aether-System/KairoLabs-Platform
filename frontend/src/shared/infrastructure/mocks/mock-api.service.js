/**
 * Simula respuestas HTTP para desarrollo sin backend.
 * @module mock-api.service
 */
import { mockOk, withMockDelay } from './mock-config.js';
import {
    addMockDevice,
    addMockEstablishment,
    addMockOperator,
    addMockTransport,
    addMockUser,
    authenticateMockUser,
    cloneMockList,
    deleteMockOperator,
    findMockAdminByEntityCode,
    mockDb,
    updateMockOperator,
} from './mock-database.js';

export const MockApi = {
    getUsers: () => withMockDelay(mockOk(cloneMockList(mockDb.users))),
    getAdmins: () => withMockDelay(mockOk(cloneMockList(mockDb.admins))),
    getEstablishments: () => withMockDelay(mockOk(cloneMockList(mockDb.establishments))),
    getOperators: () => withMockDelay(mockOk(cloneMockList(mockDb.operators))),
    getDevices: () => withMockDelay(mockOk(cloneMockList(mockDb.devices))),
    getTransports: () => withMockDelay(mockOk(cloneMockList(mockDb.transports))),
    getSubscriptions: () => withMockDelay(mockOk(cloneMockList(mockDb.subscriptions))),
    createEstablishment: (resource) =>
        withMockDelay(mockOk(addMockEstablishment(resource))),
    createDevice: (resource) => withMockDelay(mockOk(addMockDevice(resource))),
    createTransport: (resource) => withMockDelay(mockOk(addMockTransport(resource))),
    createOperator: (resource) => withMockDelay(mockOk(addMockOperator(resource))),
    updateOperator: (id, resource) => withMockDelay(mockOk(updateMockOperator(id, resource))),
    deleteOperator: (id) => withMockDelay(mockOk(deleteMockOperator(id))),

    async signIn(email, password) {
        await withMockDelay(null, 320);
        return mockOk(authenticateMockUser(email, password));
    },

    async createUser(resource) {
        await withMockDelay(null, 420);
        const created = addMockUser(resource);
        return mockOk(created.user);
    },

    async findAdminByEntityCode(code) {
        await withMockDelay(null, 200);
        return mockOk(findMockAdminByEntityCode(code));
    },
};
