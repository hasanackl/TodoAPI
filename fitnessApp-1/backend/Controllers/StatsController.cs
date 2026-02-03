using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitnessApi.Data;
using FitnessApi.DTOs;

namespace FitnessApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StatsController : ControllerBase
{
    private readonly FitnessDbContext _context;

    public StatsController(FitnessDbContext context)
    {
        _context = context;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    /// <summary>
    /// Kullanıcının ilerleme istatistiklerini getirir
    /// </summary>
    [HttpGet("progress")]
    public async Task<ActionResult<ProgressStatsDto>> GetProgress(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var userId = GetUserId();
        
        // Default date range: last 30 days
        var startDate = from ?? DateTime.UtcNow.AddDays(-30);
        var endDate = to ?? DateTime.UtcNow;

        var workouts = await _context.Workouts
            .Where(w => w.UserId == userId 
                     && w.StartedAt >= startDate 
                     && w.StartedAt <= endDate
                     && w.EndedAt != null)
            .Include(w => w.Exercises)
                .ThenInclude(e => e.Exercise)
            .OrderBy(w => w.StartedAt)
            .ToListAsync();

        // Total stats
        var totalWorkouts = workouts.Count;
        var totalMinutes = workouts.Sum(w => w.DurationMinutes ?? 0);
        var totalExercises = workouts.Sum(w => w.Exercises.Count);
        var avgWorkoutMinutes = totalWorkouts > 0 
            ? Math.Round((double)totalMinutes / totalWorkouts, 1) 
            : 0;

        // Weekly stats
        var weeklyStats = workouts
            .GroupBy(w => GetWeekStart(w.StartedAt))
            .Select(g => new WeeklyStatsDto(
                WeekStart: g.Key.ToString("yyyy-MM-dd"),
                WorkoutCount: g.Count(),
                TotalMinutes: g.Sum(w => w.DurationMinutes ?? 0)
            ))
            .OrderBy(w => w.WeekStart)
            .ToList();

        // Muscle group stats
        var muscleGroupStats = workouts
            .SelectMany(w => w.Exercises)
            .Where(e => e.Exercise?.MuscleGroup != null)
            .GroupBy(e => e.Exercise!.MuscleGroup!)
            .Select(g => new MuscleGroupStatsDto(
                MuscleGroup: g.Key,
                ExerciseCount: g.Count()
            ))
            .OrderByDescending(m => m.ExerciseCount)
            .ToList();

        return Ok(new ProgressStatsDto(
            TotalWorkouts: totalWorkouts,
            TotalMinutes: totalMinutes,
            TotalExercises: totalExercises,
            AverageWorkoutMinutes: avgWorkoutMinutes,
            WeeklyStats: weeklyStats,
            MuscleGroupStats: muscleGroupStats
        ));
    }

    private static DateTime GetWeekStart(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff).Date;
    }
}

