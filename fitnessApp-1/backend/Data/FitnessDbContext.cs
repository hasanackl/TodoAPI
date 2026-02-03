using Microsoft.EntityFrameworkCore;
using FitnessApi.Models;

namespace FitnessApi.Data;

public class FitnessDbContext : DbContext
{
    public FitnessDbContext(DbContextOptions<FitnessDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Workout> Workouts => Set<Workout>();
    public DbSet<WorkoutExercise> WorkoutExercises => Set<WorkoutExercise>();
    public DbSet<Exercise> Exercises => Set<Exercise>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
        });

        // Workout configuration
        modelBuilder.Entity<Workout>(entity =>
        {
            entity.HasOne(w => w.User)
                  .WithMany(u => u.Workouts)
                  .HasForeignKey(w => w.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // WorkoutExercise configuration
        modelBuilder.Entity<WorkoutExercise>(entity =>
        {
            entity.HasOne(we => we.Workout)
                  .WithMany(w => w.Exercises)
                  .HasForeignKey(we => we.WorkoutId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(we => we.Exercise)
                  .WithMany(e => e.WorkoutExercises)
                  .HasForeignKey(we => we.ExerciseId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Seed some exercises
        SeedExercises(modelBuilder);
    }

    private static void SeedExercises(ModelBuilder modelBuilder)
    {
        var fixedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        
        var exercises = new List<Exercise>
        {
            new() { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Bench Press", MuscleGroup = "Göğüs", Description = "Düz bench'te barbell ile yapılan göğüs egzersizi", CreatedAt = fixedDate },
            new() { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Squat", MuscleGroup = "Bacak", Description = "Barbell ile yapılan bacak egzersizi", CreatedAt = fixedDate },
            new() { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Deadlift", MuscleGroup = "Sırt", Description = "Yerden barbell kaldırma egzersizi", CreatedAt = fixedDate },
            new() { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), Name = "Pull-up", MuscleGroup = "Sırt", Description = "Barfiks çubuğunda çekiş egzersizi", CreatedAt = fixedDate },
            new() { Id = Guid.Parse("55555555-5555-5555-5555-555555555555"), Name = "Shoulder Press", MuscleGroup = "Omuz", Description = "Oturarak veya ayakta omuz press egzersizi", CreatedAt = fixedDate },
            new() { Id = Guid.Parse("66666666-6666-6666-6666-666666666666"), Name = "Bicep Curl", MuscleGroup = "Kol", Description = "Dumbbell ile biceps çalışması", CreatedAt = fixedDate },
            new() { Id = Guid.Parse("77777777-7777-7777-7777-777777777777"), Name = "Tricep Dips", MuscleGroup = "Kol", Description = "Paralel barda triceps çalışması", CreatedAt = fixedDate },
            new() { Id = Guid.Parse("88888888-8888-8888-8888-888888888888"), Name = "Plank", MuscleGroup = "Karın", Description = "Core güçlendirme egzersizi", CreatedAt = fixedDate },
            new() { Id = Guid.Parse("99999999-9999-9999-9999-999999999999"), Name = "Lunges", MuscleGroup = "Bacak", Description = "İleri adım atarak yapılan bacak egzersizi", CreatedAt = fixedDate },
            new() { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Name = "Leg Press", MuscleGroup = "Bacak", Description = "Makinede bacak press egzersizi", CreatedAt = fixedDate },
        };

        modelBuilder.Entity<Exercise>().HasData(exercises);
    }
}

