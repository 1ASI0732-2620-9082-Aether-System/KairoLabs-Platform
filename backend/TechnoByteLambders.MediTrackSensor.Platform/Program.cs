using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Application.CommandServices;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Application.Internal.CommandServices;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Application.Internal.QueryServices;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Application.QueryServices;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Repositories;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Infrastructure.Persistence.EFC.Repositories;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Application.CommandServices;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Application.Internal.CommandServices;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Application.Internal.OutboundServices;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Application.Internal.QueryServices;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Application.QueryServices;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Repositories;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Infrastructure.OutboundServices;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Infrastructure.Persistence.EFC.Repositories;
using TechnoByteLambders.MediTrackSensor.Platform.Logistics.Application.CommandServices;
using TechnoByteLambders.MediTrackSensor.Platform.Logistics.Application.Internal.CommandServices;
using TechnoByteLambders.MediTrackSensor.Platform.Logistics.Application.Internal.QueryServices;
using TechnoByteLambders.MediTrackSensor.Platform.Logistics.Application.QueryServices;
using TechnoByteLambders.MediTrackSensor.Platform.Logistics.Domain.Repositories;
using TechnoByteLambders.MediTrackSensor.Platform.Logistics.Infrastructure.Persistence.EFC.Repositories;
using TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Application.CommandServices;
using TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Application.Internal.CommandServices;
using TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Application.Internal.QueryServices;
using TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Application.QueryServices;
using TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Domain.Repositories;
using TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Infrastructure.Persistence.EFC.Repositories;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Domain.Repositories;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Infrastructure.Interfaces.ASP.Configuration;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Infrastructure.Persistence;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Infrastructure.Persistence.EFC.Configuration;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Infrastructure.Persistence.EFC.Configuration.Extensions;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Infrastructure.Persistence.EFC.Repositories;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Infrastructure.Pipeline.Middleware.Extensions;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Resources;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Resources.Errors;
using TechnoByteLambders.MediTrackSensor.Platform.Subscriptions.Application.CommandServices;
using TechnoByteLambders.MediTrackSensor.Platform.Subscriptions.Application.Internal.CommandServices;
using TechnoByteLambders.MediTrackSensor.Platform.Subscriptions.Application.Internal.QueryServices;
using TechnoByteLambders.MediTrackSensor.Platform.Subscriptions.Application.QueryServices;
using TechnoByteLambders.MediTrackSensor.Platform.Subscriptions.Domain.Repositories;
using TechnoByteLambders.MediTrackSensor.Platform.Subscriptions.Infrastructure.Persistence.EFC.Repositories;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Infrastructure.OpenApi;
using ProblemDetailsFactory = TechnoByteLambders.MediTrackSensor.Platform.Shared.Interfaces.REST.ProblemDetails.ProblemDetailsFactory;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddControllers(options => options.Conventions.Add(new KebabCaseRouteNamingConvention()))
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    })
    .AddDataAnnotationsLocalization();

builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "MediTrack Sensor API",
        Version = "v1"
    });
    options.AddServer(new Microsoft.OpenApi.Models.OpenApiServer
    {
        Url = "https://medi-track-sensor-platform.onrender.com",
        Description = "Render (producción)"
    });
    options.OperationFilter<SwaggerEndpointDocumentationFilter>();
    options.DocumentFilter<SwaggerInfrastructureFilter>();
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "http://localhost:4173",
                "https://kairo-labs-frontend.vercel.app",
                "https://medi-track-sensor-frontend.vercel.app"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret is required.");
