using TechnoByteLambders.MediTrackSensor.Platform.Shared.Domain.Model;

namespace TechnoByteLambders.MediTrackSensor.Platform.Logistics.Domain.Model.Errors;

public static class LogisticsErrors
{
    public static readonly Error TransportNotFound = new("Logistics.TransportNotFound", "Transport not found.");
    public static readonly Error TransportCreationFailed = new("Logistics.TransportCreationFailed", "An error occurred while creating the transport.");
    public static readonly Error TransportUpdateFailed = new("Logistics.TransportUpdateFailed", "An error occurred while updating the transport sensor data.");
}
