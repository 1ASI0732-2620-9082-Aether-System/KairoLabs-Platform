using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Interfaces.REST.Resources;

namespace TechnoByteLambders.MediTrackSensor.Platform.Establishments.Interfaces.REST.Transform;

public static class EstablishmentResourceFromEntityAssembler
{
    public static EstablishmentResource ToResourceFromEntity(Establishment establishment) =>
        new(
            establishment.Id,
            establishment.EstablishmentName,
            establishment.EstablishmentType.ToString(),
            establishment.Address.Street,
            establishment.Address.District,
            establishment.Address.CityRegion,
            establishment.Address.Country,
            establishment.Phone,
            establishment.Email,
            establishment.Website,
            establishment.Location.Latitude,
            establishment.Location.Longitude,
            establishment.AdminId.Value,
            establishment.CreatedAt);
}
