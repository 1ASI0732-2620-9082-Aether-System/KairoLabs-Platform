import { before, after, test } from 'node:test';
import assert from 'node:assert/strict';
import { writeFileSync, mkdirSync } from 'node:fs';
import { execFileSync } from 'node:child_process';
import { createHmac } from 'node:crypto';

// Se aceptan exclusivamente los servicios locales iniciados por el orquestador.
const base = process.env.KAIROLABS_IT_URL;
assert.equal(base, 'http://127.0.0.1:5187');
assert.match(process.env.KAIROLABS_IT_DATABASE ?? '', /^kairolabs_it_[a-f0-9]{12}$/);
const evidence = [];
const results = [];
const run = Date.now().toString(36);
const password = 'Synthetic-Only-2026!';
let admin, user, operatorUser, a, b, device, token;
const reading = { temperature: 5, humidity: 50, light_intensity: 120, air_quality: 420, vibration: 0.03, atmospheric_pressure: 1013, suspended_particles: 12, door_status: 'Closed' };

function redact(value) {
    if (Array.isArray(value)) return value.map(redact);
    if (value && typeof value === 'object') return Object.fromEntries(Object.entries(value).map(([k,v]) => [k, /password|token|hash/i.test(k) ? '[REDACTED]' : redact(v)]));
    return value;
}

async function request(method, path, body, headers = {}, authenticated = true) {
    const response = await fetch(base + path, {
        method, headers: { ...(body ? { 'Content-Type': 'application/json' } : {}), ...(authenticated && token ? { Authorization: `Bearer ${token}` } : {}), ...headers },
        ...(body ? { body: JSON.stringify(body) } : {}), signal: AbortSignal.timeout(15000),
    });
    const text = await response.text();
    let data;
    try { data = text ? JSON.parse(text) : null; } catch { data = text; }
    evidence.push({ method, path, request: redact(body), status: response.status, response: redact(data), corsOrigin: response.headers.get('access-control-allow-origin') });
    return { status: response.status, data, headers: response.headers };
}

function expectStatus(response, expected) {
    assert.equal(response.status, expected, `Esperado HTTP ${expected}; recibido ${response.status}: ${JSON.stringify(redact(response.data))}`);
}

function userPayload(role, suffix) {
    return { name: `Integration ${suffix}`, dni: '00000000', email: `it-${suffix}-${run}@example.invalid`, phone: '900000000', job_title: 'Pruebas', entry_date: '2026-10-30', role, password, photo: '', ...(role === 'Admin' ? { entity_name: `Entidad IT ${run}` } : {}) };
}

function sitePayload(name) {
    return { establishment_name: name, establishment_type: 'Warehouse', address: 'DirecciÃ³n sintÃ©tica 100', district: 'Pruebas', city_region: 'Lima', country: 'Peru', latitude: -12.04, longitude: -77.03, phone: '900000000', email: 'site@example.invalid', website: 'https://example.invalid' };
}

async function listDevices(site = a) {
    const r = await request('GET', `/api/v1/establishments/${site}/devices`);
    expectStatus(r, 200);
    return r.data;
}

function sql(query) {
    return execFileSync(process.env.KAIROLABS_MYSQL_CLIENT, ['--no-defaults','--protocol=TCP','--host=127.0.0.1','--port=33079','--user=root','--batch','--skip-column-names', process.env.KAIROLABS_IT_DATABASE, '-e', query], { encoding: 'utf8', env: { ...process.env, MYSQL_PWD: '' } }).trim();
}

function integration(id, title, check) {
    test(`${id} ${title}`, async () => {
        const start = evidence.length;
        try {
            await check();
            results.push({ id, title, status: 'passed', requests: evidence.length - start });
        } catch (error) {
            results.push({ id, title, status: 'failed', error: error.message, requests: evidence.length - start });
            throw error;
        }
    });
}

