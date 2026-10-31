using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Model.Commands;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Model.Errors;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Application.Patterns;

namespace TechnoByteLambders.MediTrackSensor.Platform.Establishments.Application.CommandServices;

public interface IEstablishmentCommandService
{
    Task<Result<Establishment, EstablishmentsError>> Handle(
        CreateEstablishmentCommand command,
        CancellationToken cancellationToken = default);
    
    Task<Result<bool, string>> DeleteAsync(int id, CancellationToken cancellationToken = default);
}