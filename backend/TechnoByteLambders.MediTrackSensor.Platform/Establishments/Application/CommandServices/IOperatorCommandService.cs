using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Model.Commands;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Model.Errors;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Application.Patterns;

namespace TechnoByteLambders.MediTrackSensor.Platform.Establishments.Application.CommandServices;

public interface IOperatorCommandService
{
    Task<Result<Operator, EstablishmentsError>> Handle(
        CreateOperatorCommand command,
        CancellationToken cancellationToken = default);

    Task<Result<Operator, EstablishmentsError>> Handle(
        UpdateOperatorCommand command,
        CancellationToken cancellationToken = default);

    Task<Result<bool, EstablishmentsError>> Handle(
        DeleteOperatorCommand command,
        CancellationToken cancellationToken = default);

    Task<Result<Operator, EstablishmentsError>> Handle(
        IncrementOperatorAlertCommand command,
        CancellationToken cancellationToken = default);
}
