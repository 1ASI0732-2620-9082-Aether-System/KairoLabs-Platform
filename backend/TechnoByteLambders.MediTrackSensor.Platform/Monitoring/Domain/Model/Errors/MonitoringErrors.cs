using TechnoByteLambders.MediTrackSensor.Platform.Shared.Domain.Model;

namespace TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Domain.Model.Errors;

public static class MonitoringErrors
{
    public static readonly Error DeviceNotFound = new(
        "Monitoring.DeviceNotFound",
        "Device not found.");

    public static readonly Error DeviceUpdateFailed = new(
        "Monitoring.DeviceUpdateFailed",
        "An error occurred while updating the device sensor data.");

    public static readonly Error DeviceCreationFailed = new(
        "Monitoring.DeviceCreationFailed",
        "An error occurred while creating the device.");

    public static readonly Error InternalServerError = new(
        "Monitoring.InternalServerError",
        "An unexpected error occurred.");
}