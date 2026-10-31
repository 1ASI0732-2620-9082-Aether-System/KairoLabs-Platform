/**
 * Application service store for the Monitoring bounded context.
 * All data is loaded from the Render REST API.
 *
 * @module useMonitoringStore
 */
import { defineStore } from 'pinia';
import { computed, ref } from 'vue';
import { MonitoringApi } from '../infrastructure/monitoring-api.js';
import { DeviceAssembler } from '../infrastructure/device.assembler.js';

const monitoringApi = new MonitoringApi();

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

const useMonitoringStore = defineStore('monitoring', () => {
    const devices = ref([]);
    const errors = ref([]);
    const devicesLoaded = ref(false);
    let simulationInterval = null;

    const devicesCount = computed(() => (devicesLoaded.value ? devices.value.length : 0));

    function fetchDevices() {
        fetchDevicesAsync();
    }

    async function fetchDevicesAsync() {
        try {
            const response = await monitoringApi.getDevices();
            devices.value = DeviceAssembler.toEntitiesFromResponse(response);
            devicesLoaded.value = true;
            return devices.value;
        } catch (error) {
            errors.value.push(error);
            return [];
        }
    }

    function getDeviceById(id) {
        const idNum = parseInt(id, 10);
        return devices.value.find((d) => d.id === idNum);
    }

    async function createDeviceAsync(device) {
        const response = await monitoringApi.createDevice(device);
        const entity = DeviceAssembler.toEntityFromResource(response.data);
        devices.value.push(entity);
        return entity;
    }

    async function deleteDeviceAsync(device) {
        await monitoringApi.deleteDevice(device.id, device.establishment_id);
        const index = devices.value.findIndex((d) => d.id === device.id);
        if (index !== -1) devices.value.splice(index, 1);
        return true;
    }

    async function pushSimulatedReadings() {
        if (!devices.value.length) return;
        await Promise.allSettled(
            devices.value.map(async (device) => {
                try {
                    const reading = generateSensorReading(device.type_of_medication);
                    await monitoringApi.updateSensorData(device.id, reading, device.establishment_id);
                    const idx = devices.value.findIndex((d) => d.id === device.id);
                    if (idx !== -1) devices.value[idx] = { ...devices.value[idx], ...reading };
                } catch { /* retry next tick */ }
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
        devices,
        errors,
        devicesLoaded,
        devicesCount,
        fetchDevices,
        fetchDevicesAsync,
        getDeviceById,
        createDeviceAsync,
        deleteDeviceAsync,
        pushSimulatedReadings,
        startSimulation,
        stopSimulation,
    };
});

export default useMonitoringStore;
