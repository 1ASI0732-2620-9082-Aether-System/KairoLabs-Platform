namespace TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Domain.Model.ValueObjects;

public record SensorReading(
    decimal Temperature,
    decimal Humidity,
    decimal LightIntensity,
    decimal AirQuality,
    decimal Vibration,
    decimal AtmosphericPressure,
    decimal SuspendedParticles);
