namespace TechnoByteLambders.MediTrackSensor.Platform.Logistics.Interfaces.REST.Resources;

public record TransportResource(
    int Id,
    string TypeOfTransport,
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
