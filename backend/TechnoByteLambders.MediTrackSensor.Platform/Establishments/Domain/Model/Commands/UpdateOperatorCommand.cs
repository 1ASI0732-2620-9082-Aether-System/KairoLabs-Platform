namespace TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Model.Commands;

public record UpdateOperatorCommand(int Id, string Schedule, int EstablishmentId);
