using TechnoByteLambders.MediTrackSensor.Platform.Logistics.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Logistics.Domain.Model.Commands;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Application.Patterns;

namespace TechnoByteLambders.MediTrackSensor.Platform.Logistics.Application.CommandServices;

public interface ITransportCommandService
{
    Task<Result<Transport, string>> Handle(CreateTransportCommand command, CancellationToken cancellationToken = default);
    Task<Result<Transport, string>> Handle(UpdateTransportSensorDataCommand command, CancellationToken cancellationToken = default);
    Task<Result<bool, string>> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
