using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.Commands;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Application.Patterns;

namespace TechnoByteLambders.MediTrackSensor.Platform.Iam.Application.CommandServices;

public interface IAdminCommandService
{
    Task<Result<Admin, string>> Handle(CreateAdminCommand command, CancellationToken cancellationToken = default);
}
