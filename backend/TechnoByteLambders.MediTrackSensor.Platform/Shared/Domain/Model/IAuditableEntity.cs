namespace TechnoByteLambders.MediTrackSensor.Platform.Shared.Domain.Model;

/// <summary>
///     Marks an entity as carrying audit timestamps managed by the persistence layer.
/// </summary>
/// <remarks>
///     Any entity in any bounded context that implements this interface will automatically
///     have <see cref="CreatedAt"/> set once on first persistence and <see cref="UpdatedAt"/>
///     refreshed on every subsequent save, via <c>AuditableEntityInterceptor</c>.
/// </remarks>
public interface IAuditableEntity
{
    DateTimeOffset? CreatedAt { get; set; }
    DateTimeOffset? UpdatedAt { get; set; }
}
