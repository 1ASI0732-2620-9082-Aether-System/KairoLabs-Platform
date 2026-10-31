using TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Application.QueryServices;
using TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Domain.Model.Queries;
using TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Domain.Repositories;

namespace TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Application.Internal.QueryServices;

public class DeviceQueryService(IDeviceRepository deviceRepository) : IDeviceQueryService
{
    public async Task<IEnumerable<Device>> Handle(GetAllDevicesQuery query, CancellationToken cancellationToken = default)
        => await deviceRepository.ListAsync(cancellationToken);
}
