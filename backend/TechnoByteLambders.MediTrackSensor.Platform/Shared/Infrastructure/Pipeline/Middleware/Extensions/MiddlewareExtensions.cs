using TechnoByteLambders.MediTrackSensor.Platform.Shared.Infrastructure.Pipeline.Middleware.Components;

namespace TechnoByteLambders.MediTrackSensor.Platform.Shared.Infrastructure.Pipeline.Middleware.Extensions;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
        => builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
}
