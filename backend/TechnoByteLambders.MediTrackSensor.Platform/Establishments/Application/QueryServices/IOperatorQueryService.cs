using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Model.Queries;

namespace TechnoByteLambders.MediTrackSensor.Platform.Establishments.Application.QueryServices;

public interface IOperatorQueryService
{
    Task<IEnumerable<Operator>> Handle(GetAllOperatorsQuery query, CancellationToken cancellationToken = default);
}
