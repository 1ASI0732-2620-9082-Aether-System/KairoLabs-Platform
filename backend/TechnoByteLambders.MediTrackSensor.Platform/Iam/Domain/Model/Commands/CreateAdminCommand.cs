namespace TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.Commands;

public record CreateAdminCommand(
    string EntityName,
    string EntityCode,
    string Schedule,
    int UserId);
