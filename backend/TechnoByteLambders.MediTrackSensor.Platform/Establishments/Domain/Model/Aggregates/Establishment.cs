using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Model.ValueObjects;

namespace TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Model.Aggregates;

public partial class Establishment(
    string establishmentName,
    EstablishmentType establishmentType,
    Address address,
    Location location,
    string phone,
    string email,
    string website,
    AdminId adminId)
{
    public Establishment() : this(
        string.Empty,
        EstablishmentType.Warehouse,
        new Address(string.Empty, string.Empty, string.Empty, string.Empty),
        new Location(0, 0),
        string.Empty,
        string.Empty,
        string.Empty,
        new AdminId(0))
    {
    }

    public int Id { get; }
    public string EstablishmentName { get; private set; } = establishmentName;
    public EstablishmentType EstablishmentType { get; private set; } = establishmentType;
    public Address Address { get; private set; } = address;
    public Location Location { get; private set; } = location;
    public string Phone { get; private set; } = phone;
    public string Email { get; private set; } = email;
    public string Website { get; private set; } = website;
    public AdminId AdminId { get; private set; } = adminId;
}
