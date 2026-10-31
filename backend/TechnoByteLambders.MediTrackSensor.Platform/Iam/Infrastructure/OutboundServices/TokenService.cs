using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Application.Internal.OutboundServices;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.Aggregates;

namespace TechnoByteLambders.MediTrackSensor.Platform.Iam.Infrastructure.OutboundServices;

public class TokenService(IConfiguration configuration) : ITokenService
{
    public string GenerateToken(User user)
    {
        var secret = configuration["Jwt:Secret"]!;
        var issuer = configuration["Jwt:Issuer"];
        var audience = configuration["Jwt:Audience"];
        var expirationDays = int.Parse(configuration["Jwt:ExpirationDays"] ?? "7");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email.Value),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(expirationDays),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public int? ValidateToken(string token)
    {
        try
        {
            var secret = configuration["Jwt:Secret"]!;
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secret);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out var validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            return int.Parse(jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
        }
        catch
        {
            return null;
        }
    }
}