if (Encoding.UTF8.GetByteCount(jwtSecret) < 32)
    throw new InvalidOperationException("Jwt:Secret must contain at least 32 UTF-8 bytes.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("Jwt:Issuer is required.");
var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("Jwt:Audience is required.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.IncludeErrorDetails = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            RequireSignedTokens = true,
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddSingleton<IStringLocalizer<ErrorMessages>, StringLocalizer<ErrorMessages>>();
builder.Services.AddSingleton<IStringLocalizer<CommonMessages>, StringLocalizer<CommonMessages>>();
builder.Services.AddSingleton<ProblemDetailsFactory>();

builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    var connectionString = DatabaseConnectionResolver.Resolve(builder.Configuration);

    options.UseMySQL(connectionString)
        .UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>())
        .EnableDetailedErrors();

    if (builder.Environment.IsDevelopment())
        options.EnableSensitiveDataLogging();
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// IAM
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAdminRepository, AdminRepository>();
builder.Services.AddScoped<IUserCommandService, UserCommandService>();
builder.Services.AddScoped<IAdminCommandService, AdminCommandService>();
builder.Services.AddScoped<IUserQueryService, UserQueryService>();
builder.Services.AddScoped<IAdminQueryService, AdminQueryService>();
builder.Services.AddScoped<IHashingService, HashingService>();
builder.Services.AddScoped<ITokenService, TokenService>();

// Establishments
builder.Services.AddScoped<IEstablishmentRepository, EstablishmentRepository>();
builder.Services.AddScoped<IOperatorRepository, OperatorRepository>();
builder.Services.AddScoped<IEstablishmentCommandService, EstablishmentCommandService>();
builder.Services.AddScoped<IOperatorCommandService, OperatorCommandService>();
builder.Services.AddScoped<IEstablishmentQueryService, EstablishmentQueryService>();
builder.Services.AddScoped<IOperatorQueryService, OperatorQueryService>();

// Monitoring
builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();
builder.Services.AddScoped<IDeviceCommandService, DeviceCommandService>();
builder.Services.AddScoped<IDeviceQueryService, DeviceQueryService>();

// Logistics
builder.Services.AddScoped<ITransportRepository, TransportRepository>();
builder.Services.AddScoped<ITransportCommandService, TransportCommandService>();
builder.Services.AddScoped<ITransportQueryService, TransportQueryService>();

// Subscriptions
builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
builder.Services.AddScoped<ISubscriptionCommandService, SubscriptionCommandService>();
builder.Services.AddScoped<ISubscriptionQueryService, SubscriptionQueryService>();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "medi-track-sensor-platform" }))
    .AllowAnonymous()
    .ExcludeFromDescription();
app.MapGet("/health/db", async (AppDbContext db, ILogger<Program> logger) =>
{
    try
    {
        await db.Database.OpenConnectionAsync();
        await db.Database.CloseConnectionAsync();

        var userCount = await db.Set<User>().CountAsync();
        return Results.Ok(new { status = "healthy", database = "connected", users = userCount });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database health check failed.");
        return Results.Json(
            new { status = "unhealthy", database = "error", detail = ex.Message },
            statusCode: 503);
    }
}).AllowAnonymous().ExcludeFromDescription();
app.MapGet("/", () => Results.Redirect("/swagger")).AllowAnonymous().ExcludeFromDescription();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    for (var attempt = 1; attempt <= 6; attempt++)
    {
        try
        {
            context.Database.ApplyPendingMigrations();
            logger.LogInformation("Database migrations completed at startup (attempt {Attempt}).", attempt);
            break;
        }
        catch (Exception ex) when (attempt < 6)
        {
            logger.LogWarning(ex, "Database migration attempt {Attempt} failed; retrying in 10s...", attempt);
            Thread.Sleep(TimeSpan.FromSeconds(10));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database migration failed after {Attempt} attempts.", attempt);
        }
    }
}

app.Lifetime.ApplicationStarted.Register(() =>
{
    _ = Task.Run(() =>
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Database.ApplyPendingMigrations();
            logger.LogInformation("Database migrations verified after startup.");
        }
        catch (Exception ex)
        {
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Database migration verification failed.");
        }
    });
});

app.UseGlobalExceptionHandler();

var supportedCultures = new[] { "en", "es" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);
app.UseRequestLocalization(localizationOptions);

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "MediTrack Sensor API v1");
    options.RoutePrefix = "swagger";
    options.DocumentTitle = "MediTrack Sensor — REST API";
    options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
    options.DisplayRequestDuration();
});
if (app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.UseCors("FrontendPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