before(async () => {
    const health = await request('GET', '/health/db');
    expectStatus(health, 200);
    const created = await request('POST', '/api/v1/users', userPayload('Admin', 'admin'));
    expectStatus(created, 201);
    user = created.data;
    const login = await request('POST', '/api/v1/users/sign-in', { email: user.email, password });
    expectStatus(login, 200);
    token = login.data.token;
    const admins = await request('GET', '/api/v1/admins');
    expectStatus(admins, 200);
    admin = admins.data.find(x => x.user_id === user.id)?.id;
    assert.ok(admin, 'El alta de entidad debe crear su administrador');
    const op = await request('POST', '/api/v1/users', userPayload('Operator', 'operator'));
    expectStatus(op, 201);
    operatorUser = op.data;
    const first = await request('POST', `/api/v1/admins/${admin}/establishments`, sitePayload('Sede IT A'));
    expectStatus(first, 201); a = first.data.id;
    const second = await request('POST', `/api/v1/admins/${admin}/establishments`, sitePayload('Sede IT B'));
    expectStatus(second, 201); b = second.data.id;
    const createdDevice = await request('POST', `/api/v1/establishments/${a}/devices`, { exact_location: 'Refrigerador IT', type_of_medication: 'Refrigerated', enabled_sensors: '{"temperature":true,"humidity":true}' });
    expectStatus(createdDevice, 201); device = createdDevice.data.id;
}, { timeout: 60000 });

after(() => {
    mkdirSync('evidence', { recursive: true });
    writeFileSync('evidence/http-transcript.json', JSON.stringify(evidence, null, 2));
    writeFileSync('evidence/results.json', JSON.stringify({ date: new Date().toISOString(), repository: 'https://github.com/1ASI0732-2620-9082-Aether-System/KairoLabs-Platform', original_backend_base: 'c6c0bc7cfe03014f0ec311d8fb6c6a3ae8dddb03', backend_changes: ['Program.cs: CORS and JWT authentication', 'Iam/Interfaces/REST/UsersController.cs: public sign-up/sign-in'], environment: { api: base, database: process.env.KAIROLABS_IT_DATABASE, mysql: sql('SELECT VERSION()'), node: process.version }, executed: results.length, passed: results.filter(x => x.status === 'passed').length, failed: results.filter(x => x.status === 'failed').length, results }, null, 2));
});

integration('IT-IAM-01', 'login valido integra API, BCrypt y MySQL sin exponer credenciales', async () => {
    const r = await request('POST', '/api/v1/users/sign-in', { email: user.email, password });
    expectStatus(r, 200); assert.equal(r.data.user.id, user.id);
    assert.equal(r.data.token.split('.').length, 3); token = r.data.token;
    assert.equal(r.data.user.password, undefined); assert.equal(r.data.user.password_hash, undefined);
    assert.match(sql(`SELECT password_hash FROM users WHERE id=${Number(user.id)}`), /^\$2[aby]\$/);
});

integration('IT-IAM-02', 'clave incorrecta devuelve 401 y no emite token', async () => {
    const r = await request('POST', '/api/v1/users/sign-in', { email: user.email, password: 'wrong' });
    expectStatus(r, 401); assert.equal(r.data.token, undefined); assert.equal(r.data.status, 401);
});

integration('IT-EST-01', 'sedes creadas se recuperan por administrador y por SQL independiente', async () => {
    const r = await request('GET', `/api/v1/admins/${admin}/establishments`);
    expectStatus(r, 200); assert.ok(r.data.some(x => x.id === a)); assert.ok(r.data.some(x => x.id === b));
    assert.equal(sql(`SELECT COUNT(*) FROM establishments WHERE id IN (${a},${b})`), '2');
});

integration('IT-OP-01', 'asignacion de operador no mezcla sedes', async () => {
    const created = await request('POST', `/api/v1/establishments/${a}/operators`, { schedule: '08:00-16:00', users_id: operatorUser.id });
    expectStatus(created, 200);
    const one = await request('GET', `/api/v1/establishments/${a}/operators`);
    const two = await request('GET', `/api/v1/establishments/${b}/operators`);
    expectStatus(one, 200); expectStatus(two, 200);
    assert.ok(one.data.some(x => x.id === created.data.id && x.schedule === '08:00-16:00'));
    assert.ok(!two.data.some(x => x.id === created.data.id));
});

