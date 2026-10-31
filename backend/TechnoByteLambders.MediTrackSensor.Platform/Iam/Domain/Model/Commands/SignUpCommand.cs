using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.ValueObjects;

namespace TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.Commands;

public record SignUpCommand(
    string Name,
    string Dni,
    string Email,
    string Phone,
    string JobTitle,
    DateOnly EntryDate,
    UserRole Role,
    string Password,
    string Photo);
