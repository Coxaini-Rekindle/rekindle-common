namespace Rekindle.Authentication.Models;

public record RefreshToken(string Token = "", DateTime ExpiryTime = default);