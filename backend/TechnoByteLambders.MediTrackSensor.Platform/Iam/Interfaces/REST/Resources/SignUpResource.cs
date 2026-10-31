using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.ValueObjects;

namespace TechnoByteLambders.MediTrackSensor.Platform.Iam.Interfaces.REST.Resources;

public record SignUpResource(
    string Name,
    string Dni,
    string Email,
    string Phone,
    string JobTitle,
    DateOnly EntryDate,
    UserRole Role,
    string Password,
    string Photo,
    string? EntityName = null);