integration('IT-DEV-01', 'dispositivo se recupera con su sede, ubicacion y tipo', async () => {
    const found = (await listDevices()).find(x => x.id === device);
    assert.ok(found); assert.equal(found.establishment_id, a);
    assert.equal(found.exact_location, 'Refrigerador IT'); assert.equal(found.type_of_medication.toLowerCase(), 'refrigerated');
});

integration('IT-DEV-02', 'lectura persiste en MySQL y se recupera en otra solicitud', async () => {
    const r = await request('PUT', `/api/v1/establishments/${a}/devices/${device}/sensor-data`, reading);
    expectStatus(r, 200);
    const found = (await listDevices()).find(x => x.id === device);
    for (const [key,value] of Object.entries(reading)) assert.equal(found[key], value, key);
    assert.equal(sql(`SELECT CONCAT(temperature, '|', humidity, '|', vibration, '|', door_status) FROM devices WHERE id=${device}`), '5.0|50.0|0.03|Closed');
});

integration('IT-DEV-03', 'actualizar desde otra sede devuelve 404 sin modificar la lectura', async () => {
    const before = (await listDevices()).find(x => x.id === device);
    expectStatus(await request('PUT', `/api/v1/establishments/${b}/devices/${device}/sensor-data`, { ...reading, temperature: 9 }), 404);
    const after = (await listDevices()).find(x => x.id === device);
    assert.deepEqual(after, before);
});

integration('IT-DEV-04', 'DoorStatus invalido devuelve 400 sin persistencia parcial', async () => {
    const before = (await listDevices()).find(x => x.id === device);
    const r = await request('PUT', `/api/v1/establishments/${a}/devices/${device}/sensor-data`, { ...reading, temperature: 9, door_status: 'INVALID' });
    expectStatus(r, 400); assert.equal(r.data.status, 400);
    assert.deepEqual((await listDevices()).find(x => x.id === device), before);
});

integration('IT-DEV-05', 'tipo de medicamento invalido devuelve 400 sin crear dispositivo', async () => {
    const before = (await listDevices()).length;
    expectStatus(await request('POST', `/api/v1/establishments/${a}/devices`, { exact_location: 'Invalid', type_of_medication: 'INVALID' }), 400);
    assert.equal((await listDevices()).length, before);
});

integration('IT-LOG-01', 'transporte guarda lectura y rechaza identificador inexistente', async () => {
    const created = await request('POST', `/api/v1/establishments/${a}/transports`, { type_of_transport: 'Van', type_of_medication: 'Refrigerated', enabled_sensors: '{}' });
    expectStatus(created, 201); const id = created.data.id;
    expectStatus(await request('PUT', `/api/v1/establishments/${a}/transports/${id}/sensor-data`, reading), 200);
    const r = await request('GET', `/api/v1/establishments/${a}/transports`); expectStatus(r, 200);
    const found = r.data.find(x => x.id === id);
    for (const [key,value] of Object.entries(reading)) assert.equal(found[key], value, key);
    assert.equal(sql(`SELECT temperature FROM transports WHERE id=${id}`), '5.0');
    expectStatus(await request('PUT', `/api/v1/establishments/${a}/transports/2147483647/sensor-data`, reading), 404);
});

integration('IT-SUB-01', 'suscripcion completa ciclo crear consultar eliminar y nueva consulta', async () => {
    const path = `/api/v1/admins/${admin}/subscriptions`;
    const created = await request('POST', path, { plan: 'Basic', start_date: '2026-10-30', end_date: '2026-11-30' });
    expectStatus(created, 201); const id = created.data.id;
    const listed = await request('GET', path); expectStatus(listed, 200);
    const found = listed.data.find(x => x.id === id); assert.ok(found);
    assert.equal(found.plan.toLowerCase(), 'basic'); assert.equal(found.admin_id, admin);
    assert.equal(found.start_date, '2026-10-30'); assert.equal(found.end_date, '2026-11-30');
    expectStatus(await request('DELETE', `${path}/${id}`), 204);
    const after = await request('GET', path); expectStatus(after, 200);
    assert.ok(!after.data.some(x => x.id === id));
    assert.equal(sql(`SELECT COUNT(*) FROM subscriptions WHERE id=${id}`), '0');
});

