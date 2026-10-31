using Microsoft.EntityFrameworkCore;
using TechnoByteLambders.MediTrackSensor.Platform.Logistics.Application.CommandServices;
using TechnoByteLambders.MediTrackSensor.Platform.Logistics.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Logistics.Domain.Model.Commands;
using TechnoByteLambders.MediTrackSensor.Platform.Logistics.Domain.Model.Errors;
using TechnoByteLambders.MediTrackSensor.Platform.Logistics.Domain.Repositories;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Application.Patterns;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Domain.Repositories;

namespace TechnoByteLambders.MediTrackSensor.Platform.Logistics.Application.Internal.CommandServices;

public class TransportCommandService(
    ITransportRepository transportRepository,
    IUnitOfWork unitOfWork) : ITransportCommandService
{
    public async Task<Result<Transport, string>> Handle(CreateTransportCommand command, CancellationToken cancellationToken = default)
    {
        var transport = new Transport(command);
        try
        {
            await transportRepository.AddAsync(transport, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
            return new Result<Transport, string>.Success(transport);
        }
        catch (OperationCanceledException) { return new Result<Transport, string>.Failure(LogisticsErrors.TransportCreationFailed.Description); }
        catch (DbUpdateException) { return new Result<Transport, string>.Failure(LogisticsErrors.TransportCreationFailed.Description); }
        catch (Exception) { return new Result<Transport, string>.Failure(LogisticsErrors.TransportCreationFailed.Description); }
    }

    public async Task<Result<Transport, string>> Handle(UpdateTransportSensorDataCommand command, CancellationToken cancellationToken = default)
    {
        var transport = await transportRepository.FindByIdAsync(command.Id, cancellationToken);
        if (transport is null)
            return new Result<Transport, string>.Failure(LogisticsErrors.TransportNotFound.Description);

        transport.UpdateSensorReading(command.SensorReading).UpdateDoorStatus(command.DoorStatus);
        try
        {
            transportRepository.Update(transport);
            await unitOfWork.CompleteAsync(cancellationToken);
            return new Result<Transport, string>.Success(transport);
        }
        catch (OperationCanceledException) { return new Result<Transport, string>.Failure(LogisticsErrors.TransportUpdateFailed.Description); }
        catch (DbUpdateException) { return new Result<Transport, string>.Failure(LogisticsErrors.TransportUpdateFailed.Description); }
        catch (Exception) { return new Result<Transport, string>.Failure(LogisticsErrors.TransportUpdateFailed.Description); }
    }

    public async Task<Result<bool, string>> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var transport = await transportRepository.FindByIdAsync(id, cancellationToken);
        if (transport is null)
            return new Result<bool, string>.Failure(LogisticsErrors.TransportNotFound.Description);

        transportRepository.Remove(transport);
        await unitOfWork.CompleteAsync(cancellationToken);
        return new Result<bool, string>.Success(true);
    }
}
