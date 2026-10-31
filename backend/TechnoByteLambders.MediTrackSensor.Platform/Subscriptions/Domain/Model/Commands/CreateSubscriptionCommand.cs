using TechnoByteLambders.MediTrackSensor.Platform.Subscriptions.Domain.Model.ValueObjects;

namespace TechnoByteLambders.MediTrackSensor.Platform.Subscriptions.Domain.Model.Commands;

public record CreateSubscriptionCommand(
    SubscriptionPlan Plan,
    DateOnly StartDate,
    DateOnly EndDate,
    int AdminId);
