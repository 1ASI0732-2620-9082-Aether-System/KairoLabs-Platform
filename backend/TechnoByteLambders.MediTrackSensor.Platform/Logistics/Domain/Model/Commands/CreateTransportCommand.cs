using TechnoByteLambders.MediTrackSensor.Platform.Logistics.Domain.Model.ValueObjects;

namespace TechnoByteLambders.MediTrackSensor.Platform.Logistics.Domain.Model.Commands;

public record CreateTransportCommand(
    string TypeOfTransport,
    TypeOfMedication TypeOfMedication,
    int EstablishmentId,
    string EnabledSensors = "");
