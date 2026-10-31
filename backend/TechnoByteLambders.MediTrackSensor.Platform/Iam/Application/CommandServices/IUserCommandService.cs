using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.Commands;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.Errors;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Application.Patterns;

namespace TechnoByteLambders.MediTrackSensor.Platform.Iam.Application.CommandServices;

public interface IUserCommandService
{
    Task<Result<User, string>> Handle(SignUpCommand command, CancellationToken cancellationToken = default);

    Task<Result<User, string>> RegisterHealthEntityAsync(
        SignUpCommand command,
        string entityName,
        CancellationToken cancellationToken = default);
    
    Task<Result<(User User, string Token), IamError>> Handle(
        SignInCommand command,
        CancellationToken cancellationToken = default);
    
    Task<Result<bool, string>> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
