using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Model.ValueObjects;

namespace TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Model.Aggregates;

public partial class Operator(int alertsAnswered, string schedule, int establishmentId, UserId userId)
{
    public Operator() : this(0, string.Empty, 0, new UserId(0))
    {
    }

    public int Id { get; }
    public int AlertsAnswered { get; private set; } = alertsAnswered;
    public string Schedule { get; private set; } = schedule;
    public int EstablishmentId { get; private set; } = establishmentId;
    public UserId UserId { get; private set; } = userId;

    public Operator UpdateSchedule(string schedule)
    {
        Schedule = schedule;
        return this;
    }

    public Operator UpdateEstablishment(int establishmentId)
    {
        EstablishmentId = establishmentId;
        return this;
    }

    public Operator IncrementAlertsAnswered()
    {
        AlertsAnswered++;
        return this;
    }
}
