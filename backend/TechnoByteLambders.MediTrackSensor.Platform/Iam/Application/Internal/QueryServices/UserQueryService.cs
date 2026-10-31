using TechnoByteLambders.MediTrackSensor.Platform.Iam.Application.QueryServices;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.Queries;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Repositories;

namespace TechnoByteLambders.MediTrackSensor.Platform.Iam.Application.Internal.QueryServices;

public class UserQueryService(IUserRepository userRepository) : IUserQueryService
{
    public async Task<IEnumerable<User>> Handle(GetAllUsersQuery query, CancellationToken cancellationToken = default)
        => await userRepository.ListAsync(cancellationToken);
}
