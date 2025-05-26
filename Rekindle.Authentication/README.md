# Rekindle.Authentication

A comprehensive authentication library for .NET applications providing JWT token generation and validation, secure password hashing, refresh token handling, and user management functionality.

## Features

- **JWT Authentication**: Generate and validate JWT tokens for secure API access
- **Password Hashing**: Securely hash and verify passwords using BCrypt
- **Refresh Token Support**: Implement token refresh functionality for better security
- **User Claims Management**: Easily work with user identity claims
- **ASP.NET Core Integration**: Simple setup with dependency injection

## Installation

```bash
dotnet add package Rekindle.Authentication
```

## Usage

### Setup in Program.cs

```csharp
// Add JWT authentication
builder.Services.AddJwtAuth(builder.Configuration);

// Configure JWT settings in appsettings.json
{
  "JwtSettings": {
    "Secret": "your-secret-key-here",
    "ExpiryMinutes": 60,
    "Issuer": "Rekindle",
    "Audience": "Rekindle"
  }
}
```

### Password Hashing

```csharp
// Inject IPasswordHasher
private readonly IPasswordHasher _passwordHasher;

public UserService(IPasswordHasher passwordHasher)
{
    _passwordHasher = passwordHasher;
}

// Hash a password
string hashedPassword = _passwordHasher.HashPassword("user-password");

// Verify a password
bool isValid = _passwordHasher.VerifyPassword("user-password", hashedPassword);
```

### JWT Token Generation

```csharp
// Inject IJwtTokenGenerator
private readonly IJwtTokenGenerator _jwtTokenGenerator;

public AuthService(IJwtTokenGenerator jwtTokenGenerator)
{
    _jwtTokenGenerator = jwtTokenGenerator;
}

// Generate a token
var userClaims = new UserClaims(
    id: userId,
    email: userEmail,
    roles: new[] { "User" }
);

string token = _jwtTokenGenerator.GenerateToken(userClaims);
```

### Current User Access

```csharp
// Inject ICurrentUserService
private readonly ICurrentUserService _currentUserService;

public SomeService(ICurrentUserService currentUserService)
{
    _currentUserService = currentUserService;
}

// Get current user ID
string userId = _currentUserService.UserId;
```

## License

This project is licensed under the MIT License - see the LICENSE file for details.
