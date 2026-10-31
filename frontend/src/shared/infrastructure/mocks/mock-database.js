/**
 * Base de datos en memoria para pruebas del frontend (sin MockAPI externa).
 * @module mock-database
 */

let nextId = 100;

function uid() {
    nextId += 1;
    return nextId;
}

const users = [
    {
        id: 1,
        name: 'Franco Diego López',
        dni: '45678901',
        email: 'admin@csg-health.pe',
        phone: '+51 999 111 222',
        job_title: 'Administrador de Entidad',
        entry_date: '2023-03-15',
        role: 'ADMIN',
        password: 'admin123',
        photo: '',
        created_at: '2023-03-15T08:00:00.000Z',
        entity_code: 'CSG-2024',
    },
    {
        id: 2,
        name: 'María Elena Ríos',
        dni: '72345678',
        email: 'maria.rios@csg-health.pe',
        phone: '+51 987 654 321',
        job_title: 'Operador de Monitoreo',
        entry_date: '2024-01-10',
        role: 'OPERATOR',
        password: 'oper123',
        photo: '',
        created_at: '2024-01-10T09:30:00.000Z',
        entity_code: 'CSG-2024',
    },
    {
        id: 3,
        name: 'Carlos Mendoza Vega',
        dni: '70123456',
        email: 'carlos.mendoza@csg-health.pe',
        phone: '+51 912 345 678',
        job_title: 'Operador Logístico',
        entry_date: '2024-06-01',
        role: 'OPERATOR',
        password: 'oper123',
        photo: '',
        created_at: '2024-06-01T10:00:00.000Z',
        entity_code: 'CSG-2024',
    },
    {
        id: 4,
        name: 'Ana Sofía Torres',
        dni: '71234567',
        email: 'ana.torres@csg-health.pe',
        phone: '+51 923 456 789',
        job_title: 'Operador de Turno',
        entry_date: '2024-08-20',
        role: 'OPERATOR',
        password: 'oper123',
        photo: '',
        created_at: '2024-08-20T07:45:00.000Z',
        entity_code: 'CSG-2024',
    },
    {
        id: 5,
        name: 'Jorge Pérez',
        dni: '89450361',
        email: 'jorg_p@gmail.com',
        phone: '654213987',
        job_title: 'Administrador de farmacia',
        entry_date: '2026-04-19',
        role: 'OPERATOR',
        password: 'oper123',
        photo: '',
        created_at: '2026-04-19T08:00:00.000Z',
        entity_code: 'CSG-2024',
    },
    {
        id: 6,
        name: 'María López',
        dni: '45612378',
        email: 'maria.lopez@csg-health.pe',
        phone: '+51 911 222 333',
        job_title: 'Técnica de monitoreo',
        entry_date: '2025-01-12',
        role: 'OPERATOR',
        password: 'oper123',
        photo: '',
        created_at: '2025-01-12T09:00:00.000Z',
        entity_code: 'CSG-2024',
    },
    {
        id: 7,
        name: 'Carlos Ruiz',
        dni: '70111222',
        email: 'carlos.ruiz@csg-health.pe',
        phone: '+51 944 555 666',
        job_title: 'Operador de almacén',
        entry_date: '2025-06-03',
        role: 'OPERATOR',
        password: 'oper123',
        photo: '',
        created_at: '2025-06-03T10:30:00.000Z',
        entity_code: 'CSG-2024',
    },
];

const admins = [
    {
        id: 1,
        entity_name: 'Clínica San Gabriel',
        entity_code: 'CSG-2024',
        schedule: 'Lun-Vie 08:00-18:00',
        users_id: 1,
    },
];

