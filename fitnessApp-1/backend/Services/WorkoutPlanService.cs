using Microsoft.EntityFrameworkCore;
using FitnessApi.Data;
using FitnessApi.DTOs;
using FitnessApi.Models;

namespace FitnessApi.Services;

public interface IWorkoutPlanService
{
    Task<WeeklyPlanDto> GenerateWeeklyPlanAsync(Guid userId, GeneratePlanRequest request);
    Task<PerformanceAnalysisDto> AnalyzePerformanceAsync(Guid userId);
}

public class WorkoutPlanService : IWorkoutPlanService
{
    private readonly FitnessDbContext _context;

    // Kas grubu bazlı egzersiz şablonları
    private static readonly Dictionary<string, List<(string Name, int Sets, int Reps, int? Duration)>> ExerciseTemplates = new()
    {
        ["Göğüs"] = new() { ("Bench Press", 4, 10, null), ("Incline Dumbbell Press", 3, 12, null), ("Cable Fly", 3, 15, null) },
        ["Sırt"] = new() { ("Deadlift", 4, 8, null), ("Pull-up", 4, 10, null), ("Barbell Row", 3, 12, null), ("Lat Pulldown", 3, 12, null) },
        ["Bacak"] = new() { ("Squat", 4, 10, null), ("Leg Press", 3, 12, null), ("Lunges", 3, 12, null), ("Leg Curl", 3, 15, null) },
        ["Omuz"] = new() { ("Shoulder Press", 4, 10, null), ("Lateral Raise", 3, 15, null), ("Front Raise", 3, 12, null) },
        ["Kol"] = new() { ("Bicep Curl", 3, 12, null), ("Tricep Dips", 3, 12, null), ("Hammer Curl", 3, 12, null), ("Tricep Pushdown", 3, 15, null) },
        ["Karın"] = new() { ("Plank", 3, 1, 60), ("Crunch", 3, 20, null), ("Leg Raise", 3, 15, null) }
    };

    // Günlük split programları
    private static readonly Dictionary<int, List<(string DayName, List<string> MuscleGroups)>> SplitPrograms = new()
    {
        [3] = new() // 3 gün: Push-Pull-Legs
        {
            ("Push Günü", new() { "Göğüs", "Omuz", "Kol" }),
            ("Pull Günü", new() { "Sırt", "Kol" }),
            ("Bacak Günü", new() { "Bacak", "Karın" })
        },
        [4] = new() // 4 gün: Upper-Lower Split
        {
            ("Üst Vücut A", new() { "Göğüs", "Sırt", "Omuz" }),
            ("Alt Vücut A", new() { "Bacak", "Karın" }),
            ("Üst Vücut B", new() { "Göğüs", "Sırt", "Kol" }),
            ("Alt Vücut B", new() { "Bacak", "Karın" })
        },
        [5] = new() // 5 gün: Bro Split
        {
            ("Göğüs Günü", new() { "Göğüs", "Karın" }),
            ("Sırt Günü", new() { "Sırt" }),
            ("Omuz Günü", new() { "Omuz", "Karın" }),
            ("Bacak Günü", new() { "Bacak" }),
            ("Kol Günü", new() { "Kol", "Karın" })
        },
        [6] = new() // 6 gün: PPL x2
        {
            ("Push Günü 1", new() { "Göğüs", "Omuz", "Kol" }),
            ("Pull Günü 1", new() { "Sırt", "Kol" }),
            ("Bacak Günü 1", new() { "Bacak", "Karın" }),
            ("Push Günü 2", new() { "Göğüs", "Omuz", "Kol" }),
            ("Pull Günü 2", new() { "Sırt", "Kol" }),
            ("Bacak Günü 2", new() { "Bacak", "Karın" })
        }
    };

    public WorkoutPlanService(FitnessDbContext context)
    {
        _context = context;
    }

