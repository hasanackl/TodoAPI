using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessApi.Models;

public class WorkoutExercise
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid WorkoutId { get; set; }

    public Guid? ExerciseId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public int? Sets { get; set; }

    public int? Reps { get; set; }

    public decimal? Weight { get; set; }

    public int? DurationSeconds { get; set; }

    public int Order { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("WorkoutId")]
    public Workout Workout { get; set; } = null!;

    [ForeignKey("ExerciseId")]
    public Exercise? Exercise { get; set; }
}

