using TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Interfaces.REST.Resources;

namespace TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Interfaces.REST.Transform;

public static class DeviceResourceFromEntityAssembler
{
    public static DeviceResource ToResourceFromEntity(Device device) =>
        new(
            device.Id,
            device.ExactLocation,
            device.TypeOfMedication.ToString(),
            device.DoorStatus.ToString(),
            device.SensorReading.Temperature,
            device.SensorReading.Humidity,
            device.SensorReading.LightIntensity,
            device.SensorReading.AirQuality,
            device.SensorReading.Vibration,
            device.SensorReading.AtmosphericPressure,
            device.SensorReading.SuspendedParticles,
            device.EstablishmentId.Value,
            device.CreatedAt,
            device.EnabledSensors);
}