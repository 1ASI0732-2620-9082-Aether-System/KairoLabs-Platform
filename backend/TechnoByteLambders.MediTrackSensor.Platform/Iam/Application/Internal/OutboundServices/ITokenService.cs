using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.Aggregates;

namespace TechnoByteLambders.MediTrackSensor.Platform.Iam.Application.Internal.OutboundServices;

public interface ITokenService
{
    string GenerateToken(User user);
    int? ValidateToken(string token);
}