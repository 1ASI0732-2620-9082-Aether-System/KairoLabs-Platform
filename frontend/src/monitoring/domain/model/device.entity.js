/**
 * Device (sensor) entity within the Monitoring bounded context.
 *
 * @class Device
 */
export class Device {
    constructor({
        id = null,
        exact_location = '',
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
        this.exact_location = exact_location;
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

    // ── Sensor metric definitions ─────────────────────────────────────────────

    static METRIC_DEFS = [
        { key: 'temperature', field: 'temperature', labelKey: 'sensorTemp', unit: '°C', decimals: 1 },
        { key: 'humidity', field: 'humidity', labelKey: 'sensorHumidity', unit: '% RH', decimals: 1 },
        { key: 'light', field: 'light_intensity', altFields: ['lightIntensity', 'lux'], labelKey: 'sensorLight', unit: 'LUX', decimals: 0 },
        { key: 'vibration', field: 'vibration', labelKey: 'sensorVibration', unit: 'm/s²', decimals: 2 },
        { key: 'door', field: 'door_status', altFields: ['doorStatus', 'door'], labelKey: 'sensorDoor', discrete: true },
        { key: 'co2', field: 'co2', altFields: ['air_quality', 'co2_ppm'], labelKey: 'sensorCo2', unit: 'CO₂ PPM', decimals: 0 },
        { key: 'pressure', field: 'atmospheric_pressure', altFields: ['pressure'], labelKey: 'sensorPressure', unit: 'hPa', decimals: 1 },
        { key: 'pm25', field: 'pm25', altFields: ['pm2_5', 'suspended_particles'], labelKey: 'sensorPm25', unit: 'PM2.5', decimals: 1 },
    ];

    static #THRESHOLDS = {
        temperature: { min: 2, max: 8, warnBelow: 2.5, warnAbove: 7, safe: 4.5, drift: 0.12 },
        humidity:    { min: 35, max: 65, warnBelow: 38, warnAbove: 60, safe: 47, drift: 0.6 },
        light:       { min: 0, max: 350, warnAbove: 280, safe: 120, drift: 8 },
        vibration:   { min: 0, max: 0.12, warnAbove: 0.08, safe: 0.03, drift: 0.008 },
        co2:         { min: 0, max: 900, warnAbove: 700, safe: 420, drift: 12 },
        pressure:    { min: 990, max: 1030, warnBelow: 992, warnAbove: 1026, safe: 1013.2, drift: 0.4 },
        pm25:        { min: 0, max: 30, warnAbove: 22, safe: 12, drift: 0.5 },
    };

    // ── Helpers ───────────────────────────────────────────────────────────────

    static toNum(v) {
        if (v == null || (typeof v === 'string' && v.trim() === '')) return null;
        const n = Number(v);
        return Number.isFinite(n) ? n : null;
    }

    static findMetricDef(key) {
        return Device.METRIC_DEFS.find((d) => d.key === key);
    }

    static readField(dev, def) {
        if (!dev) return null;
        const primary = dev[def.field];
        if (primary != null && primary !== '') return primary;
        for (const alt of def.altFields || []) {
            const v = dev[alt];
            if (v != null && v !== '') return v;
        }
        return null;
    }

    // ── Threshold evaluation ──────────────────────────────────────────────────

    static evaluateMetric(key, rawValue) {
        if (key === 'door') {
            const door = String(rawValue ?? '').toUpperCase();
            if (!door) return 'unknown';
            return door.includes('OPEN') || door.includes('ABIERTA') ? 'critical' : 'ok';
        }
        const value = Device.toNum(rawValue);
        if (value == null) return 'unknown';
        const t = Device.#THRESHOLDS[key];
        if (!t) return 'ok';
        if (t.max != null && value > t.max) return 'critical';
        if (t.min != null && value < t.min) return 'critical';
        if (t.warnAbove != null && value > t.warnAbove) return 'warning';
        if (t.warnBelow != null && value < t.warnBelow) return 'warning';
        return 'ok';
    }

    static isOutOfRange(key, rawValue) {
        const st = Device.evaluateMetric(key, rawValue);
        return st === 'warning' || st === 'critical';
    }

    static getSafeValue(key) {
        if (key === 'door') return 'CLOSED';
        return Device.#THRESHOLDS[key]?.safe ?? null;
    }

    static driftValue(key, current) {
        const t = Device.#THRESHOLDS[key];
        if (!t) return current;
        const base = Device.toNum(current) ?? t.safe;
        let next = base + (Math.random() - 0.48) * t.drift * 2;
        if (t.min != null) next = Math.max(t.min - t.drift * 3, next);
        if (t.max != null) next = Math.min(t.max + t.drift * 4, next);
        const decimals = Device.findMetricDef(key)?.decimals ?? 1;
        return Number(next.toFixed(decimals));
    }

    // ── Formatting ────────────────────────────────────────────────────────────

    static formatMetric(def, rawValue) {
        if (def.discrete) return String(rawValue ?? '—') || '—';
        const value = Device.toNum(rawValue);
        if (value == null) return '—';
        return `${value.toFixed(def.decimals ?? 1)} ${def.unit}`;
    }

    static formatSafeMetric(key) {
        const def = Device.findMetricDef(key);
        if (!def) return '—';
        return Device.formatMetric(def, Device.getSafeValue(key));
    }

    // ── Live snapshot ─────────────────────────────────────────────────────────

    static initLiveSnapshot(dev) {
        const snap = {};
        for (const def of Device.METRIC_DEFS) {
            // Only initialize metrics that are enabled for this device
            const enabled = dev?.enabled_sensors;
            const hasEnabledMap = enabled && typeof enabled === 'object' && Object.keys(enabled).length > 0;
            if (hasEnabledMap && enabled[def.key] !== true) continue;

            if (def.discrete) {
                snap[def.key] = Device.readField(dev, def) ?? 'CLOSED';
            } else {
                const v = Device.readField(dev, def);
                snap[def.key] = v != null && v !== '' ? Device.toNum(v) : Device.getSafeValue(def.key);
            }
        }
        return snap;
    }

    static isMetricEnabled(def, dev, liveSnap) {
        const enabled = dev?.enabled_sensors;
        // If enabled_sensors is a parsed object with keys, only show explicitly enabled sensors
        if (enabled && typeof enabled === 'object' && Object.keys(enabled).length > 0) {
            return enabled[def.key] === true;
        }
        // No enabled_sensors (old device or empty): show everything
        if (def.discrete) return true;
        return liveSnap[def.key] != null;
    }
}
