# KairoLabs Platform — Core Integration Tests

Backend ASP.NET Core 10 / EF Core / MySQL reutilizado de MediTrackSensor (base c6c0bc7cfe03014f0ec311d8fb6c6a3ae8dddb03), con correcciones CORS y autenticación JWT. El código ejecutado está en `backend/`; la suite independiente está en `tests/integration.test.mjs`.

## Reproducir

Windows x64 y Node.js 24+. Desde la raíz:

```powershell
npm run setup
npm test
```

La preparación descarga .NET 10 y MySQL portátiles oficiales y verifica checksums. La primera ejecución restaura NuGet y npm. Requiere Internet, espacio en disco y puertos 5187/33079 libres. Abrir `KairoLabs-Integration.code-workspace` para ejecutar con F5 en VS Code.

El ejecutor compila este backend, inicia MySQL y API locales, crea una base temporal, ejecuta 14 pruebas HTTP con verificaciones SQL independientes y elimina su base/procesos al terminar. No usa producción ni mocks. `frontend/` contiene los gateways/assemblers de referencia para IT-WEB-01; no es el frontend desplegado.

## Correcciones y alcance

Program.cs permite el origen exacto https://kairo-labs-frontend.vercel.app y valida firma, emisor, audiencia y vencimiento JWT, con autenticación por defecto. UsersController declara login/registro públicos. Salud y redirección inicial son públicos. Configurar Jwt__Secret (32 caracteres mínimo), Jwt__Issuer, Jwt__Audience y DATABASE_URL para una ejecución fuera del runner; las claves del runner son exclusivamente locales.

La suite comprueba autenticación, no una matriz completa de autorización por rol/entidad ni navegación Vue. Consumidores anónimos anteriores de endpoints protegidos deben adaptarse a enviar JWT; el flujo de registro de operadores requiere revisión. Persisten advertencias NuGet y una incidencia de migración recuperada por reintento; no se presentan como corregidas.

Los registros reales se conservan en evidence/. Los resultados solo acreditan los 14 casos enumerados. La publicación del código no implica despliegue de la aplicación.
