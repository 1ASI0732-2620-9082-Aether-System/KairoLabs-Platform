using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Infrastructure.Persistence.EFC.Configuration;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Infrastructure.Persistence.EFC.Configuration;
using TechnoByteLambders.MediTrackSensor.Platform.Logistics.Infrastructure.Persistence.EFC.Configuration;
using TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Infrastructure.Persistence.EFC.Configuration;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Infrastructure.Persistence.EFC.Configuration.Extensions;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Infrastructure.Persistence.EFC.Interceptors;
using TechnoByteLambders.MediTrackSensor.Platform.Subscriptions.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;

namespace TechnoByteLambders.MediTrackSensor.Platform.Shared.Infrastructure.Persistence.EFC.Configuration;

/// <summary>
///     Application database context
/// </summary>
public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    /// <inheritdoc />
    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.AddInterceptors(new AuditableEntityInterceptor());
        base.OnConfiguring(builder);
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // IAM Context
        builder.ApplyIamConfiguration();

        // Establishments Context
        builder.ApplyEstablishmentsConfiguration();

        // Monitoring Context
        builder.ApplyMonitoringConfiguration();

        // Logistics Context
        builder.ApplyLogisticsConfiguration();

        // Subscriptions Context
        builder.ApplySubscriptionsConfiguration();

        builder.UseSnakeCaseNamingConvention();
    }
}
