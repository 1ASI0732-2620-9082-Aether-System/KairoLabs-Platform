namespace TechnoByteLambders.MediTrackSensor.Platform.Subscriptions.Interfaces.REST.Resources;

public record SubscriptionResource(
    int Id,
    string Plan,
    string Status,
    DateOnly StartDate,
    DateOnly EndDate,
    int AdminId,
    DateTimeOffset? CreatedAt);
