using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.Queries;

namespace TechnoByteLambders.MediTrackSensor.Platform.Iam.Application.QueryServices;

public interface IAdminQueryService
{
    Task<IEnumerable<Admin>> Handle(GetAllAdminsQuery query, CancellationToken cancellationToken = default);
}
