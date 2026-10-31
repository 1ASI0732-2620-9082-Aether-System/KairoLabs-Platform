using TechnoByteLambders.MediTrackSensor.Platform.Shared.Domain.Model;

namespace TechnoByteLambders.MediTrackSensor.Platform.Subscriptions.Domain.Model.Errors;

public static class SubscriptionsErrors
{
    public static readonly Error SubscriptionNotFound = new("Subscriptions.SubscriptionNotFound", "Subscription not found.");
    public static readonly Error SubscriptionAlreadyExists = new("Subscriptions.SubscriptionAlreadyExists", "This admin already has an active subscription.");
    public static readonly Error SubscriptionCreationFailed = new("Subscriptions.SubscriptionCreationFailed", "An error occurred while creating the subscription.");
    public static readonly Error SubscriptionUpdateFailed = new("Subscriptions.SubscriptionUpdateFailed", "An error occurred while updating the subscription.");
}
