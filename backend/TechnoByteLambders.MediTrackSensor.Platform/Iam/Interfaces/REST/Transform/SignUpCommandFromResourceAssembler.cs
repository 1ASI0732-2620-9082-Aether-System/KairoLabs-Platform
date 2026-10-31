using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.Commands;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Interfaces.REST.Resources;

namespace TechnoByteLambders.MediTrackSensor.Platform.Iam.Interfaces.REST.Transform;

public static class SignUpCommandFromResourceAssembler
{
    public static SignUpCommand ToCommandFromResource(SignUpResource resource) =>
        new(resource.Name, resource.Dni, resource.Email, resource.Phone,
            resource.JobTitle, resource.EntryDate, resource.Role,
            resource.Password, resource.Photo);
}
