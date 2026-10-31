using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.ValueObjects;

namespace TechnoByteLambders.MediTrackSensor.Platform.Iam.Interfaces.REST.Resources;

public record UserResource(
    int Id,
    string Name,
    string Dni,
    string Email,
    string Phone,
    string JobTitle,
    DateOnly EntryDate,
    UserRole Role,
    string Photo,
    DateTimeOffset? CreatedAt);