const establishments = [
    {
        id: 1,
        establishment_name: 'Hospital Nacional Arzobispo Loayza',
        establishment_type: 'HOSPITAL',
        address: 'Av. Alfonso Ugarte 1012',
        district: 'Cercado de Lima',
        city_region: 'Lima',
        country: 'Perú',
        phone: '987654321',
        email: 'contacto@loayza.gob.pe',
        website: 'www.loayza.gob.pe',
        created_at: '15/03/2024, 08:30:00',
        admin_id: 1,
        latitude: -12.0469,
        longitude: -77.0428,
        lat: -12.0469,
        lng: -77.0428,
    },
    {
        id: 2,
        establishment_name: 'Almacén Vitarte',
        establishment_type: 'WAREHOUSE',
        address: 'Av. Los Frutales 1234',
        district: 'Ate Vitarte',
        city_region: 'Lima',
        country: 'Perú',
        phone: '912345678',
        email: 'vitarte@csg-health.pe',
        website: 'www.csg-vitarte.com',
        created_at: '20/04/2024, 10:15:00',
        admin_id: 1,
        latitude: -12.025,
        longitude: -76.92,
        lat: -12.025,
        lng: -76.92,
    },
    {
        id: 3,
        establishment_name: 'Sede Miraflores',
        establishment_type: 'CLINIC',
        address: 'Av. Pardo 456',
        district: 'Miraflores',
        city_region: 'Lima',
        country: 'Perú',
        phone: '923456789',
        email: 'miraflores@csg-health.pe',
        website: 'www.csg-miraflores.com',
        created_at: '05/05/2024, 14:00:00',
        admin_id: 1,
        latitude: -12.111,
        longitude: -77.031,
        lat: -12.111,
        lng: -77.031,
    },
    {
        id: 4,
        establishment_name: 'Sede San Isidro',
        establishment_type: 'CLINIC',
        address: 'Av. Javier Prado 789',
        district: 'San Isidro',
        city_region: 'Lima',
        country: 'Perú',
        phone: '934567890',
        email: 'sanisidro@csg-health.pe',
        website: 'www.csg-sanisidro.com',
        created_at: '12/06/2024, 09:45:00',
        admin_id: 1,
        latitude: -12.095,
        longitude: -77.033,
        lat: -12.095,
        lng: -77.033,
    },
    {
        id: 5,
        establishment_name: 'Sede Trujillo',
        establishment_type: 'HOSPITAL',
        address: 'Av. América Norte 321',
        district: 'Trujillo',
        city_region: 'Trujillo',
        country: 'Perú',
        phone: '945678901',
        email: 'trujillo@csg-health.pe',
        website: 'www.csg-trujillo.com',
        created_at: '01/07/2024, 11:20:00',
        admin_id: 1,
        latitude: -8.115,
        longitude: -79.029,
        lat: -8.115,
        lng: -79.029,
    },
];

const operators = [
    {
        id: 1,
        alerts_answered: 27,
        entry_date: '2026-04-19',
        schedule: [
            { turn: 'Mañana', day: 'Lun - Vie 6:00 AM - 2:00 PM' },
            { turn: 'Tarde', day: 'Lun - Sáb 2:00 PM - 10:00 PM' },
            { turn: 'Noche', day: 'Mar - Dom 10:00 PM - 6:00 AM' },
        ],
        establishment_id: 1,
        users_id: 5,
    },
    {
        id: 2,
        alerts_answered: 42,
        entry_date: '2024-01-10',
        schedule: [
            { turn: 'Mañana', day: 'Lun - Vie 6:00 AM - 2:00 PM' },
            { turn: 'Tarde', day: 'Lun - Sáb 2:00 PM - 10:00 PM' },
        ],
        establishment_id: 1,
        users_id: 6,
    },
    {
        id: 3,
        alerts_answered: 18,
        entry_date: '2025-06-03',
        schedule: 'Noche 22:00-06:00',
        establishment_id: 1,
        users_id: 7,
    },
    { id: 4, alerts_answered: 28, schedule: 'Tarde 14:00-22:00', establishment_id: 3, users_id: 3 },
    { id: 5, alerts_answered: 15, schedule: 'Noche 22:00-06:00', establishment_id: null, users_id: 4 },
];

const devices = [
    {
        id: 1,
        exact_location: 'Refrigerador Vacunas — Almacén P1',
        type_of_medication: 'VACCINES',
        temperature: 4.3,
        humidity: 47.2,
        light_intensity: 183,
        co2: 412,
        air_quality: 412,
        vibration: 0.03,
        door_status: 'CLOSED',
        atmospheric_pressure: 1013.2,
        pm25: 12.4,
        suspended_particles: 12.4,
        created_at: '2024-01-15',
        updated_at: '2026-05-30',
        establishment_id: 1,
    },
    {
        id: 2,
        exact_location: 'Sensor Humedad — Sala Biológicos',
        type_of_medication: 'BIOLOGICALS',
        temperature: 5.1,
        humidity: 58,
        light_intensity: 95,
        co2: 380,
        air_quality: 380,
        vibration: 0.01,
        door_status: 'CERRADA',
        atmospheric_pressure: 1012,
        pm25: 8,
        suspended_particles: 8,
        created_at: '2024-02-20',
        updated_at: '2026-05-30',
        establishment_id: 1,
    },
    {
        id: 4,
        exact_location: 'Cámara fría — Ala Norte A1',
        type_of_medication: 'Vacunas refrigeradas',
        temperature: 4.2,
        humidity: 62,
        light_intensity: 120,
        co2: 410,
        air_quality: 410,
        vibration: 0.02,
        door_status: 'CERRADA',
        atmospheric_pressure: 1013,
        pm25: 12,
        suspended_particles: 12,
        created_at: '2024-01-15',
        updated_at: '2026-05-30',
        establishment_id: 1,
    },
    {
        id: 3,
        exact_location: 'Sala de monitoreo — Rack 2',
        type_of_medication: 'Sueros y soluciones',
        temperature: 22.5,
        humidity: 45,
        light_intensity: 200,
        air_quality: 420,
        vibration: 0.03,
        door_status: 'ABIERTA',
        atmospheric_pressure: 1011,
        suspended_particles: 15,
        created_at: '2024-03-10',
        updated_at: '2026-05-30',
        establishment_id: 3,
    },
];

