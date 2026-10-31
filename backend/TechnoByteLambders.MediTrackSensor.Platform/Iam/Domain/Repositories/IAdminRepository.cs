using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Domain.Repositories;

namespace TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Repositories;

public interface IAdminRepository : IBaseRepository<Admin>
{
    Task<Admin?> FindByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByUserIdAsync(int userId, CancellationToken cancellationToken = default);
}
