using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Application.QueryServices;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Model.Queries;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Repositories;

namespace TechnoByteLambders.MediTrackSensor.Platform.Establishments.Application.Internal.QueryServices;

public class OperatorQueryService(IOperatorRepository operatorRepository) : IOperatorQueryService
{
    public async Task<IEnumerable<Operator>> Handle(GetAllOperatorsQuery query, CancellationToken cancellationToken = default)
        => await operatorRepository.ListAsync(cancellationToken);
}
