using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TechnoByteLambders.MediTrackSensor.Platform.Shared.Infrastructure.OpenApi;

/// <summary>
/// Adds summary, description, path-parameter examples and request-body examples for every REST endpoint.
/// </summary>
public sealed class SwaggerEndpointDocumentationFilter : IOperationFilter
{
    private sealed record EndpointDoc(
        string Summary,
        string Description,
        string? RequestBodyJson = null,
        Dictionary<string, object>? PathExamples = null);

    private static readonly Dictionary<string, EndpointDoc> Docs = new(StringComparer.OrdinalIgnoreCase)
    {
        // ─── Users (auth) ───────────────────────────────────────────────────
        ["GET:/api/v1/users"] = new(
            "Listar todos los usuarios",
            "Devuelve la lista completa de usuarios registrados. No requiere body. " +
            "Útil para verificar IDs antes de crear operadores (campo users_id)."),

        ["POST:/api/v1/users"] = new(
            "Registro (sign-up)",
            "**Paso 1 del flujo.** Crea un usuario. Si role=Admin y envías entity_name, " +
            "también crea la entidad de salud (admin) en la misma transacción. " +
            "Usa un **email nuevo** en cada prueba.",
            """
            {
              "name": "María García",
              "dni": "87654321",
              "email": "nuevo.admin@clinica.com",
              "phone": "+51999999999",
              "job_title": "Administrador",
              "entry_date": "2026-07-06",
              "role": "Admin",
              "password": "Secret123!",
              "photo": "",
              "entity_name": "Clínica San Martín"
            }
            """),

        ["POST:/api/v1/users/sign-in"] = new(
            "Inicio de sesión (sign-in)",
            "**Paso 2 del flujo.** Autentica con email y password. " +
            "Respuesta 200: `{ user, token }`. Usa credenciales de un usuario ya registrado.",
            """
            {
              "email": "pilsen@gmail.com",
              "password": "tu_password_aqui"
            }
            """),

        ["DELETE:/api/v1/users/{id}"] = new(
            "Eliminar usuario",
            "Elimina un usuario por ID. Respuesta 204 si tuvo éxito.",
            PathExamples: new Dictionary<string, object> { ["id"] = 1 }),

        // ─── Admins ───────────────────────────────────────────────────────
        ["GET:/api/v1/admins"] = new(
            "Listar admins (entidades de salud)",
            "Lista todas las entidades de salud. Anota el **id** del admin para crear establecimientos y suscripciones."),

        ["POST:/api/v1/admins"] = new(
            "Crear admin manualmente",
            "Crea un admin vinculado a un user_id existente. " +
            "En registro normal esto se hace automáticamente con POST /users + entity_name.",
            """
            {
              "entity_name": "Clínica San Martín",
              "entity_code": "ENT-99",
              "schedule": "Lun-Vie 8:00-18:00",
              "user_id": 1
            }
            """),

        // ─── Establishments (nested under admin) ──────────────────────────
        ["GET:/api/v1/admins/{adminid}/establishments"] = new(
            "Listar establecimientos de un admin",
            "Devuelve solo los establecimientos cuyo admin_id coincide con el de la URL.",
            PathExamples: new Dictionary<string, object> { ["adminId"] = 1 }),

        ["POST:/api/v1/admins/{adminid}/establishments"] = new(
            "Crear establecimiento (REST anidado)",
            "**Paso 3.** Crea un establecimiento bajo el admin indicado en la URL. " +
            "No envíes admin_id en el body — va en la ruta.",
            """
            {
              "establishment_name": "Farmacia Central",
              "establishment_type": "Pharmacy",
              "address": "Av. Principal 123",
              "district": "Miraflores",
              "city_region": "Lima",
              "country": "PE",
              "latitude": -12.1201,
              "longitude": -77.0342,
              "phone": "+51988887777",
              "email": "farmacia@clinica.com",
              "website": "https://farmacia.clinica.com"
            }
            """,
            PathExamples: new Dictionary<string, object> { ["adminId"] = 1 }),

        ["GET:/api/v1/establishments"] = new(
            "Listar todos los establecimientos",
            "Lista global de establecimientos. Anota el **id** para crear devices, operators y transports."),

        ["DELETE:/api/v1/establishments/{id}"] = new(
            "Eliminar establecimiento",
            "Elimina un establecimiento por ID. Respuesta 204.",
            PathExamples: new Dictionary<string, object> { ["id"] = 1 }),

        // ─── Operators (nested under establishment) ───────────────────────
        ["GET:/api/v1/establishments/{establishmentid}/operators"] = new(
            "Listar operadores de un establecimiento",
            "Operadores asignados al establishmentId de la URL.",
            PathExamples: new Dictionary<string, object> { ["establishmentId"] = 1 }),

        ["POST:/api/v1/establishments/{establishmentid}/operators"] = new(
            "Asignar operador a establecimiento",
            "Crea un operador. **users_id** debe ser el ID de un usuario con role=Operator ya registrado.",
            """
            {
              "schedule": "Lun-Sáb 9:00-17:00",
              "users_id": 2
            }
            """,
            PathExamples: new Dictionary<string, object> { ["establishmentId"] = 1 }),

        ["PUT:/api/v1/establishments/{establishmentid}/operators/{operatorid}"] = new(
            "Actualizar horario de operador",
            "Actualiza el schedule del operador en este establecimiento.",
            """
            {
              "schedule": "Lun-Vie 8:00-16:00"
            }
            """,
            PathExamples: new Dictionary<string, object>
            {
                ["establishmentId"] = 1,
                ["operatorId"] = 1
            }),

        ["PUT:/api/v1/establishments/{establishmentid}/operators/{operatorid}/alert-answered"] = new(
            "Incrementar alertas atendidas",
            "Suma 1 al contador alerts_answered del operador. Sin body.",
            PathExamples: new Dictionary<string, object>
            {
                ["establishmentId"] = 1,
                ["operatorId"] = 1
            }),

        ["DELETE:/api/v1/establishments/{establishmentid}/operators/{operatorid}"] = new(
            "Eliminar operador del establecimiento",
            "Elimina el operador si pertenece al establecimiento indicado.",
            PathExamples: new Dictionary<string, object>
            {
                ["establishmentId"] = 1,
                ["operatorId"] = 1
            }),

        ["GET:/api/v1/operators"] = new(
            "Listar todos los operadores",
            "Lista global. Útil para login de personal operativo y asignaciones."),

        ["DELETE:/api/v1/operators/{id}"] = new(
            "Eliminar operador (ruta plana)",
            "Elimina operador por ID. Respuesta 204.",
            PathExamples: new Dictionary<string, object> { ["id"] = 1 }),

        // ─── Devices (nested under establishment) ─────────────────────────
        ["GET:/api/v1/establishments/{establishmentid}/devices"] = new(
            "Listar dispositivos de un establecimiento",
            "Sensores IoT del establecimiento indicado en la URL.",
            PathExamples: new Dictionary<string, object> { ["establishmentId"] = 1 }),

        ["POST:/api/v1/establishments/{establishmentid}/devices"] = new(
            "Registrar dispositivo IoT",
            "**Paso 4.** Crea un dispositivo de monitoreo. type_of_medication: Refrigerated | Biological | Controlled.",
            """
            {
              "exact_location": "Cámara fría - Estante 3",
              "type_of_medication": "Refrigerated",
              "enabled_sensors": "{\"temperature\":true,\"humidity\":true,\"door\":true}"
            }
            """,
            PathExamples: new Dictionary<string, object> { ["establishmentId"] = 1 }),

        ["PUT:/api/v1/establishments/{establishmentid}/devices/{deviceid}/sensor-data"] = new(
            "Actualizar lecturas del sensor",
            "Envía datos simulados de temperatura, humedad, etc. door_status: Open | Closed.",
            """
            {
              "temperature": 4.2,
              "humidity": 55.0,
              "light_intensity": 120.5,
              "air_quality": 98.0,
              "vibration": 0.1,
              "atmospheric_pressure": 1013.25,
              "suspended_particles": 12.0,
              "door_status": "Closed"
            }
            """,
            PathExamples: new Dictionary<string, object>
            {
                ["establishmentId"] = 1,
                ["deviceId"] = 1
            }),

        ["DELETE:/api/v1/establishments/{establishmentid}/devices/{deviceid}"] = new(
            "Eliminar dispositivo",
            "Elimina el dispositivo si pertenece al establecimiento.",
            PathExamples: new Dictionary<string, object>
            {
                ["establishmentId"] = 1,
                ["deviceId"] = 1
            }),

        ["GET:/api/v1/devices"] = new(
            "Listar todos los dispositivos",
            "Lista global de dispositivos de todos los establecimientos."),

        ["DELETE:/api/v1/devices/{id}"] = new(
            "Eliminar dispositivo (ruta plana)",
            "Elimina dispositivo por ID.",
            PathExamples: new Dictionary<string, object> { ["id"] = 1 }),

        // ─── Transports (nested under establishment) ──────────────────────
        ["GET:/api/v1/establishments/{establishmentid}/transports"] = new(
            "Listar transportes de un establecimiento",
            "Unidades de transporte refrigerado del establecimiento.",
            PathExamples: new Dictionary<string, object> { ["establishmentId"] = 1 }),

        ["POST:/api/v1/establishments/{establishmentid}/transports"] = new(
            "Registrar transporte",
            "Crea un transporte con sensores. type_of_transport y type_of_medication suelen ser Refrigerated.",
            """
            {
              "type_of_transport": "Refrigerated",
              "type_of_medication": "Refrigerated",
              "enabled_sensors": "{\"temperature\":true,\"humidity\":true,\"door\":true}"
            }
            """,
            PathExamples: new Dictionary<string, object> { ["establishmentId"] = 1 }),

        ["PUT:/api/v1/establishments/{establishmentid}/transports/{transportid}/sensor-data"] = new(
            "Actualizar lecturas del transporte",
            "Igual que devices: envía telemetría simulada del vehículo/unidad.",
            """
            {
              "temperature": 3.8,
              "humidity": 60.0,
              "light_intensity": 80.0,
              "air_quality": 95.0,
              "vibration": 0.3,
              "atmospheric_pressure": 1012.0,
              "suspended_particles": 8.0,
              "door_status": "Closed"
            }
            """,
            PathExamples: new Dictionary<string, object>
            {
                ["establishmentId"] = 1,
                ["transportId"] = 1
            }),

        ["DELETE:/api/v1/establishments/{establishmentid}/transports/{transportid}"] = new(
            "Eliminar transporte",
            "Elimina el transporte del establecimiento.",
            PathExamples: new Dictionary<string, object>
            {
                ["establishmentId"] = 1,
                ["transportId"] = 1
            }),

        ["GET:/api/v1/transports"] = new(
            "Listar todos los transportes",
            "Lista global de transportes."),

        ["DELETE:/api/v1/transports/{id}"] = new(
            "Eliminar transporte (ruta plana)",
            "Elimina transporte por ID.",
            PathExamples: new Dictionary<string, object> { ["id"] = 1 }),

        // ─── Subscriptions (nested under admin) ───────────────────────────
        ["GET:/api/v1/admins/{adminid}/subscriptions"] = new(
            "Listar suscripciones de un admin",
            "Planes activos del admin indicado en la URL.",
            PathExamples: new Dictionary<string, object> { ["adminId"] = 1 }),

        ["POST:/api/v1/admins/{adminid}/subscriptions"] = new(
            "Crear suscripción",
            "plan: Basic | Premium | Enterprise. Fechas en formato YYYY-MM-DD.",
            """
            {
              "plan": "Premium",
              "start_date": "2026-07-01",
              "end_date": "2027-06-30"
            }
            """,
            PathExamples: new Dictionary<string, object> { ["adminId"] = 1 }),

        ["DELETE:/api/v1/admins/{adminid}/subscriptions/{subscriptionid}"] = new(
            "Eliminar suscripción de un admin",
            "Elimina la suscripción si pertenece al admin.",
            PathExamples: new Dictionary<string, object>
            {
                ["adminId"] = 1,
                ["subscriptionId"] = 1
            }),

        ["GET:/api/v1/subscriptions"] = new(
            "Listar todas las suscripciones",
            "Lista global de suscripciones."),

        ["DELETE:/api/v1/subscriptions/{id}"] = new(
            "Eliminar suscripción (ruta plana)",
            "Elimina suscripción por ID.",
            PathExamples: new Dictionary<string, object> { ["id"] = 1 }),
    };

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var method = context.ApiDescription.HttpMethod ?? "";
        var path = "/" + (context.ApiDescription.RelativePath ?? "").TrimStart('/');
        var key = $"{method}:{path}";

