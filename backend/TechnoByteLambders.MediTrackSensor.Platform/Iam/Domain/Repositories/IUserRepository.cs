using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Domain.Repositories;

namespace TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Repositories;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
}