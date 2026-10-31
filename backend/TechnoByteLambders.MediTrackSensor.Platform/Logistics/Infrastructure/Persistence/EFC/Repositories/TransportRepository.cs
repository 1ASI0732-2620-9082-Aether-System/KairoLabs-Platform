using Microsoft.EntityFrameworkCore;
using TechnoByteLambders.MediTrackSensor.Platform.Logistics.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Logistics.Domain.Model.ValueObjects;
using TechnoByteLambders.MediTrackSensor.Platform.Logistics.Domain.Repositories;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Infrastructure.Persistence.EFC.Configuration;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace TechnoByteLambders.MediTrackSensor.Platform.Logistics.Infrastructure.Persistence.EFC.Repositories;

public class TransportRepository(AppDbContext context)
    : BaseRepository<Transport>(context), ITransportRepository
{
    public async Task<IEnumerable<Transport>> FindByEstablishmentIdAsync(
        int establishmentId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<Transport>()
            .Where(t => t.EstablishmentId == new EstablishmentId(establishmentId))
            .ToListAsync(cancellationToken);
    }
}
