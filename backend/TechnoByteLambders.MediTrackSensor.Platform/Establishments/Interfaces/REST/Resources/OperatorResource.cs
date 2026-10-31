namespace TechnoByteLambders.MediTrackSensor.Platform.Establishments.Interfaces.REST.Resources;

public record OperatorResource(int Id, int AlertsAnswered, string Schedule, int EstablishmentId, int UsersId);
