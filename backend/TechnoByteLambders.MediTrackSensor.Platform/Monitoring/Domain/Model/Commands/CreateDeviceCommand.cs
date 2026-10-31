using TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Domain.Model.ValueObjects;

namespace TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Domain.Model.Commands;

public record CreateDeviceCommand(
    string ExactLocation,
    TypeOfMedication TypeOfMedication,
    int EstablishmentId,
    string EnabledSensors = "");
