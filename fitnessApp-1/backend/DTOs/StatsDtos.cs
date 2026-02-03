namespace FitnessApi.DTOs;

// Progress Stats Response
public record ProgressStatsDto(
    int TotalWorkouts,
    int TotalMinutes,
    int TotalExercises,
    double AverageWorkoutMinutes,
    List<WeeklyStatsDto> WeeklyStats,
    List<MuscleGroupStatsDto> MuscleGroupStats
);

// Weekly Stats
public record WeeklyStatsDto(
    string WeekStart,
    int WorkoutCount,
    int TotalMinutes
);

// Muscle Group Stats
public record MuscleGroupStatsDto(
    string MuscleGroup,
    int ExerciseCount
);