const transports = [
    {
        id: 1,
        type_of_transport: 'Refrigerado',
        type_of_medication: 'Vacunas',
        temperature: 4.5,
        humidity: 60,
        light_intensity: 80,
        air_quality: 390,
        vibration: 0.04,
        door_status: 'CERRADA',
        atmospheric_pressure: 1012,
        suspended_particles: 10,
        created_at: '2026-05-30T08:00:00',
        updated_at: '2026-05-30T09:15:00',
        establishment_id: 2,
    },
    {
        id: 2,
        type_of_transport: 'Cadena de frío',
        type_of_medication: 'Insulina',
        temperature: 3.8,
        humidity: 55,
        light_intensity: 70,
        air_quality: 360,
        vibration: 0.02,
        door_status: 'CERRADA',
        atmospheric_pressure: 1011,
        suspended_particles: 8,
        created_at: '2026-05-29T14:00:00',
        updated_at: '2026-05-29T15:15:00',
        establishment_id: 4,
    },
    {
        id: 3,
        type_of_transport: 'VAN',
        type_of_medication: 'VACCINES',
        temperature: 4.3,
        humidity: 47.2,
        light_intensity: 183,
        air_quality: 412,
        vibration: 0.03,
        door_status: 'CLOSED',
        atmospheric_pressure: 1013.2,
        suspended_particles: 12.4,
        created_at: '2026-05-28T10:00:00',
        updated_at: '2026-05-30T08:00:00',
        establishment_id: 1,
    },
    {
        id: 4,
        type_of_transport: 'OFF_ROAD',
        type_of_medication: 'PILLS',
        temperature: 5.2,
        humidity: 52,
        light_intensity: 150,
        air_quality: 400,
        vibration: 0.05,
        door_status: 'CERRADA',
        atmospheric_pressure: 1012,
        suspended_particles: 11,
        created_at: '2026-05-27T12:00:00',
        updated_at: '2026-05-30T07:30:00',
        establishment_id: 2,
    },
    {
        id: 5,
        type_of_transport: 'MOTORCYCLE',
        type_of_medication: 'SYRUP',
        temperature: 6.1,
        humidity: 48,
        light_intensity: 200,
        air_quality: 430,
        vibration: 0.08,
        door_status: 'CERRADA',
        atmospheric_pressure: 1011.5,
        suspended_particles: 14,
        created_at: '2026-05-26T09:00:00',
        updated_at: '2026-05-30T06:45:00',
        establishment_id: 3,
    },
];

const subscriptions = [
    {
        id: 1,
        admin_id: 1,
        plan: 'PROFESSIONAL',
        start_date: '2024-03-15',
        end_date: '2026-03-15',
        status: 'ACTIVE',
        created_at: '2024-03-15T08:00:00.000Z',
    },
];

export const mockDb = {
    users,
    admins,
    establishments,
    operators,
    devices,
    transports,
    subscriptions,
};

export function cloneMockList(list) {
    return list.map((item) => ({ ...item }));
}

export function addMockEstablishment(payload) {
    const record = {
        id: uid(),
        admin_id: 1,
        country: 'Perú',
        ...payload,
        latitude: parseFloat(payload.lat ?? payload.latitude ?? -12.04),
        longitude: parseFloat(payload.lng ?? payload.longitude ?? -77.03),
        lat: parseFloat(payload.lat ?? payload.latitude ?? -12.04),
        lng: parseFloat(payload.lng ?? payload.longitude ?? -77.03),
    };
    establishments.push(record);
    return { ...record };
}

