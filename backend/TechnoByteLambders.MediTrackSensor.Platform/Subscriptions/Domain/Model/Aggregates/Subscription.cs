using TechnoByteLambders.MediTrackSensor.Platform.Subscriptions.Domain.Model.Commands;
using TechnoByteLambders.MediTrackSensor.Platform.Subscriptions.Domain.Model.ValueObjects;

namespace TechnoByteLambders.MediTrackSensor.Platform.Subscriptions.Domain.Model.Aggregates;

public partial class Subscription(
    SubscriptionPlan plan,
    DateOnly startDate,
    DateOnly endDate,
    SubscriptionStatus status,
    AdminId adminId)
{
    public Subscription() : this(
        SubscriptionPlan.Basic,
        DateOnly.MinValue,
        DateOnly.MinValue,
        SubscriptionStatus.Pending,
        new AdminId(0))
    {
    }

    public Subscription(CreateSubscriptionCommand command) : this(
        command.Plan,
        command.StartDate,
        command.EndDate,
        SubscriptionStatus.Pending,
        new AdminId(command.AdminId))
    {
    }

    public int Id { get; }
    public SubscriptionPlan Plan { get; private set; } = plan;
    public DateOnly StartDate { get; private set; } = startDate;
    public DateOnly EndDate { get; private set; } = endDate;
    public SubscriptionStatus Status { get; private set; } = status;
    public AdminId AdminId { get; private set; } = adminId;
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
