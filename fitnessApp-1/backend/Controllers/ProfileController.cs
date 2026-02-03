using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitnessApi.Data;
using FitnessApi.DTOs;
using FitnessApi.Models;

namespace FitnessApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly FitnessDbContext _context;

    public ProfileController(FitnessDbContext context)
    {
        _context = context;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    /// <summary>
    /// Kullanıcının profil bilgilerini getirir
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ProfileDto>> GetProfile()
    {
        var userId = GetUserId();
        
        var user = await _context.Users
            .Include(u => u.Workouts)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return NotFound(new { message = "Kullanıcı bulunamadı" });
        }

        var stats = await GetProfileStatsAsync(userId);

        return Ok(MapToProfileDto(user, stats));
    }

    /// <summary>
    /// Kullanıcının profil bilgilerini günceller
    /// </summary>
    [HttpPut]
    public async Task<ActionResult<ProfileDto>> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = GetUserId();
        
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            return NotFound(new { message = "Kullanıcı bulunamadı" });
        }

        // Sadece gönderilen alanları güncelle
        if (request.Name != null) user.Name = request.Name;
        if (request.Height.HasValue) user.Height = request.Height;
        if (request.Weight.HasValue) user.Weight = request.Weight;
        if (request.Gender != null) user.Gender = ValidateGender(request.Gender);
        if (request.DateOfBirth.HasValue) user.DateOfBirth = request.DateOfBirth;
        if (request.FitnessGoal != null) user.FitnessGoal = ValidateFitnessGoal(request.FitnessGoal);
        if (request.ActivityLevel != null) user.ActivityLevel = ValidateActivityLevel(request.ActivityLevel);
        if (request.ProfileImageUrl != null) user.ProfileImageUrl = request.ProfileImageUrl;

        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var stats = await GetProfileStatsAsync(userId);
        return Ok(MapToProfileDto(user, stats));
    }

    /// <summary>
    /// Kullanıcının şifresini değiştirir
    /// </summary>
    [HttpPost("change-password")]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = GetUserId();
        
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            return NotFound(new { message = "Kullanıcı bulunamadı" });
        }

        // Mevcut şifreyi doğrula
        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return BadRequest(new { message = "Mevcut şifre yanlış" });
        }

        // Yeni şifreyi hashle ve kaydet
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Şifre başarıyla değiştirildi" });
    }

    /// <summary>
    /// Kullanıcının sağlık metriklerini hesaplar (BMI, kalori, vb.)
    /// </summary>
    [HttpGet("health-metrics")]
    public async Task<ActionResult<HealthMetricsDto>> GetHealthMetrics()
    {
        var userId = GetUserId();
        
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            return NotFound(new { message = "Kullanıcı bulunamadı" });
        }

        var bmi = user.Bmi;
        var bmiCategory = GetBmiCategory(bmi);
        var dailyCalories = CalculateDailyCalories(user);

        return Ok(new HealthMetricsDto(
            Bmi: bmi,
            BmiCategory: bmiCategory,
            EstimatedDailyCalories: dailyCalories,
            Age: user.Age
        ));
    }

    /// <summary>
    /// Kilo geçmişini kaydeder (isteğe bağlı tracking)
    /// </summary>
    [HttpPost("log-weight")]
    public async Task<ActionResult> LogWeight([FromBody] decimal weight)
    {
        var userId = GetUserId();
        
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            return NotFound(new { message = "Kullanıcı bulunamadı" });
        }

        user.Weight = weight;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { 
            message = "Kilo kaydedildi", 
            weight = weight,
            bmi = user.Bmi,
            bmiCategory = GetBmiCategory(user.Bmi)
        });
    }

    private async Task<ProfileStatsDto> GetProfileStatsAsync(Guid userId)
    {
        var workouts = await _context.Workouts
            .Where(w => w.UserId == userId && w.EndedAt != null)
            .OrderByDescending(w => w.StartedAt)
            .ToListAsync();

        var totalWorkouts = workouts.Count;
        var totalMinutes = workouts.Sum(w => w.DurationMinutes ?? 0);
        var lastWorkoutDate = workouts.FirstOrDefault()?.StartedAt;

        // Streak hesaplama
        var (currentStreak, longestStreak) = CalculateStreaks(workouts);

        return new ProfileStatsDto(
            TotalWorkouts: totalWorkouts,
            TotalMinutes: totalMinutes,
            CurrentStreak: currentStreak,
            LongestStreak: longestStreak,
            LastWorkoutDate: lastWorkoutDate
        );
    }

    private static (int current, int longest) CalculateStreaks(List<Workout> workouts)
    {
        if (!workouts.Any()) return (0, 0);

        var workoutDates = workouts
            .Select(w => w.StartedAt.Date)
            .Distinct()
            .OrderByDescending(d => d)
            .ToList();

        int currentStreak = 0;
        int longestStreak = 0;
        int tempStreak = 1;

        // Mevcut streak - bugünden veya dünden başlıyor mu?
        var today = DateTime.UtcNow.Date;
        if (workoutDates.Contains(today) || workoutDates.Contains(today.AddDays(-1)))
        {
            var checkDate = workoutDates.Contains(today) ? today : today.AddDays(-1);
            currentStreak = 1;

            for (int i = 1; i < 365; i++)
            {
                if (workoutDates.Contains(checkDate.AddDays(-i)))
                    currentStreak++;
                else
                    break;
            }
        }

        // En uzun streak
        for (int i = 1; i < workoutDates.Count; i++)
        {
            if ((workoutDates[i - 1] - workoutDates[i]).Days == 1)
            {
                tempStreak++;
            }
            else
            {
                longestStreak = Math.Max(longestStreak, tempStreak);
                tempStreak = 1;
            }
        }
        longestStreak = Math.Max(longestStreak, tempStreak);
        longestStreak = Math.Max(longestStreak, currentStreak);

        return (currentStreak, longestStreak);
    }

    private static string? GetBmiCategory(decimal? bmi)
    {
        if (!bmi.HasValue) return null;

        return bmi.Value switch
        {
            < 18.5m => "Zayıf",
            < 25m => "Normal",
            < 30m => "Fazla Kilolu",
            _ => "Obez"
        };
    }

    private static int? CalculateDailyCalories(User user)
    {
        if (!user.Weight.HasValue || !user.Height.HasValue || !user.Age.HasValue)
            return null;

        // Harris-Benedict formülü (BMR)
        double bmr;
        if (user.Gender?.ToLower() == "male")
        {
            bmr = 88.362 + (13.397 * (double)user.Weight.Value) + 
                  (4.799 * (double)user.Height.Value) - (5.677 * user.Age.Value);
        }
        else
        {
            bmr = 447.593 + (9.247 * (double)user.Weight.Value) + 
                  (3.098 * (double)user.Height.Value) - (4.330 * user.Age.Value);
        }

        // Aktivite seviyesine göre çarpan
        var activityMultiplier = user.ActivityLevel?.ToLower() switch
        {
            "sedentary" => 1.2,
            "light" => 1.375,
            "moderate" => 1.55,
            "active" => 1.725,
            "very_active" => 1.9,
            _ => 1.55 // varsayılan: moderate
        };

        return (int)(bmr * activityMultiplier);
    }

    private static string? ValidateGender(string? gender)
    {
        var valid = new[] { "male", "female", "other" };
        return valid.Contains(gender?.ToLower()) ? gender?.ToLower() : null;
    }

    private static string? ValidateFitnessGoal(string? goal)
    {
        var valid = new[] { "weight_loss", "muscle_gain", "maintain", "endurance" };
        return valid.Contains(goal?.ToLower()) ? goal?.ToLower() : null;
    }

    private static string? ValidateActivityLevel(string? level)
    {
        var valid = new[] { "sedentary", "light", "moderate", "active", "very_active" };
        return valid.Contains(level?.ToLower()) ? level?.ToLower() : null;
    }

    private static ProfileDto MapToProfileDto(User user, ProfileStatsDto stats)
    {
        return new ProfileDto(
            Id: user.Id.ToString(),
            Email: user.Email,
            Name: user.Name,
            Height: user.Height,
            Weight: user.Weight,
            Gender: user.Gender,
            DateOfBirth: user.DateOfBirth,
            FitnessGoal: user.FitnessGoal,
            ActivityLevel: user.ActivityLevel,
            ProfileImageUrl: user.ProfileImageUrl,
            CreatedAt: user.CreatedAt,
            UpdatedAt: user.UpdatedAt,
            Stats: stats
        );
    }
}

