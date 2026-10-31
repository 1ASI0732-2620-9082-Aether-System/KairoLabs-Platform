namespace TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Interfaces.REST.Resources;

public record UpdateDeviceSensorDataResource(
    decimal Temperature,
    decimal Humidity,
    decimal LightIntensity,
    decimal AirQuality,
    decimal Vibration,
    decimal AtmosphericPressure,
    decimal SuspendedParticles,
    string DoorStatus);
