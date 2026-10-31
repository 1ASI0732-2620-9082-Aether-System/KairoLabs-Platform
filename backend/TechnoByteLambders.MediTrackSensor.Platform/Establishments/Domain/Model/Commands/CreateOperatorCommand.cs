namespace TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Model.Commands;

public record CreateOperatorCommand(string Schedule, int EstablishmentId, int UserId);
