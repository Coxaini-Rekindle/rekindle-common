using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Rekindle.Authentication.Interfaces;
using Rekindle.Authentication.Models;

namespace Rekindle.Authentication.Jwt;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _jwtSettings;

    public JwtTokenGenerator(IOptions<JwtSettings> jwtOptions)
    {
        _jwtSettings = jwtOptions.Value;
    }

    public string GenerateToken(UserClaims userClaims)
    {
        var signingCredentials = new SigningCredentials(new
                SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
            SecurityAlgorithms.HmacSha256);        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.NameId, userClaims.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, userClaims.Email)
        };

        var securityToken = new JwtSecurityToken(
            claims: claims,
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            signingCredentials: signingCredentials
        );


        return new JwtSecurityTokenHandler().WriteToken(securityToken);
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        byte[] key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            ClockSkew = TimeSpan.Zero
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Invalid token");

        return principal;
    }
}