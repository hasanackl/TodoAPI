using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitnessApi.Data;
using FitnessApi.DTOs;

namespace FitnessApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExercisesController : ControllerBase
{
    private readonly FitnessDbContext _context;

    public ExercisesController(FitnessDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Tüm egzersizleri listeler
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<ExerciseDto>>> GetExercises([FromQuery] string? muscleGroup)
    {
        var query = _context.Exercises.AsQueryable();

        if (!string.IsNullOrEmpty(muscleGroup))
        {
            query = query.Where(e => e.MuscleGroup == muscleGroup);
        }

        var exercises = await query
            .OrderBy(e => e.Name)
            .Select(e => new ExerciseDto(
                e.Id.ToString(),
                e.Name,
                e.Description,
                e.VideoUrl,
                e.MuscleGroup
            ))
            .ToListAsync();

        return Ok(exercises);
    }

    /// <summary>
    /// Belirli bir egzersizi getirir
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ExerciseDto>> GetExercise(Guid id)
    {
        var exercise = await _context.Exercises.FindAsync(id);

        if (exercise == null)
        {
            return NotFound(new { message = "Egzersiz bulunamadı" });
        }

        return Ok(new ExerciseDto(
            exercise.Id.ToString(),
            exercise.Name,
            exercise.Description,
            exercise.VideoUrl,
            exercise.MuscleGroup
        ));
    }

    /// <summary>
    /// Kas gruplarını listeler
    /// </summary>
    [HttpGet("muscle-groups")]
    public async Task<ActionResult<List<string>>> GetMuscleGroups()
    {
        var muscleGroups = await _context.Exercises
            .Where(e => e.MuscleGroup != null)
            .Select(e => e.MuscleGroup!)
            .Distinct()
            .OrderBy(m => m)
            .ToListAsync();

        return Ok(muscleGroups);
    }
}

