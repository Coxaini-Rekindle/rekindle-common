using Rekindle.Authentication.Models;

namespace Rekindle.Authentication.Interfaces;

public interface IRefreshTokenGenerator
{
    public RefreshToken GenerateRefreshToken();
}