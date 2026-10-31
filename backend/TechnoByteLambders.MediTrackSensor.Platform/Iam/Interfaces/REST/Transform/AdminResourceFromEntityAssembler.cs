using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Interfaces.REST.Resources;

namespace TechnoByteLambders.MediTrackSensor.Platform.Iam.Interfaces.REST.Transform;

public static class AdminResourceFromEntityAssembler
{
    public static AdminResource ToResourceFromEntity(Admin admin) =>
        new(admin.Id, admin.EntityName, admin.EntityCode, admin.Schedule, admin.UserId.Value);
}
