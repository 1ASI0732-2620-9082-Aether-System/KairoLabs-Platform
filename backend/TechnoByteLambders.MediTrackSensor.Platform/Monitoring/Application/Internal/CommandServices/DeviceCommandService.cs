using Microsoft.EntityFrameworkCore;
using TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Application.CommandServices;
using TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Domain.Model.Commands;
using TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Domain.Model.Errors;
using TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Domain.Repositories;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Application.Patterns;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Domain.Repositories;

namespace TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Application.Internal.CommandServices;

public class DeviceCommandService(
    IDeviceRepository deviceRepository,
    IUnitOfWork unitOfWork) : IDeviceCommandService
{
    public async Task<Result<Device, string>> Handle(CreateDeviceCommand command, CancellationToken cancellationToken = default)
    {
        var device = new Device(command);
        try
        {
            await deviceRepository.AddAsync(device, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
            return new Result<Device, string>.Success(device);
        }
        catch (OperationCanceledException) { return new Result<Device, string>.Failure(MonitoringErrors.DeviceCreationFailed.Description); }
        catch (DbUpdateException) { return new Result<Device, string>.Failure(MonitoringErrors.DeviceCreationFailed.Description); }
        catch (Exception) { return new Result<Device, string>.Failure(MonitoringErrors.DeviceCreationFailed.Description); }
    }

    public async Task<Result<Device, MonitoringError>> Handle(
        UpdateDeviceSensorDataCommand command,
        CancellationToken cancellationToken = default)
    {
        var device = await deviceRepository.FindByIdAsync(command.Id, cancellationToken);
        if (device is null)
            return new Result<Device, MonitoringError>.Failure(MonitoringError.DeviceNotFound);

        device.UpdateSensorReading(command.SensorReading).UpdateDoorStatus(command.DoorStatus);

        try
        {
            deviceRepository.Update(device);
            await unitOfWork.CompleteAsync(cancellationToken);
            return new Result<Device, MonitoringError>.Success(device);
        }
        catch (Exception)
        {
            return new Result<Device, MonitoringError>.Failure(MonitoringError.DeviceUpdateFailed);
        }
    }

    public async Task<Result<bool, string>> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var device = await deviceRepository.FindByIdAsync(id, cancellationToken);
        if (device is null)
            return new Result<bool, string>.Failure(MonitoringErrors.DeviceNotFound.Description);

        deviceRepository.Remove(device);
        await unitOfWork.CompleteAsync(cancellationToken);
        return new Result<bool, string>.Success(true);
    }
}
