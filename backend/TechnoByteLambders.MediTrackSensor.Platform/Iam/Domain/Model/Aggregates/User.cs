using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.Commands;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.ValueObjects;

namespace TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.Aggregates;

public partial class User(
    string name,
    Dni dni,
    Email email,
    string phone,
    string jobTitle,
    DateOnly entryDate,
    UserRole role,
    string passwordHash,
    string photo)
{
    public User() : this(
        string.Empty,
        new Dni(string.Empty),
        new Email(string.Empty),
        string.Empty,
        string.Empty,
        DateOnly.MinValue,
        UserRole.Operator,
        string.Empty,
        string.Empty)
    {
    }

    public User(SignUpCommand command, string passwordHash) : this(
        command.Name,
        new Dni(command.Dni),
        new Email(command.Email),
        command.Phone,
        command.JobTitle,
        command.EntryDate,
        command.Role,
        passwordHash,
        command.Photo)
    {
    }

    public int Id { get; }
    public string Name { get; private set; } = name;
    public Dni Dni { get; private set; } = dni;
    public Email Email { get; private set; } = email;
    public string Phone { get; private set; } = phone;
    public string JobTitle { get; private set; } = jobTitle;
    public DateOnly EntryDate { get; private set; } = entryDate;
    public UserRole Role { get; private set; } = role;
    public string PasswordHash { get; private set; } = passwordHash;
    public string Photo { get; private set; } = photo;

    public User UpdateProfile(string name, string phone, string jobTitle, string photo)
    {
        Name = name;
        Phone = phone;
        JobTitle = jobTitle;
        Photo = photo;
        return this;
    }

    public User UpdatePasswordHash(string passwordHash)
    {
        PasswordHash = passwordHash;
        return this;
    }
}