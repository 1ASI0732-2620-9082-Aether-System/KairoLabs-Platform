using TechnoByteLambders.MediTrackSensor.Platform.Shared.Domain.Model;

namespace TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Domain.Model.Aggregates;

public partial class Device : IAuditableEntity
{
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
