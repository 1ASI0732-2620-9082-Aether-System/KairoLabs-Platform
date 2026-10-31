using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.Queries;

namespace TechnoByteLambders.MediTrackSensor.Platform.Iam.Application.QueryServices;

public interface IUserQueryService
{
    Task<IEnumerable<User>> Handle(GetAllUsersQuery query, CancellationToken cancellationToken = default);
}
