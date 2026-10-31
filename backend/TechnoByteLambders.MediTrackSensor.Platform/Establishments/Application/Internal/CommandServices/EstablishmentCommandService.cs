using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Application.CommandServices;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Model.Commands;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Model.Errors;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Model.ValueObjects;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Repositories;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Application.Patterns;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Domain.Repositories;

namespace TechnoByteLambders.MediTrackSensor.Platform.Establishments.Application.Internal.CommandServices;

public class EstablishmentCommandService(
    IEstablishmentRepository establishmentRepository,
    IUnitOfWork unitOfWork) : IEstablishmentCommandService
{
    public async Task<Result<Establishment, EstablishmentsError>> Handle(
        CreateEstablishmentCommand command,
        CancellationToken cancellationToken = default)
    {
        var establishment = new Establishment(
            command.EstablishmentName,
            command.EstablishmentType,
            command.Address,
            command.Location,
            command.Phone,
            command.Email,
            command.Website,
            new AdminId(command.AdminId));

        try
        {
            await establishmentRepository.AddAsync(establishment, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
            return new Result<Establishment, EstablishmentsError>.Success(establishment);
        }
        catch (Exception)
        {
            return new Result<Establishment, EstablishmentsError>.Failure(EstablishmentsError.EstablishmentCreationFailed);
        }
    }

    public async Task<Result<bool, string>> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var establishment = await establishmentRepository.FindByIdAsync(id, cancellationToken);
        if (establishment is null)
            return new Result<bool, string>.Failure(EstablishmentsErrors.EstablishmentNotFound.Description);

        establishmentRepository.Remove(establishment);
        await unitOfWork.CompleteAsync(cancellationToken);
        return new Result<bool, string>.Success(true);
    }
}