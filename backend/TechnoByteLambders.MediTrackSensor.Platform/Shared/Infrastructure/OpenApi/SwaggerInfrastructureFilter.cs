using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TechnoByteLambders.MediTrackSensor.Platform.Shared.Infrastructure.OpenApi;

/// <summary>
/// Removes infrastructure minimal-API paths from Swagger so only REST controllers are shown.
/// </summary>
public sealed class SwaggerInfrastructureFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var pathsToRemove = swaggerDoc.Paths.Keys
            .Where(p => p is "/" or "/health" or "/health/db")
            .ToList();

        foreach (var path in pathsToRemove)
            swaggerDoc.Paths.Remove(path);
    }
}
