namespace TechnoByteLambders.MediTrackSensor.Platform.Subscriptions.Interfaces.REST.Resources;

public record CreateNestedSubscriptionResource(
    string Plan,
    DateOnly StartDate,
    DateOnly EndDate);
