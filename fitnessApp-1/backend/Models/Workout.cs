using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessApi.Models;

public class Workout
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid UserId { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = "Antrenman";

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    public DateTime? EndedAt { get; set; }

    [NotMapped]
    public int? DurationMinutes => EndedAt.HasValue 
        ? (int)(EndedAt.Value - StartedAt).TotalMinutes 
        : null;

    public string? Notes { get; set; }

    // Navigation properties
    [ForeignKey("UserId")]
    public User User { get; set; } = null!;

    public ICollection<WorkoutExercise> Exercises { get; set; } = new List<WorkoutExercise>();
}

