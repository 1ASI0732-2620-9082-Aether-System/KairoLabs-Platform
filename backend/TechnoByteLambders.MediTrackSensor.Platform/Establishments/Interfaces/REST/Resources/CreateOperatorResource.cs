namespace TechnoByteLambders.MediTrackSensor.Platform.Establishments.Interfaces.REST.Resources;

public record CreateOperatorResource(string Schedule, int EstablishmentId, int UsersId);
