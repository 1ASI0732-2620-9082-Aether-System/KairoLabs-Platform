using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Model.ValueObjects;

namespace TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Model.Commands;

public record CreateEstablishmentCommand(
    string EstablishmentName,
    EstablishmentType EstablishmentType,
    Address Address,
    Location Location,
    string Phone,
    string Email,
    string Website,
    int AdminId);
