using Microsoft.EntityFrameworkCore;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.ValueObjects;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Repositories;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Infrastructure.Persistence.EFC.Configuration;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace TechnoByteLambders.MediTrackSensor.Platform.Iam.Infrastructure.Persistence.EFC.Repositories;

public class UserRepository(AppDbContext context)
    : BaseRepository<User>(context), IUserRepository
{
    public async Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.ToLowerInvariant().Trim();
        return await Context.Set<User>()
            .FirstOrDefaultAsync(u => u.Email == new Email(normalized), cancellationToken);
    }
    
    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.ToLowerInvariant().Trim();
        return await Context.Set<User>()
            .AnyAsync(u => u.Email == new Email(normalized), cancellationToken);
    }
}