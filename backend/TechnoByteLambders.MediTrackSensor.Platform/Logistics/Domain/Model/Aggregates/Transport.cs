using TechnoByteLambders.MediTrackSensor.Platform.Logistics.Domain.Model.Commands;
using TechnoByteLambders.MediTrackSensor.Platform.Logistics.Domain.Model.ValueObjects;

namespace TechnoByteLambders.MediTrackSensor.Platform.Logistics.Domain.Model.Aggregates;

public partial class Transport(
    string typeOfTransport,
    TypeOfMedication typeOfMedication,
    SensorReading sensorReading,
    DoorStatus doorStatus,
    EstablishmentId establishmentId,
    string enabledSensors = "")
{
    public Transport() : this(
        string.Empty,
        TypeOfMedication.General,
        new SensorReading(0, 0, 0, 0, 0, 0, 0),
        DoorStatus.Closed,
        new EstablishmentId(0))
    {
    }

    public Transport(CreateTransportCommand command) : this(
        command.TypeOfTransport,
        command.TypeOfMedication,
        new SensorReading(0, 0, 0, 0, 0, 0, 0),
        DoorStatus.Closed,
        new EstablishmentId(command.EstablishmentId),
        command.EnabledSensors)
    {
    }

    public int Id { get; }
    public string TypeOfTransport { get; private set; } = typeOfTransport;
    public TypeOfMedication TypeOfMedication { get; private set; } = typeOfMedication;
    public SensorReading SensorReading { get; private set; } = sensorReading;
    public DoorStatus DoorStatus { get; private set; } = doorStatus;
    public EstablishmentId EstablishmentId { get; private set; } = establishmentId;
    public string EnabledSensors { get; private set; } = enabledSensors;

    public Transport UpdateSensorReading(SensorReading sensorReading)
    {
        SensorReading = sensorReading;
        return this;
    }

    public Transport UpdateDoorStatus(DoorStatus doorStatus)
    {
        DoorStatus = doorStatus;
        return this;
    }
}
