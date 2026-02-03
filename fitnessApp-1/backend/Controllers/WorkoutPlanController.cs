using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FitnessApi.DTOs;
using FitnessApi.Services;

namespace FitnessApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkoutPlanController : ControllerBase
{
    private readonly IWorkoutPlanService _planService;

    public WorkoutPlanController(IWorkoutPlanService planService)
    {
        _planService = planService;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    /// <summary>
    /// Kullanıcıya özel haftalık antrenman planı oluşturur
    /// </summary>
    /// <remarks>
    /// Hedefler (goal):
    /// - strength: Güç antrenmanı (düşük tekrar, yüksek ağırlık)
    /// - muscle: Kas geliştirme (orta tekrar)
    /// - endurance: Dayanıklılık (yüksek tekrar)
    /// - weight_loss: Yağ yakımı
    /// - general: Genel fitness
    /// 
    /// Seviyeler (level):
    /// - beginner: Başlangıç
    /// - intermediate: Orta
    /// - advanced: İleri
    /// </remarks>
    [HttpPost("generate")]
    public async Task<ActionResult<WeeklyPlanDto>> GenerateWeeklyPlan([FromBody] GeneratePlanRequest? request)
    {
        var userId = GetUserId();
        var planRequest = request ?? new GeneratePlanRequest();
        
        var plan = await _planService.GenerateWeeklyPlanAsync(userId, planRequest);
        return Ok(plan);
    }

    /// <summary>
    /// Kullanıcının performans analizini getirir
    /// </summary>
    /// <remarks>
    /// Son 30 günlük antrenman verilerini analiz eder:
    /// - Toplam antrenman sayısı
    /// - Ortalama antrenman süresi
    /// - Kas grubu bazlı çalışma frekansı
    /// - Az/çok çalışılan kaslar
    /// - Progressive overload önerileri
    /// </remarks>
    [HttpGet("analysis")]
    public async Task<ActionResult<PerformanceAnalysisDto>> GetPerformanceAnalysis()
    {
        var userId = GetUserId();
        var analysis = await _planService.AnalyzePerformanceAsync(userId);
        return Ok(analysis);
    }

    /// <summary>
    /// Hızlı plan önerileri - hazır şablonlar
    /// </summary>
    [HttpGet("templates")]
    public ActionResult<List<object>> GetPlanTemplates()
    {
        var templates = new List<object>
        {
            new {
                Id = "beginner-3day",
                Name = "Başlangıç Programı",
                Description = "Haftada 3 gün, temel hareketlerle full body antrenman",
                DaysPerWeek = 3,
                Goal = "general",
                Level = "beginner",
                Suitable = "Spora yeni başlayanlar için ideal"
            },
            new {
                Id = "muscle-4day",
                Name = "Kas Geliştirme",
                Description = "Haftada 4 gün, Upper-Lower split program",
                DaysPerWeek = 4,
                Goal = "muscle",
                Level = "intermediate",
                Suitable = "Kas kütlesi artırmak isteyenler için"
            },
            new {
                Id = "strength-4day",
                Name = "Güç Programı",
                Description = "Haftada 4 gün, compound hareketlere odaklı",
                DaysPerWeek = 4,
                Goal = "strength",
                Level = "intermediate",
                Suitable = "Güç artışı hedefleyenler için"
            },
            new {
                Id = "ppl-6day",
                Name = "Push-Pull-Legs x2",
                Description = "Haftada 6 gün, yoğun hacim programı",
                DaysPerWeek = 6,
                Goal = "muscle",
                Level = "advanced",
                Suitable = "Deneyimli sporcular için"
            },
            new {
                Id = "weightloss-3day",
                Name = "Yağ Yakımı",
                Description = "Haftada 3 gün, metabolizmayı hızlandıran program",
                DaysPerWeek = 3,
                Goal = "weight_loss",
                Level = "beginner",
                Suitable = "Kilo vermek isteyenler için"
            }
        };

        return Ok(templates);
    }
}

