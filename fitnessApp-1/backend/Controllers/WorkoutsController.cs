using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitnessApi.Data;
using FitnessApi.DTOs;
using FitnessApi.Models;

namespace FitnessApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkoutsController : ControllerBase
{
    private readonly FitnessDbContext _context;

    public WorkoutsController(FitnessDbContext context)
    {
        _context = context;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    /// <summary>
    /// Kullanıcının tüm antrenmanlarını getirir
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<WorkoutDto>>> GetWorkouts()
    {
        var userId = GetUserId();
        
        var workouts = await _context.Workouts
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.StartedAt)
            .Include(w => w.Exercises)
            .ToListAsync();

        return Ok(workouts.Select(MapToWorkoutDto).ToList());
    }

    /// <summary>
    /// Belirli bir antrenmanın detaylarını getirir
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<WorkoutDto>> GetWorkout(Guid id)
    {
        var userId = GetUserId();
        
        var workout = await _context.Workouts
            .Include(w => w.Exercises)
            .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);

        if (workout == null)
        {
            return NotFound(new { message = "Antrenman bulunamadı" });
        }

        return Ok(MapToWorkoutDto(workout));
    }

    /// <summary>
    /// Yeni antrenman başlatır
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<WorkoutDto>> StartWorkout([FromBody] CreateWorkoutRequest? request)
    {
        var userId = GetUserId();

        var workout = new Workout
        {
            UserId = userId,
            Name = request?.Name ?? "Antrenman",
            StartedAt = DateTime.UtcNow
        };

        _context.Workouts.Add(workout);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetWorkout), new { id = workout.Id }, MapToWorkoutDto(workout));
    }

    /// <summary>
    /// Antrenmanı sonlandırır
    /// </summary>
    [HttpPatch("{id}/end")]
    public async Task<ActionResult<WorkoutDto>> EndWorkout(Guid id)
    {
        var userId = GetUserId();

        var workout = await _context.Workouts
            .Include(w => w.Exercises)
            .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);

        if (workout == null)
        {
            return NotFound(new { message = "Antrenman bulunamadı" });
        }

        if (workout.EndedAt.HasValue)
        {
            return BadRequest(new { message = "Bu antrenman zaten sonlandırılmış" });
        }

        workout.EndedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(MapToWorkoutDto(workout));
    }

    /// <summary>
    /// Antrenmana egzersiz ekler
    /// </summary>
    [HttpPost("{id}/exercises")]
    public async Task<ActionResult<WorkoutDto>> AddExercise(Guid id, [FromBody] AddExerciseRequest request)
    {
        var userId = GetUserId();

        var workout = await _context.Workouts
            .Include(w => w.Exercises)
            .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);

        if (workout == null)
        {
            return NotFound(new { message = "Antrenman bulunamadı" });
        }

        var exercise = new WorkoutExercise
        {
            WorkoutId = workout.Id,
            ExerciseId = request.ExerciseId,
            Name = request.Name,
            Sets = request.Sets,
            Reps = request.Reps,
            Weight = request.Weight,
            DurationSeconds = request.DurationSeconds,
            Order = workout.Exercises.Count + 1
        };

        workout.Exercises.Add(exercise);
        await _context.SaveChangesAsync();

        return Ok(MapToWorkoutDto(workout));
    }

    /// <summary>
    /// Antrenmanı siler
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteWorkout(Guid id)
    {
        var userId = GetUserId();

        var workout = await _context.Workouts
            .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);

        if (workout == null)
        {
            return NotFound(new { message = "Antrenman bulunamadı" });
        }

        _context.Workouts.Remove(workout);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static WorkoutDto MapToWorkoutDto(Workout workout)
    {
        return new WorkoutDto(
            Id: workout.Id.ToString(),
            Name: workout.Name,
            StartedAt: workout.StartedAt.ToString("o"),
            EndedAt: workout.EndedAt?.ToString("o"),
            DurationMinutes: workout.DurationMinutes,
            Exercises: workout.Exercises.Select(e => new WorkoutExerciseDto(
                Id: e.Id.ToString(),
                Name: e.Name,
                Sets: e.Sets,
                Reps: e.Reps,
                Weight: e.Weight,
                DurationSeconds: e.DurationSeconds
            )).ToList()
        );
    }
}

