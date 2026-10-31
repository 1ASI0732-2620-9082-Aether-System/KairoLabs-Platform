namespace TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Interfaces.REST.Resources;

public record CreateNestedDeviceResource(
    string ExactLocation,
    string TypeOfMedication,
    string EnabledSensors = "");
