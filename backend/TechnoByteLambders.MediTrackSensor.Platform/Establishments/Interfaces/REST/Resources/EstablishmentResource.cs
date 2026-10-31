namespace TechnoByteLambders.MediTrackSensor.Platform.Establishments.Interfaces.REST.Resources;

public record EstablishmentResource(
    int Id,
    string EstablishmentName,
    string EstablishmentType,
    string Address,
    string District,
    string CityRegion,
    string Country,
    string Phone,
    string Email,
    string Website,
    decimal Latitude,
    decimal Longitude,
    int AdminId,
    DateTimeOffset? CreatedAt);
