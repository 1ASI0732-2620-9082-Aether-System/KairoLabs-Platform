using TechnoByteLambders.MediTrackSensor.Platform.Shared.Domain.Repositories;
using TechnoByteLambders.MediTrackSensor.Platform.Subscriptions.Domain.Model.Aggregates;

namespace TechnoByteLambders.MediTrackSensor.Platform.Subscriptions.Domain.Repositories;

public interface ISubscriptionRepository : IBaseRepository<Subscription>;
