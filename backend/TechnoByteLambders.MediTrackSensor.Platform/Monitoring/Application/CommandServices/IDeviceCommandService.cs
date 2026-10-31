using TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Domain.Model.Commands;
using TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Domain.Model.Errors;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Application.Patterns;

namespace TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Application.CommandServices;

public interface IDeviceCommandService
{
    Task<Result<Device, string>> Handle(CreateDeviceCommand command, CancellationToken cancellationToken = default);
    
    Task<Result<Device, MonitoringError>> Handle(
        UpdateDeviceSensorDataCommand command,
        CancellationToken cancellationToken = default);

    Task<Result<bool, string>> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