        if (!Docs.TryGetValue(key, out var doc))
            return;

        operation.Summary = doc.Summary;
        operation.Description = doc.Description;

        if (doc.PathExamples is not null)
        {
            foreach (var parameter in operation.Parameters)
            {
                if (!doc.PathExamples.TryGetValue(parameter.Name, out var value))
                    continue;

                parameter.Example = value switch
                {
                    int i => new OpenApiInteger(i),
                    long l => new OpenApiLong(l),
                    string s => new OpenApiString(s),
                    _ => new OpenApiString(value.ToString())
                };
                parameter.Description ??= $"Ejemplo para probar: **{value}**";
            }
        }

        if (string.IsNullOrWhiteSpace(doc.RequestBodyJson))
            return;

        operation.RequestBody ??= new OpenApiRequestBody
        {
            Required = true,
            Content = new Dictionary<string, OpenApiMediaType>()
        };

        if (!operation.RequestBody.Content.ContainsKey("application/json"))
            operation.RequestBody.Content["application/json"] = new OpenApiMediaType();

        operation.RequestBody.Content["application/json"].Example =
            OpenApiAnyFactory.CreateFromJson(doc.RequestBodyJson.Trim());
        operation.RequestBody.Description = "Ejemplo listo para **Try it out** — puedes editar los valores antes de ejecutar.";
    }
}
