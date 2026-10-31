namespace TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Interfaces.REST.Resources;

public record DeviceResource(
    int Id,
    string ExactLocation,
    string TypeOfMedication,
    string DoorStatus,
    decimal Temperature,
    decimal Humidity,
    decimal LightIntensity,
    decimal AirQuality,
    decimal Vibration,
    decimal AtmosphericPressure,
    decimal SuspendedParticles,
    int EstablishmentId,
    DateTimeOffset? CreatedAt,
    string EnabledSensors);