export function addMockDevice(payload) {
    const record = {
        id: uid(),
        establishment_id: payload.establishment_id ?? 1,
        created_at: new Date().toISOString().slice(0, 10),
        updated_at: new Date().toISOString().slice(0, 10),
        ...payload,
    };
    devices.push(record);
    return { ...record };
}

export function getNextMockDeviceId() {
    const maxId = devices.reduce((m, d) => Math.max(m, Number(d.id) || 0), 0);
    return maxId + 1;
}

export function addMockTransport(payload) {
    const record = {
        id: uid(),
        establishment_id: payload.establishment_id ?? 1,
        created_at: new Date().toISOString(),
        updated_at: new Date().toISOString(),
        ...payload,
    };
    transports.push(record);
    return { ...record };
}

export function getNextMockTransportId() {
    const maxId = transports.reduce((m, t) => Math.max(m, Number(t.id) || 0), 0);
    return maxId + 1;
}

export function addMockOperator(payload) {
    const record = {
        id: uid(),
        alerts_answered: 0,
        schedule: 'Mañana 06:00-14:00',
        establishment_id: null,
        ...payload,
    };
    operators.push(record);
    return { ...record };
}

export function updateMockOperator(id, patch) {
    const op = operators.find((o) => Number(o.id) === Number(id));
    if (!op) return null;
    Object.assign(op, patch);
    return { ...op };
}

export function deleteMockOperator(id) {
    const idx = operators.findIndex((o) => Number(o.id) === Number(id));
    if (idx === -1) return false;
    operators.splice(idx, 1);
    return true;
}

export function findMockOperatorByUserId(userId) {
    const uidNum = Number(userId);
    return operators.find((o) => Number(o.users_id) === uidNum) ?? null;
}

export function findMockUserByEmail(email) {
    const normalized = String(email || '').trim().toLowerCase();
    return users.find((u) => u.email.toLowerCase() === normalized) ?? null;
}

export function findMockAdminByEntityCode(code) {
    const normalized = String(code || '').trim().toUpperCase();
    return admins.find((a) => String(a.entity_code || '').toUpperCase() === normalized) ?? null;
}

export function addMockUser(payload) {
    const email = String(payload.email || '').trim().toLowerCase();
    if (findMockUserByEmail(email)) {
        const err = new Error('Email already exists');
        err.response = { status: 400, data: { error: 'Email already exists' } };
        throw err;
    }

    const roleRaw = String(payload.role || 'Operator');
    const isAdmin = roleRaw.toLowerCase() === 'admin';
    const user = {
        id: uid(),
        name: payload.name || '',
        dni: payload.dni || '00000000',
        email,
        phone: payload.phone || '',
        job_title: payload.job_title || (isAdmin ? 'Administrador' : 'Operador'),
        entry_date: payload.entry_date || new Date().toISOString().slice(0, 10),
        role: isAdmin ? 'ADMIN' : 'OPERATOR',
        password: payload.password || 'demo123',
        photo: payload.photo || '',
        created_at: new Date().toISOString(),
        entity_code: payload.entity_code || null,
    };
    users.push(user);

    if (isAdmin) {
        const entityCode =
            payload.entity_code ||
            `KL-${String(user.id).padStart(4, '0')}`;
        const admin = {
            id: uid(),
            entity_name: payload.entity_name || 'Entidad KairoLabs',
            entity_code: entityCode,
            schedule: 'Lun-Vie 08:00-18:00',
            users_id: user.id,
            user_id: user.id,
        };
        admins.push(admin);
        user.entity_code = entityCode;
        return { user: { ...user }, admin: { ...admin } };
    }

    const operator = {
        id: uid(),
        alerts_answered: 0,
        schedule: 'Mañana 06:00-14:00',
        establishment_id: null,
        users_id: user.id,
    };
    operators.push(operator);
    return { user: { ...user }, operator: { ...operator } };
}

export function authenticateMockUser(email, password) {
    const user = findMockUserByEmail(email);
    if (!user || String(user.password) !== String(password)) {
        const err = new Error('Invalid credentials');
        err.response = { status: 401, data: { error: 'Invalid credentials' } };
        throw err;
    }
    return {
        user: {
            id: user.id,
            email: user.email,
            name: user.name,
            role: String(user.role).toLowerCase() === 'admin' ? 'admin' : 'operator',
        },
        token: `mock-token-${user.id}-${Date.now()}`,
    };
}
