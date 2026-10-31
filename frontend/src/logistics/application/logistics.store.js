/**
 * Application service store for the Logistics bounded context.
 * All data is loaded from the Render REST API.
 *
 * @module useLogisticsStore
 */
import { defineStore } from 'pinia';
import { computed, ref } from 'vue';
import { LogisticsApi } from '../infrastructure/logistics-api.js';
import { TransportAssembler } from '../infrastructure/transport.assembler.js';

const logisticsApi = new LogisticsApi();

function generateSensorReading(typeOfMedication) {
    const r = (min, max, dec = 1) => parseFloat((min + Math.random() * (max - min)).toFixed(dec));
    const door = Math.random() < 0.15 ? 'Open' : 'Closed';
    switch (String(typeOfMedication).toLowerCase()) {
        case 'refrigerated':
            return { temperature: r(2, 8), humidity: r(40, 60), light_intensity: r(0, 50), air_quality: r(350, 420), vibration: r(0.01, 0.08, 3), atmospheric_pressure: r(1010, 1015), suspended_particles: r(5, 15), door_status: door };
        case 'biological':
            return { temperature: r(-20, -15), humidity: r(20, 35), light_intensity: r(0, 30), air_quality: r(380, 450), vibration: r(0.00, 0.03, 3), atmospheric_pressure: r(1008, 1013), suspended_particles: r(3, 10), door_status: door };
        case 'controlled':
            return { temperature: r(15, 25), humidity: r(35, 55), light_intensity: r(100, 300), air_quality: r(400, 500), vibration: r(0.05, 0.15, 3), atmospheric_pressure: r(1005, 1015), suspended_particles: r(10, 30), door_status: door };
        default:
            return { temperature: r(18, 28), humidity: r(30, 60), light_intensity: r(50, 400), air_quality: r(350, 500), vibration: r(0.02, 0.12, 3), atmospheric_pressure: r(1005, 1020), suspended_particles: r(5, 25), door_status: door };
    }
}

const useLogisticsStore = defineStore('logistics', () => {
    const transports = ref([]);
    const errors = ref([]);
    const transportsLoaded = ref(false);
    let simulationInterval = null;

    const transportsCount = computed(() => (transportsLoaded.value ? transports.value.length : 0));

    async function fetchTransportsAsync() {
        try {
            const response = await logisticsApi.getTransports();
            transports.value = TransportAssembler.toEntitiesFromResponse(response);
            transportsLoaded.value = true;
            return transports.value;
        } catch (error) {
            errors.value.push(error);
            return [];
        }
    }

    function fetchTransports() {
        fetchTransportsAsync();
    }

    function getTransportById(id) {
        const idNum = parseInt(id, 10);
        return transports.value.find((t) => t.id === idNum);
    }

    async function createTransportAsync(transport) {
        const response = await logisticsApi.createTransport(transport);
        const entity = TransportAssembler.toEntityFromResource(response.data);
        transports.value.push(entity);
        return entity;
    }

    async function deleteTransportAsync(transport) {
        await logisticsApi.deleteTransport(transport.id, transport.establishment_id);
        transports.value = transports.value.filter((t) => t.id !== transport.id);
    }

    async function pushSimulatedReadings() {
        if (!transports.value.length) return;
        await Promise.allSettled(
            transports.value.map(async (transport) => {
                try {
                    const reading = generateSensorReading(transport.type_of_medication);
                    await logisticsApi.updateSensorData(transport.id, reading, transport.establishment_id);
                    const idx = transports.value.findIndex((t) => t.id === transport.id);
                    if (idx !== -1) transports.value[idx] = { ...transports.value[idx], ...reading };
                } catch { /* non-fatal */ }
            }),
        );
    }

    function startSimulation(intervalMs = 10000) {
        if (simulationInterval !== null) return;
        pushSimulatedReadings();
        simulationInterval = setInterval(pushSimulatedReadings, intervalMs);
    }

    function stopSimulation() {
        if (simulationInterval !== null) {
            clearInterval(simulationInterval);
            simulationInterval = null;
        }
    }

    return {
        transports,
        errors,
        transportsLoaded,
        transportsCount,
        fetchTransports,
        fetchTransportsAsync,
        getTransportById,
        createTransportAsync,
        deleteTransportAsync,
        pushSimulatedReadings,
        startSimulation,
        stopSimulation,
    };
});

export default useLogisticsStore;
