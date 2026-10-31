namespace TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Interfaces.REST.Resources;

public record CreateDeviceResource(
    string ExactLocation,
    string TypeOfMedication,
    int EstablishmentId,
    string EnabledSensors = "");