    public async Task<WeeklyPlanDto> GenerateWeeklyPlanAsync(Guid userId, GeneratePlanRequest request)
    {
        // Kullanıcının geçmiş performansını al
        var analysis = await AnalyzePerformanceAsync(userId);
        
        // Gün sayısını ayarla (2-6 arası)
        var daysPerWeek = Math.Clamp(request.DaysPerWeek, 2, 6);
        if (daysPerWeek == 2) daysPerWeek = 3; // Minimum 3 gün

        // Split programını al
        var splitProgram = SplitPrograms[daysPerWeek];

        // Veritabanındaki egzersizleri al
        var dbExercises = await _context.Exercises.ToListAsync();

        // Günlük planları oluştur
        var days = new List<DailyPlanDto>();
        
        for (int i = 0; i < splitProgram.Count; i++)
        {
            var (dayName, muscleGroups) = splitProgram[i];
            var exercises = new List<PlannedExerciseDto>();

            foreach (var muscleGroup in muscleGroups)
            {
                if (!ExerciseTemplates.ContainsKey(muscleGroup)) continue;

                var templates = ExerciseTemplates[muscleGroup];
                var exerciseCount = GetExerciseCountForGoal(request.Goal, muscleGroup);

                foreach (var template in templates.Take(exerciseCount))
                {
                    // Veritabanından egzersizi bul
                    var dbExercise = dbExercises.FirstOrDefault(e => 
                        e.Name.Equals(template.Name, StringComparison.OrdinalIgnoreCase) ||
                        e.MuscleGroup == muscleGroup);

                    // Kullanıcının bu egzersiz için son performansını al
                    var lastPerformance = await GetLastPerformanceAsync(userId, template.Name);

                    // Seviyeye göre set/rep ayarla
                    var (sets, reps) = AdjustForLevel(template.Sets, template.Reps, request.Level, request.Goal);

                    // Progressive overload önerisi
                    var suggestedWeight = CalculateProgressiveWeight(lastPerformance, request.Level);

                    exercises.Add(new PlannedExerciseDto(
                        ExerciseId: dbExercise?.Id.ToString() ?? Guid.NewGuid().ToString(),
                        Name: template.Name,
                        MuscleGroup: muscleGroup,
                        RecommendedSets: sets,
                        RecommendedReps: reps,
                        RecommendedWeight: suggestedWeight,
                        RecommendedDurationSeconds: template.Duration,
                        Notes: GenerateExerciseNote(lastPerformance, suggestedWeight)
                    ));
                }
            }

            days.Add(new DailyPlanDto(
                DayNumber: i + 1,
                DayName: dayName,
                Focus: string.Join(" & ", muscleGroups),
                Exercises: exercises
            ));
        }

        // İlerleme notu oluştur
        var progressNote = GenerateProgressNote(analysis, request.Goal);

        return new WeeklyPlanDto(
            PlanName: GetPlanName(daysPerWeek, request.Goal),
            Goal: request.Goal,
            Level: request.Level,
            TotalDays: daysPerWeek,
            ProgressNote: progressNote,
            Days: days
        );
    }

    public async Task<PerformanceAnalysisDto> AnalyzePerformanceAsync(Guid userId)
    {
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

        // Son 30 günlük antrenmanları al
        var workouts = await _context.Workouts
            .Where(w => w.UserId == userId && w.StartedAt >= thirtyDaysAgo && w.EndedAt != null)
            .Include(w => w.Exercises)
                .ThenInclude(e => e.Exercise)
            .ToListAsync();

        var totalWorkouts = workouts.Count;
        var avgDuration = workouts.Any() 
            ? workouts.Average(w => w.DurationMinutes ?? 0) 
            : 0;

        // Kas grubu frekansı
        var muscleGroupFrequency = workouts
            .SelectMany(w => w.Exercises)
            .Where(e => e.Exercise?.MuscleGroup != null)
            .GroupBy(e => e.Exercise!.MuscleGroup!)
            .ToDictionary(g => g.Key, g => g.Count());

        // Tüm kas grupları
        var allMuscleGroups = new[] { "Göğüs", "Sırt", "Bacak", "Omuz", "Kol", "Karın" };
        
        // Az ve çok çalışılan kasları bul
        var avgFrequency = muscleGroupFrequency.Any() 
            ? muscleGroupFrequency.Values.Average() 
            : 0;

        var undertrainedMuscles = allMuscleGroups
            .Where(mg => !muscleGroupFrequency.ContainsKey(mg) || muscleGroupFrequency[mg] < avgFrequency * 0.5)
            .ToList();

        var overtrainedMuscles = muscleGroupFrequency
            .Where(kv => kv.Value > avgFrequency * 1.5)
            .Select(kv => kv.Key)
            .ToList();

        // İlerleme önerileri
        var suggestions = await GenerateProgressSuggestionsAsync(userId);

        // Önerilen odak
        var recommendedFocus = undertrainedMuscles.Any()
            ? $"Az çalışılan kaslar: {string.Join(", ", undertrainedMuscles)}"
            : "Dengeli bir program uyguluyorsunuz!";

        return new PerformanceAnalysisDto(
            TotalWorkoutsLast30Days: totalWorkouts,
            AverageWorkoutDuration: Math.Round(avgDuration, 1),
            MuscleGroupFrequency: muscleGroupFrequency,
            UndertrainedMuscles: undertrainedMuscles,
            OvertrainedMuscles: overtrainedMuscles,
            RecommendedFocus: recommendedFocus,
            Suggestions: suggestions
        );
    }

    private async Task<WorkoutExercise?> GetLastPerformanceAsync(Guid userId, string exerciseName)
    {
        return await _context.WorkoutExercises
            .Include(we => we.Workout)
            .Where(we => we.Workout.UserId == userId && 
                        we.Name.ToLower().Contains(exerciseName.ToLower()))
            .OrderByDescending(we => we.CreatedAt)
            .FirstOrDefaultAsync();
    }

