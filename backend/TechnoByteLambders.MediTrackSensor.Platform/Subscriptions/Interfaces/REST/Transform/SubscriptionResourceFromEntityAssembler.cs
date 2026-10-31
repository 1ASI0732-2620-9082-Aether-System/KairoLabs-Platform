using TechnoByteLambders.MediTrackSensor.Platform.Subscriptions.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Subscriptions.Interfaces.REST.Resources;

namespace TechnoByteLambders.MediTrackSensor.Platform.Subscriptions.Interfaces.REST.Transform;

public static class SubscriptionResourceFromEntityAssembler
{
    public static SubscriptionResource ToResourceFromEntity(Subscription s) =>
        new(s.Id, s.Plan.ToString(), s.Status.ToString(), s.StartDate, s.EndDate, s.AdminId.Value, s.CreatedAt);
}
