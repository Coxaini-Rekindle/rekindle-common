namespace Rekindle.Authentication.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    string? Username { get; }
    bool IsAuthenticated { get; }
}
