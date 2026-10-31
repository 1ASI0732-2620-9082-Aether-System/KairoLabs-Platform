using TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Domain.Model.Queries;

namespace TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Application.QueryServices;

public interface IDeviceQueryService
{
    Task<IEnumerable<Device>> Handle(GetAllDevicesQuery query, CancellationToken cancellationToken = default);
}
