using TechnoByteLambders.MediTrackSensor.Platform.Logistics.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Domain.Repositories;

namespace TechnoByteLambders.MediTrackSensor.Platform.Logistics.Domain.Repositories;

public interface ITransportRepository : IBaseRepository<Transport>
{
    Task<IEnumerable<Transport>> FindByEstablishmentIdAsync(int establishmentId, CancellationToken cancellationToken = default);
}
