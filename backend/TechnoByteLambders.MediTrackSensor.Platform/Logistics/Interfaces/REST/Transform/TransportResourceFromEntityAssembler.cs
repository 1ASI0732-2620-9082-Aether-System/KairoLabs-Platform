using TechnoByteLambders.MediTrackSensor.Platform.Logistics.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Logistics.Interfaces.REST.Resources;

namespace TechnoByteLambders.MediTrackSensor.Platform.Logistics.Interfaces.REST.Transform;

public static class TransportResourceFromEntityAssembler
{
    public static TransportResource ToResourceFromEntity(Transport t) =>
        new(t.Id, t.TypeOfTransport, t.TypeOfMedication.ToString(), t.DoorStatus.ToString(),
            t.SensorReading.Temperature, t.SensorReading.Humidity,
            t.SensorReading.LightIntensity, t.SensorReading.AirQuality,
            t.SensorReading.Vibration, t.SensorReading.AtmosphericPressure,
            t.SensorReading.SuspendedParticles,
            t.EstablishmentId.Value, t.CreatedAt, t.EnabledSensors);
}
