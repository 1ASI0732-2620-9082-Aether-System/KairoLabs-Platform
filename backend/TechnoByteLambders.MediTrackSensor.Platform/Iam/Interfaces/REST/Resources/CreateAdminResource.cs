namespace TechnoByteLambders.MediTrackSensor.Platform.Iam.Interfaces.REST.Resources;

public record CreateAdminResource(string EntityName, string EntityCode, string Schedule, int UserId);
