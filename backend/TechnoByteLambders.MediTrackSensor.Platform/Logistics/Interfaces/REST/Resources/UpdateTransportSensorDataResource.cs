namespace TechnoByteLambders.MediTrackSensor.Platform.Logistics.Interfaces.REST.Resources;

public record UpdateTransportSensorDataResource(
    decimal Temperature,
    decimal Humidity,
    decimal LightIntensity,
    decimal AirQuality,
    decimal Vibration,
    decimal AtmosphericPressure,
    decimal SuspendedParticles,
    string DoorStatus);
