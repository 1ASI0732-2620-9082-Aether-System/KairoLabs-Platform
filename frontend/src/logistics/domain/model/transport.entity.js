/**
 * Transport value-carrying entity within the Logistics bounded context.
 *
 * @class Transport
 */
export class Transport {
    /**
     * @param {Object} params - Entity attributes.
     */
    constructor({
        id = null,
        type_of_transport = '',
        type_of_medication = '',
        temperature = null,
        humidity = null,
        light_intensity = null,
        air_quality = null,
        vibration = null,
        door_status = '',
        atmospheric_pressure = null,
        suspended_particles = null,
        created_at = '',
        updated_at = '',
        establishment_id = null,
        enabled_sensors = null,
    } = {}) {
        this.id = id;
        this.type_of_transport = type_of_transport;
        this.type_of_medication = type_of_medication;
        this.temperature = temperature;
        this.humidity = humidity;
        this.light_intensity = light_intensity;
        this.air_quality = air_quality;
        this.vibration = vibration;
        this.door_status = door_status;
        this.atmospheric_pressure = atmospheric_pressure;
        this.suspended_particles = suspended_particles;
        this.created_at = created_at;
        this.updated_at = updated_at;
        this.establishment_id = establishment_id;
        // Parse enabled_sensors JSON string from backend
        if (typeof enabled_sensors === 'string' && enabled_sensors.trim().startsWith('{')) {
            try { this.enabled_sensors = JSON.parse(enabled_sensors); } catch { this.enabled_sensors = null; }
        } else if (enabled_sensors && typeof enabled_sensors === 'object') {
            this.enabled_sensors = enabled_sensors;
        } else {
            this.enabled_sensors = null;
        }
    }
}
