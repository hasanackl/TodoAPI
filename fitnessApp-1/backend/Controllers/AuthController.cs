using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitnessApi.Data;
using FitnessApi.DTOs;
using FitnessApi.Models;
using FitnessApi.Services;

namespace FitnessApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly FitnessDbContext _context;
    private readonly IJwtService _jwtService;

    public AuthController(FitnessDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    /// <summary>
    /// Kullanıcı girişi
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "E-posta veya şifre hatalı" });
        }

        var token = _jwtService.GenerateToken(user);
        
        return Ok(new AuthResponse(
            Token: token,
            User: MapToUserDto(user)
        ));
    }

    /// <summary>
    /// Yeni kullanıcı kaydı
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        // Check if email already exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

        if (existingUser != null)
        {
            return Conflict(new { message = "Bu e-posta adresi zaten kullanılıyor" });
        }

        var user = new User
        {
            Email = request.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Name = request.Name
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = _jwtService.GenerateToken(user);

        return CreatedAtAction(nameof(Register), new AuthResponse(
            Token: token,
            User: MapToUserDto(user)
        ));
    }

    private static UserDto MapToUserDto(User user)
    {
        return new UserDto(
            Id: user.Id.ToString(),
            Email: user.Email,
            Name: user.Name,
            CreatedAt: user.CreatedAt.ToString("o")
        );
    }
}

