using System.ComponentModel.DataAnnotations;

namespace FitnessApi.DTOs;

// Profil görüntüleme yanıtı
public record ProfileDto(
    string Id,
    string Email,
    string? Name,
    decimal? Height,           // Boy (cm)
    decimal? Weight,           // Kilo (kg)
    string? Gender,            // Cinsiyet: "male", "female", "other"
    DateTime? DateOfBirth,     // Doğum tarihi
    string? FitnessGoal,       // Hedef: "weight_loss", "muscle_gain", "maintain", "endurance"
    string? ActivityLevel,     // Aktivite seviyesi: "sedentary", "light", "moderate", "active", "very_active"
    string? ProfileImageUrl,   // Profil fotoğrafı URL
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    ProfileStatsDto? Stats     // Profil istatistikleri
);

// Profil istatistikleri
public record ProfileStatsDto(
    int TotalWorkouts,
    int TotalMinutes,
    int CurrentStreak,         // Üst üste antrenman günü
    int LongestStreak,         // En uzun seri
    DateTime? LastWorkoutDate
);

// Profil güncelleme isteği
public record UpdateProfileRequest(
    [MaxLength(100)] string? Name,
    [Range(50, 300)] decimal? Height,
    [Range(20, 500)] decimal? Weight,
    string? Gender,
    DateTime? DateOfBirth,
    string? FitnessGoal,
    string? ActivityLevel,
    string? ProfileImageUrl
);

// Şifre değiştirme isteği
public record ChangePasswordRequest(
    [Required] string CurrentPassword,
    [Required][MinLength(6)] string NewPassword
);

// BMI ve kalori hesaplama yanıtı
public record HealthMetricsDto(
    decimal? Bmi,
    string? BmiCategory,           // "Underweight", "Normal", "Overweight", "Obese"
    int? EstimatedDailyCalories,   // Tahmini günlük kalori ihtiyacı
    int? Age
);

