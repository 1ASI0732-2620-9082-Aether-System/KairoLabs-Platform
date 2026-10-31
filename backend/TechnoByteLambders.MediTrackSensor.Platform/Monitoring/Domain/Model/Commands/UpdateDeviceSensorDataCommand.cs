using TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Domain.Model.ValueObjects;

namespace TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Domain.Model.Commands;

public record UpdateDeviceSensorDataCommand(
    int Id,
    SensorReading SensorReading,
    DoorStatus DoorStatus);
