using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.Commands;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.ValueObjects;

namespace TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.Aggregates;

public partial class Admin(string entityName, string entityCode, string schedule, UserId userId)
{
    public Admin() : this(string.Empty, string.Empty, string.Empty, new UserId(0))
    {
    }

    public Admin(CreateAdminCommand command) : this(
        command.EntityName,
        command.EntityCode,
        command.Schedule,
        new UserId(command.UserId))
    {
    }

    public int Id { get; }
    public string EntityName { get; private set; } = entityName;
    public string EntityCode { get; private set; } = entityCode;
    public string Schedule { get; private set; } = schedule;
    public UserId UserId { get; private set; } = userId;

    public Admin UpdateSchedule(string schedule)
    {
        Schedule = schedule;
        return this;
    }

    public Admin UpdateEntityInfo(string entityName, string entityCode)
    {
        EntityName = entityName;
        EntityCode = entityCode;
        return this;
    }
}
