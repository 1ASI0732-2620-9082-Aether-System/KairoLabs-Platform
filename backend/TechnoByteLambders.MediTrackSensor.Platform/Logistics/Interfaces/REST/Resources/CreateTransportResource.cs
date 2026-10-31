namespace TechnoByteLambders.MediTrackSensor.Platform.Logistics.Interfaces.REST.Resources;

public record CreateTransportResource(
    string TypeOfTransport,
    string TypeOfMedication,
    int EstablishmentId,
    string EnabledSensors = "");
