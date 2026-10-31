namespace TechnoByteLambders.MediTrackSensor.Platform.Subscriptions.Interfaces.REST.Resources;

public record CreateSubscriptionResource(
    string Plan,
    DateOnly StartDate,
    DateOnly EndDate,
    int AdminId);
