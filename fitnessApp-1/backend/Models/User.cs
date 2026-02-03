using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessApi.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Name { get; set; }

    // Profil bilgileri
    [Column(TypeName = "decimal(5,2)")]
    public decimal? Height { get; set; }  // Boy (cm)

    [Column(TypeName = "decimal(5,2)")]
    public decimal? Weight { get; set; }  // Kilo (kg)

    [MaxLength(10)]
    public string? Gender { get; set; }   // "male", "female", "other"

    public DateTime? DateOfBirth { get; set; }

    [MaxLength(20)]
    public string? FitnessGoal { get; set; }  // "weight_loss", "muscle_gain", "maintain", "endurance"

    [MaxLength(20)]
    public string? ActivityLevel { get; set; }  // "sedentary", "light", "moderate", "active", "very_active"

    [MaxLength(500)]
    public string? ProfileImageUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation property
    public ICollection<Workout> Workouts { get; set; } = new List<Workout>();

    // Calculated properties
    [NotMapped]
    public int? Age => DateOfBirth.HasValue 
        ? (int)((DateTime.UtcNow - DateOfBirth.Value).TotalDays / 365.25) 
        : null;

    [NotMapped]
    public decimal? Bmi => (Height.HasValue && Weight.HasValue && Height.Value > 0)
        ? Math.Round(Weight.Value / ((Height.Value / 100) * (Height.Value / 100)), 1)
        : null;
}
