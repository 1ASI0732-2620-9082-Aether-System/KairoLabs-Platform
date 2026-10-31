using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Model.ValueObjects;

namespace TechnoByteLambders.MediTrackSensor.Platform.Establishments.Interfaces.REST.Resources;

public record CreateEstablishmentResource(
    string EstablishmentName,
    EstablishmentType EstablishmentType,
    string Address,
    string District,
    string CityRegion,
    string Country,
    decimal Latitude,
    decimal Longitude,
    string Phone,
    string Email,
    string Website,
    int AdminId);
