using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;
using Rekindle.Authentication.Interfaces;

namespace Rekindle.Authentication;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !user.Identity?.IsAuthenticated == true) return null;

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                         user.FindFirst("sub")?.Value ??
                         user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ??
                         user.FindFirst(JwtRegisteredClaimNames.NameId)?.Value;

            return string.IsNullOrEmpty(userId) ? null : Guid.Parse(userId);
        }
    }

    public string? Email
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.FindFirst(ClaimTypes.Email)?.Value ??
                   user?.FindFirst("email")?.Value ??
                   user?.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
        }
    }

    public string? Username
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.FindFirst(ClaimTypes.Name)?.Value ??
                   user?.FindFirst("username")?.Value ??
                   user?.FindFirst("preferred_username")?.Value;
        }
    }

    public bool IsAuthenticated
    {
        get
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;
        }
    }
}
