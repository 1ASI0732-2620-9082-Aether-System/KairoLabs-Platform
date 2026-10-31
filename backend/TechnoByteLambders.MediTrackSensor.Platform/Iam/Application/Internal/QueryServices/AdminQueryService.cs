using TechnoByteLambders.MediTrackSensor.Platform.Iam.Application.QueryServices;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.Queries;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Repositories;

namespace TechnoByteLambders.MediTrackSensor.Platform.Iam.Application.Internal.QueryServices;

public class AdminQueryService(IAdminRepository adminRepository) : IAdminQueryService
{
    public async Task<IEnumerable<Admin>> Handle(GetAllAdminsQuery query, CancellationToken cancellationToken = default)
        => await adminRepository.ListAsync(cancellationToken);
}
