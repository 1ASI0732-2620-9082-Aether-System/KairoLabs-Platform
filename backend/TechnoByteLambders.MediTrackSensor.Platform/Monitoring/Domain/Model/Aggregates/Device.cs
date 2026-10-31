using TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Domain.Model.Commands;
using TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Domain.Model.ValueObjects;

namespace TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Domain.Model.Aggregates;

public partial class Device(
    string exactLocation,
    TypeOfMedication typeOfMedication,
    SensorReading sensorReading,
    DoorStatus doorStatus,
    EstablishmentId establishmentId,
    string enabledSensors = "")
{
    public Device() : this(
        string.Empty,
        TypeOfMedication.General,
        new SensorReading(0, 0, 0, 0, 0, 0, 0),
        DoorStatus.Closed,
        new EstablishmentId(0))
    {
    }

    public Device(CreateDeviceCommand command) : this(
        command.ExactLocation,
        command.TypeOfMedication,
        new SensorReading(0, 0, 0, 0, 0, 0, 0),
        DoorStatus.Closed,
        new EstablishmentId(command.EstablishmentId),
        command.EnabledSensors)
    {
    }

    public int Id { get; }
    public string ExactLocation { get; private set; } = exactLocation;
    public TypeOfMedication TypeOfMedication { get; private set; } = typeOfMedication;
    public SensorReading SensorReading { get; private set; } = sensorReading;
    public DoorStatus DoorStatus { get; private set; } = doorStatus;
    public EstablishmentId EstablishmentId { get; private set; } = establishmentId;
    public string EnabledSensors { get; private set; } = enabledSensors;

    public Device UpdateSensorReading(SensorReading sensorReading)
    {
        SensorReading = sensorReading;
        return this;
    }

    public Device UpdateDoorStatus(DoorStatus doorStatus)
    {
        DoorStatus = doorStatus;
        return this;
    }
}