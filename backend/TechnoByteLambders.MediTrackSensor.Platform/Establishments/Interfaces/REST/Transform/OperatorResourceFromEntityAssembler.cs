using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Interfaces.REST.Resources;

namespace TechnoByteLambders.MediTrackSensor.Platform.Establishments.Interfaces.REST.Transform;

public static class OperatorResourceFromEntityAssembler
{
    public static OperatorResource ToResourceFromEntity(Operator op) =>
        new(op.Id, op.AlertsAnswered, op.Schedule, op.EstablishmentId, op.UserId.Value);
}
