using System.Security.Claims;
using Rekindle.Authentication.Models;

namespace Rekindle.Authentication.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(UserClaims userClaims);
    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}