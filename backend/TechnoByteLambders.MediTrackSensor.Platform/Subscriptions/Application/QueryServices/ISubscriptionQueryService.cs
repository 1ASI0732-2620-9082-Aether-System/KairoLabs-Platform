using TechnoByteLambders.MediTrackSensor.Platform.Subscriptions.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Subscriptions.Domain.Model.Queries;

namespace TechnoByteLambders.MediTrackSensor.Platform.Subscriptions.Application.QueryServices;

public interface ISubscriptionQueryService
{
    Task<IEnumerable<Subscription>> Handle(GetAllSubscriptionsQuery query, CancellationToken cancellationToken = default);
}
