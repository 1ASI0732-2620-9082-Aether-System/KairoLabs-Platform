using Microsoft.EntityFrameworkCore;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.ValueObjects;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Repositories;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Infrastructure.Persistence.EFC.Configuration;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace TechnoByteLambders.MediTrackSensor.Platform.Iam.Infrastructure.Persistence.EFC.Repositories;

public class AdminRepository(AppDbContext context)
    : BaseRepository<Admin>(context), IAdminRepository
{
    public async Task<Admin?> FindByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Admin>()
            .FirstOrDefaultAsync(a => a.UserId == new UserId(userId), cancellationToken);
    }

    public async Task<bool> ExistsByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Admin>()
            .AnyAsync(a => a.UserId == new UserId(userId), cancellationToken);
    }
}
