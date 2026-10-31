using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Application.CommandServices;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Model.Commands;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Model.Errors;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Model.ValueObjects;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Repositories;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Application.Patterns;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Domain.Repositories;

namespace TechnoByteLambders.MediTrackSensor.Platform.Establishments.Application.Internal.CommandServices;

public class OperatorCommandService(
    IOperatorRepository operatorRepository,
    IEstablishmentRepository establishmentRepository,
    IUnitOfWork unitOfWork) : IOperatorCommandService
{
    public async Task<Result<Operator, EstablishmentsError>> Handle(
        CreateOperatorCommand command,
        CancellationToken cancellationToken = default)
    {
        var establishment = await establishmentRepository.FindByIdAsync(command.EstablishmentId, cancellationToken);
        if (establishment is null)
            return new Result<Operator, EstablishmentsError>.Failure(EstablishmentsError.EstablishmentNotFound);

        var op = new Operator(0, command.Schedule, command.EstablishmentId, new UserId(command.UserId));

        try
        {
            await operatorRepository.AddAsync(op, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
            return new Result<Operator, EstablishmentsError>.Success(op);
        }
        catch (Exception)
        {
            return new Result<Operator, EstablishmentsError>.Failure(EstablishmentsError.OperatorCreationFailed);
        }
    }

    public async Task<Result<Operator, EstablishmentsError>> Handle(
        UpdateOperatorCommand command,
        CancellationToken cancellationToken = default)
    {
        var op = await operatorRepository.FindByIdAsync(command.Id, cancellationToken);
        if (op is null)
            return new Result<Operator, EstablishmentsError>.Failure(EstablishmentsError.OperatorNotFound);

        var establishment = await establishmentRepository.FindByIdAsync(command.EstablishmentId, cancellationToken);
        if (establishment is null)
            return new Result<Operator, EstablishmentsError>.Failure(EstablishmentsError.EstablishmentNotFound);

        op.UpdateSchedule(command.Schedule).UpdateEstablishment(command.EstablishmentId);

        try
        {
            operatorRepository.Update(op);
            await unitOfWork.CompleteAsync(cancellationToken);
            return new Result<Operator, EstablishmentsError>.Success(op);
        }
        catch (Exception)
        {
            return new Result<Operator, EstablishmentsError>.Failure(EstablishmentsError.OperatorUpdateFailed);
        }
    }

    public async Task<Result<bool, EstablishmentsError>> Handle(
        DeleteOperatorCommand command,
        CancellationToken cancellationToken = default)
    {
        var op = await operatorRepository.FindByIdAsync(command.Id, cancellationToken);
        if (op is null)
            return new Result<bool, EstablishmentsError>.Failure(EstablishmentsError.OperatorNotFound);

        try
        {
            operatorRepository.Remove(op);
            await unitOfWork.CompleteAsync(cancellationToken);
            return new Result<bool, EstablishmentsError>.Success(true);
        }
        catch (Exception)
        {
            return new Result<bool, EstablishmentsError>.Failure(EstablishmentsError.OperatorDeleteFailed);
        }
    }

    public async Task<Result<Operator, EstablishmentsError>> Handle(
        IncrementOperatorAlertCommand command,
        CancellationToken cancellationToken = default)
    {
        var op = await operatorRepository.FindByIdAsync(command.Id, cancellationToken);
        if (op is null)
            return new Result<Operator, EstablishmentsError>.Failure(EstablishmentsError.OperatorNotFound);

        op.IncrementAlertsAnswered();

        try
        {
            operatorRepository.Update(op);
            await unitOfWork.CompleteAsync(cancellationToken);
            return new Result<Operator, EstablishmentsError>.Success(op);
        }
        catch (Exception)
        {
            return new Result<Operator, EstablishmentsError>.Failure(EstablishmentsError.OperatorUpdateFailed);
        }
    }
}
