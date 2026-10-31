namespace TechnoByteLambders.MediTrackSensor.Platform.Logistics.Interfaces.REST.Resources;

public record CreateNestedTransportResource(
    string TypeOfTransport,
    string TypeOfMedication,
    string EnabledSensors = "");
