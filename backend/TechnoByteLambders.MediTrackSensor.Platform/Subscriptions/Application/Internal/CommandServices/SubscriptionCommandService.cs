using Microsoft.EntityFrameworkCore;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Application.Patterns;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Domain.Repositories;
using TechnoByteLambders.MediTrackSensor.Platform.Subscriptions.Application.CommandServices;
using TechnoByteLambders.MediTrackSensor.Platform.Subscriptions.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Subscriptions.Domain.Model.Commands;
using TechnoByteLambders.MediTrackSensor.Platform.Subscriptions.Domain.Model.Errors;
using TechnoByteLambders.MediTrackSensor.Platform.Subscriptions.Domain.Repositories;

namespace TechnoByteLambders.MediTrackSensor.Platform.Subscriptions.Application.Internal.CommandServices;

public class SubscriptionCommandService(
    ISubscriptionRepository subscriptionRepository,
    IUnitOfWork unitOfWork) : ISubscriptionCommandService
{
    public async Task<Result<Subscription, string>> Handle(CreateSubscriptionCommand command, CancellationToken cancellationToken = default)
    {
        var subscription = new Subscription(command);
        try
        {
            await subscriptionRepository.AddAsync(subscription, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
            return new Result<Subscription, string>.Success(subscription);
        }
        catch (OperationCanceledException) { return new Result<Subscription, string>.Failure(SubscriptionsErrors.SubscriptionCreationFailed.Description); }
        catch (DbUpdateException) { return new Result<Subscription, string>.Failure(SubscriptionsErrors.SubscriptionCreationFailed.Description); }
        catch (Exception) { return new Result<Subscription, string>.Failure(SubscriptionsErrors.SubscriptionCreationFailed.Description); }
    }

    public async Task<Result<bool, string>> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var subscription = await subscriptionRepository.FindByIdAsync(id, cancellationToken);
        if (subscription is null)
            return new Result<bool, string>.Failure(SubscriptionsErrors.SubscriptionNotFound.Description);

        subscriptionRepository.Remove(subscription);
        await unitOfWork.CompleteAsync(cancellationToken);
        return new Result<bool, string>.Success(true);
    }
}
