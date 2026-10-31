using TechnoByteLambders.MediTrackSensor.Platform.Shared.Domain.Model;

namespace TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Model.Aggregates;

public partial class Establishment : IAuditableEntity
{
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
