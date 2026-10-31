using TechnoByteLambders.MediTrackSensor.Platform.Shared.Application.Patterns;
using TechnoByteLambders.MediTrackSensor.Platform.Subscriptions.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Subscriptions.Domain.Model.Commands;

namespace TechnoByteLambders.MediTrackSensor.Platform.Subscriptions.Application.CommandServices;

public interface ISubscriptionCommandService
{
    Task<Result<Subscription, string>> Handle(CreateSubscriptionCommand command, CancellationToken cancellationToken = default);
    Task<Result<bool, string>> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
