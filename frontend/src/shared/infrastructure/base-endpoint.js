/**
 * Shared infrastructure helper that wraps a single REST resource endpoint.
 * Provides generic CRUD operations using the Axios instance from a {@link BaseApi}.
 *
 * @class BaseEndpoint
 */
export class BaseEndpoint {
    /**
     * @param {import('./base-api.js').BaseApi} baseApi - The API gateway that owns the HTTP client.
     * @param {string} endpointPath - Relative path of this endpoint (e.g. `/tutorials`).
     */
    constructor(baseApi, endpointPath) {
        this.http = baseApi.http;
        this.endpointPath = endpointPath;
    }

    /**
     * Retrieves all resources from the endpoint.
     * @returns {Promise<import('axios').AxiosResponse<Array<Object>|Object>>} HTTP response with the resource collection.
     */
    getAll() {
        return this.http.get(this.endpointPath);
    }

    /**
     * Retrieves a single resource by identifier.
     * @param {number|string} id - Resource identifier.
     * @returns {Promise<import('axios').AxiosResponse<Object>>} HTTP response with the resource.
     */
    getById(id) {
        return this.http.get(`${this.endpointPath}/${id}`);
    }

    /**
     * Creates a new resource.
     * @param {Object} resource - Resource payload to persist.
     * @returns {Promise<import('axios').AxiosResponse<Object>>} HTTP response with the created resource.
     */
    create(resource) {
        return this.http.post(this.endpointPath, resource);
    }

    /**
     * Updates an existing resource by identifier.
     * @param {number|string} id - Resource identifier.
     * @param {Object} resource - Resource payload with updated data.
     * @returns {Promise<import('axios').AxiosResponse<Object>>} HTTP response with the updated resource.
     */
    update(id, resource) {
        return this.http.put(`${this.endpointPath}/${id}`, resource);
    }

    /**
     * Deletes a resource by identifier.
     * @param {number|string} id - Resource identifier to remove.
     * @returns {Promise<import('axios').AxiosResponse<void>>} HTTP response confirming deletion.
     */
    delete(id) {
        return this.http.delete(`${this.endpointPath}/${id}`);
    }
}