    private async Task<List<ProgressSuggestionDto>> GenerateProgressSuggestionsAsync(Guid userId)
    {
        var suggestions = new List<ProgressSuggestionDto>();

        // Son yapılan egzersizleri al
        var recentExercises = await _context.WorkoutExercises
            .Include(we => we.Workout)
            .Where(we => we.Workout.UserId == userId && we.Weight.HasValue)
            .GroupBy(we => we.Name)
            .Select(g => g.OrderByDescending(we => we.CreatedAt).First())
            .Take(5)
            .ToListAsync();

        foreach (var exercise in recentExercises)
        {
            if (exercise.Weight.HasValue && exercise.Reps.HasValue)
            {
                // %5-10 ağırlık artışı öner
                var suggestedWeight = Math.Round(exercise.Weight.Value * 1.05m, 1);
                
                suggestions.Add(new ProgressSuggestionDto(
                    ExerciseName: exercise.Name,
                    LastWeight: exercise.Weight,
                    LastReps: exercise.Reps,
                    SuggestedWeight: suggestedWeight,
                    SuggestedReps: exercise.Reps,
                    Reason: "Progressive overload için %5 ağırlık artışı"
                ));
            }
        }

        return suggestions;
    }

    private static int GetExerciseCountForGoal(string goal, string muscleGroup)
    {
        return goal.ToLower() switch
        {
            "strength" => 2,      // Az egzersiz, ağır ağırlık
            "muscle" => 3,        // Orta egzersiz sayısı
            "endurance" => 3,     // Orta egzersiz, çok tekrar
            "weight_loss" => 2,   // Compound hareketler
            _ => 2                // Genel
        };
    }

    private static (int Sets, int Reps) AdjustForLevel(int baseSets, int baseReps, string level, string goal)
    {
        var sets = baseSets;
        var reps = baseReps;

        // Seviyeye göre ayarla
        switch (level.ToLower())
        {
            case "beginner":
                sets = Math.Max(2, baseSets - 1);
                reps = baseReps;
                break;
            case "advanced":
                sets = baseSets + 1;
                break;
        }

        // Hedefe göre ayarla
        switch (goal.ToLower())
        {
            case "strength":
                reps = Math.Min(6, baseReps);
                sets = Math.Max(4, sets);
                break;
            case "muscle":
                reps = Math.Clamp(baseReps, 8, 12);
                break;
            case "endurance":
                reps = Math.Max(15, baseReps);
                sets = Math.Max(3, sets - 1);
                break;
        }

        return (sets, reps);
    }

    private static decimal? CalculateProgressiveWeight(WorkoutExercise? lastPerformance, string level)
    {
        if (lastPerformance?.Weight == null) return null;

        // Seviyeye göre artış oranı
        var increaseRate = level.ToLower() switch
        {
            "beginner" => 1.025m,   // %2.5 artış
            "intermediate" => 1.05m, // %5 artış
            "advanced" => 1.025m,    // İleri seviyede yavaş artış
            _ => 1.05m
        };

        return Math.Round(lastPerformance.Weight.Value * increaseRate, 1);
    }

    private static string? GenerateExerciseNote(WorkoutExercise? lastPerformance, decimal? suggestedWeight)
    {
        if (lastPerformance == null) return "İlk kez yapıyorsanız hafif başlayın";
        
        if (suggestedWeight.HasValue && lastPerformance.Weight.HasValue)
        {
            var increase = suggestedWeight.Value - lastPerformance.Weight.Value;
            return $"Son: {lastPerformance.Weight}kg x {lastPerformance.Reps} rep. Önerilen artış: +{increase:F1}kg";
        }

        return null;
    }

    private static string GenerateProgressNote(PerformanceAnalysisDto analysis, string goal)
    {
        if (analysis.TotalWorkoutsLast30Days == 0)
        {
            return "Henüz antrenman kaydınız yok. Bu planla başlayın ve ilerlemenizi takip edin!";
        }

        if (analysis.TotalWorkoutsLast30Days < 8)
        {
            return $"Son 30 günde {analysis.TotalWorkoutsLast30Days} antrenman yaptınız. Tutarlılık için haftada en az 3 antrenman hedefleyin.";
        }

        if (analysis.UndertrainedMuscles.Any())
        {
            return $"Harika gidiyorsunuz! Dikkat: {string.Join(", ", analysis.UndertrainedMuscles)} kas gruplarına daha fazla odaklanabilirsiniz.";
        }

        return "Mükemmel ilerliyorsunuz! Dengeli bir antrenman programı uyguluyorsunuz.";
    }

    private static string GetPlanName(int days, string goal)
    {
        var goalName = goal.ToLower() switch
        {
            "strength" => "Güç",
            "muscle" => "Kas Geliştirme",
            "endurance" => "Dayanıklılık",
            "weight_loss" => "Yağ Yakımı",
            _ => "Genel Fitness"
        };

        var splitName = days switch
        {
            3 => "Push-Pull-Legs",
            4 => "Upper-Lower",
            5 => "Bro Split",
            6 => "PPL x2",
            _ => "Özel"
        };

        return $"{goalName} Programı ({splitName})";
    }
}

