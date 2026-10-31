using TechnoByteLambders.MediTrackSensor.Platform.Logistics.Domain.Model.ValueObjects;

namespace TechnoByteLambders.MediTrackSensor.Platform.Logistics.Domain.Model.Commands;

public record UpdateTransportSensorDataCommand(
    int Id,
    SensorReading SensorReading,
    DoorStatus DoorStatus);
