using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Model.Queries;

namespace TechnoByteLambders.MediTrackSensor.Platform.Establishments.Application.QueryServices;

public interface IEstablishmentQueryService
{
    Task<IEnumerable<Establishment>> Handle(GetAllEstablishmentsQuery query, CancellationToken cancellationToken = default);
}