integration('IT-WEB-01', 'gateways Axios y assemblers reales consumen API y datos persistidos', async () => {
    globalThis.sessionStorage = { getItem: () => JSON.stringify({ token }) };
    const { IamApi, MonitoringApi, SignInAssembler, DeviceAssembler } = await import('../frontend/.integration/gateways.mjs');
    const response = await new IamApi().signIn({ email: user.email, password });
    const session = SignInAssembler.toSessionFromResponse(response, 'health-entity');
    assert.equal(session.userId, user.id); assert.equal(session.segment, 'health-entity');
    const http = await new MonitoringApi().getDevices();
    const entities = DeviceAssembler.toEntitiesFromResponse(http);
    const found = entities.find(x => x.id === device);
    assert.equal(found.temperature, reading.temperature); assert.equal(found.establishment_id, a);
    assert.deepEqual(found.enabled_sensors, { temperature: true, humidity: true });
    // No se guardan las credenciales ni el JWT en las evidencias.
    evidence.push({ component: 'Axios gateways + assemblers', status: http.status, userId: session.userId, deviceId: found.id, mocks: false });
});

integration('IT-WEB-02', 'CORS permite origen local rechaza ajeno y admite dominio KairoLabs', async () => {
    const check = origin => request('OPTIONS', '/api/v1/devices', undefined, { Origin: origin, 'Access-Control-Request-Method': 'GET', 'Access-Control-Request-Headers': 'authorization,content-type' }, false);
    const local = await check('http://localhost:5173');
    assert.equal(local.headers.get('access-control-allow-origin'), 'http://localhost:5173');
    const denied = await check('https://example.invalid');
    assert.equal(denied.headers.get('access-control-allow-origin'), null);
    const kairo = await check('https://kairo-labs-frontend.vercel.app');
    assert.equal(kairo.headers.get('access-control-allow-origin'), 'https://kairo-labs-frontend.vercel.app');
});

integration('IT-SEC-01', 'API exige autenticacion para listar datos operativos', async () => {
    const anonymous = await request('GET', '/api/v1/devices', undefined, {}, false);
    const invalid = await request('GET', '/api/v1/devices', undefined, { Authorization: 'Bearer invalid' });
    assert.equal(anonymous.status, 401, 'Un visitante anonimo no debe listar dispositivos');
    assert.equal(invalid.status, 401, 'Un token invalido debe rechazarse');
    assert.equal(anonymous.headers.get('www-authenticate'), 'Bearer');
    const payload = JSON.parse(Buffer.from(token.split('.')[1], 'base64url').toString());
    const jwtHeader = Buffer.from(JSON.stringify({ alg: 'HS256', typ: 'JWT' })).toString('base64url');
    const sign = claims => {
        const content = `${jwtHeader}.${Buffer.from(JSON.stringify(claims)).toString('base64url')}`;
        return `${content}.${createHmac('sha256', process.env.Jwt__Secret).update(content).digest('base64url')}`;
    };
    for (const [label, invalidToken] of [
        ['firma alterada', `${token.split('.')[0]}.${token.split('.')[1]}.invalid-signature`],
        ['emisor incorrecto', sign({ ...payload, iss: 'UntrustedIssuer' })],
        ['audiencia incorrecta', sign({ ...payload, aud: 'OtherAudience' })],
        ['token expirado', sign({ ...payload, exp: Math.floor(Date.now() / 1000) - 120 })],
    ]) {
        const r = await request('GET', '/api/v1/devices', undefined, { Authorization: `Bearer ${invalidToken}` });
        assert.equal(r.status, 401, label);
    }
    const authorized = await request('GET', '/api/v1/devices');
    expectStatus(authorized, 200);
});
