using TechnoByteLambders.MediTrackSensor.Platform.Iam.Application.Internal.OutboundServices;

namespace TechnoByteLambders.MediTrackSensor.Platform.Iam.Infrastructure.OutboundServices;

public class HashingService : IHashingService
{
    public string HashPassword(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password);

    public bool VerifyPassword(string password, string passwordHash) =>
        BCrypt.Net.BCrypt.Verify(password, passwordHash);
}
