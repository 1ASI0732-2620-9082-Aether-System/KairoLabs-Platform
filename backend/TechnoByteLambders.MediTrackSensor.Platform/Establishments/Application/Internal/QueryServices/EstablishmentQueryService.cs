using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Application.QueryServices;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Model.Queries;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Repositories;

namespace TechnoByteLambders.MediTrackSensor.Platform.Establishments.Application.Internal.QueryServices;

public class EstablishmentQueryService(IEstablishmentRepository establishmentRepository) : IEstablishmentQueryService
{
    public async Task<IEnumerable<Establishment>> Handle(GetAllEstablishmentsQuery query, CancellationToken cancellationToken = default)
        => await establishmentRepository.ListAsync(cancellationToken);
}
