using System.ComponentModel.DataAnnotations;

namespace FitnessApi.DTOs;

// Create Workout Request
public record CreateWorkoutRequest(
    string? Name
);

// Workout Exercise DTO
public record WorkoutExerciseDto(
    string Id,
    string Name,
    int? Sets,
    int? Reps,
    decimal? Weight,
    int? DurationSeconds
);

// Workout Response
public record WorkoutDto(
    string Id,
    string Name,
    string StartedAt,
    string? EndedAt,
    int? DurationMinutes,
    List<WorkoutExerciseDto>? Exercises
);

// Add Exercise to Workout
public record AddExerciseRequest(
    [Required] string Name,
    int? Sets,
    int? Reps,
    decimal? Weight,
    int? DurationSeconds,
    Guid? ExerciseId
);

// Exercise DTO (for catalog)
public record ExerciseDto(
    string Id,
    string Name,
    string? Description,
    string? VideoUrl,
    string? MuscleGroup
);

