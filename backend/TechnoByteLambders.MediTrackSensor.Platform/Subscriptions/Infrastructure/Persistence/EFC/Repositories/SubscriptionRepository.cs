using TechnoByteLambders.MediTrackSensor.Platform.Shared.Infrastructure.Persistence.EFC.Configuration;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Infrastructure.Persistence.EFC.Repositories;
using TechnoByteLambders.MediTrackSensor.Platform.Subscriptions.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Subscriptions.Domain.Repositories;

namespace TechnoByteLambders.MediTrackSensor.Platform.Subscriptions.Infrastructure.Persistence.EFC.Repositories;

public class SubscriptionRepository(AppDbContext context)
    : BaseRepository<Subscription>(context), ISubscriptionRepository;
