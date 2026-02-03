namespace FitnessApi.DTOs;

// Antrenman planı isteği
public record GeneratePlanRequest(
    int DaysPerWeek = 3,           // Haftada kaç gün
    string Goal = "general",       // Hedef: "strength", "muscle", "endurance", "weight_loss", "general"
    string Level = "intermediate"  // Seviye: "beginner", "intermediate", "advanced"
);

// Günlük plan
public record DailyPlanDto(
    int DayNumber,
    string DayName,
    string Focus,                  // O günün odak noktası (örn: "Göğüs & Triceps")
    List<PlannedExerciseDto> Exercises
);

// Planlanan egzersiz
public record PlannedExerciseDto(
    string ExerciseId,
    string Name,
    string MuscleGroup,
    int RecommendedSets,
    int RecommendedReps,
    decimal? RecommendedWeight,    // Geçmiş performansa göre önerilen ağırlık
    int? RecommendedDurationSeconds,
    string? Notes
);

// Haftalık plan yanıtı
public record WeeklyPlanDto(
    string PlanName,
    string Goal,
    string Level,
    int TotalDays,
    string? ProgressNote,          // İlerleme notu
    List<DailyPlanDto> Days
);

// Performans analizi
public record PerformanceAnalysisDto(
    int TotalWorkoutsLast30Days,
    double AverageWorkoutDuration,
    Dictionary<string, int> MuscleGroupFrequency,  // Kas grubu başına çalışma sayısı
    List<string> UndertrainedMuscles,              // Az çalışılan kas grupları
    List<string> OvertrainedMuscles,               // Çok çalışılan kas grupları
    string RecommendedFocus,                        // Önerilen odak
    List<ProgressSuggestionDto> Suggestions
);

// İlerleme önerisi
public record ProgressSuggestionDto(
    string ExerciseName,
    decimal? LastWeight,
    int? LastReps,
    decimal? SuggestedWeight,
    int? SuggestedReps,
    string Reason
);

