using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CoachBackend.Models;
using CoachBackend.Repositories;
using System.Data;

namespace CoachBackend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FieldPositionsController : ControllerBase
{
    private readonly ILogger<FieldPositionsController> _logger;

    public FieldPositionsController(ILogger<FieldPositionsController> logger)
    {
        _logger = logger;
    }

    private FieldPositionRepository GetFieldPositionRepository()
    {
        var teamConnection = HttpContext.Items["TeamDatabaseConnection"] as IDbConnection;
        
        if (teamConnection == null)
        {
            var globalConnection = HttpContext.RequestServices.GetRequiredService<IDbConnection>();
            return new FieldPositionRepository(globalConnection);
        }

        return new FieldPositionRepository(teamConnection);
    }

    [HttpGet]
    public async Task<ActionResult<List<FieldPosition>>> GetAllFieldPositions()
    {
        var repository = GetFieldPositionRepository();
        var fieldPositions = await repository.GetAllFieldPositionsAsync();
        
        _logger.LogInformation("Hämtade {Count} fältpositioner", fieldPositions.Count);
        return Ok(fieldPositions);
    }

    [HttpGet("zone/{zone}")]
    public async Task<ActionResult<List<FieldPosition>>> GetFieldPositionsByZone(string zone)
    {
        var repository = GetFieldPositionRepository();
        var fieldPositions = await repository.GetFieldPositionsByZoneAsync(zone);
        
        _logger.LogInformation("Hämtade {Count} fältpositioner för zon {Zone}", fieldPositions.Count, zone);
        return Ok(fieldPositions);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FieldPosition>> GetFieldPositionById(int id)
    {
        var repository = GetFieldPositionRepository();
        var fieldPosition = await repository.GetFieldPositionByIdAsync(id);
        if (fieldPosition == null)
        {
            return NotFound();
        }
        return Ok(fieldPosition);
    }
} 