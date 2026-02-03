using System.ComponentModel.DataAnnotations;

namespace FitnessApi.DTOs;

// Login
public record LoginRequest(
    [Required][EmailAddress] string Email,
    [Required][MinLength(6)] string Password
);

// Register
public record RegisterRequest(
    [Required][EmailAddress] string Email,
    [Required][MinLength(6)] string Password,
    string? Name
);

// Auth Response (for both login and register)
public record AuthResponse(
    string Token,
    UserDto User
);

// User DTO
public record UserDto(
    string Id,
    string Email,
    string? Name,
    string? CreatedAt
);

