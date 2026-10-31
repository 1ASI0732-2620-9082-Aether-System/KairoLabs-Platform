using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Interfaces.REST.Resources;

namespace TechnoByteLambders.MediTrackSensor.Platform.Iam.Interfaces.REST.Transform;

public static class UserResourceFromEntityAssembler
{
    public static UserResource ToResourceFromEntity(User user) =>
        new(
            user.Id,
            user.Name,
            user.Dni.Value,
            user.Email.Value,
            user.Phone,
            user.JobTitle,
            user.EntryDate,
            user.Role,
            user.Photo,
            user.CreatedAt);
}
