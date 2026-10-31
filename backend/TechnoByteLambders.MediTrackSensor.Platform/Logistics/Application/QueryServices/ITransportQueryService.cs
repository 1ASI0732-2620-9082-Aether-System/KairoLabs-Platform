using TechnoByteLambders.MediTrackSensor.Platform.Logistics.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Logistics.Domain.Model.Queries;

namespace TechnoByteLambders.MediTrackSensor.Platform.Logistics.Application.QueryServices;

public interface ITransportQueryService
{
    Task<IEnumerable<Transport>> Handle(GetAllTransportsQuery query, CancellationToken cancellationToken = default);
}
