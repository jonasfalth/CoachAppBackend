using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CoachBackend.Models;
using CoachBackend.Services;
using CoachBackend.Repositories;
using System.Data;

namespace CoachBackend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PositionsController : ControllerBase
{
    private readonly ILogger<PositionsController> _logger;

    public PositionsController(ILogger<PositionsController> logger)
    {
        _logger = logger;
    }

    private PositionService GetPositionService()
    {
        // Hämta team-specifik databasanslutning från middleware
        var teamConnection = HttpContext.Items["TeamDatabaseConnection"] as IDbConnection;
        
        if (teamConnection == null)
        {
            _logger.LogWarning("Team-databasanslutning hittades inte, använder global anslutning");
            // Fallback till global anslutning
            var globalConnection = HttpContext.RequestServices.GetRequiredService<IDbConnection>();
            var globalPositionRepo = new PositionRepository(globalConnection);
            return new PositionService(globalPositionRepo);
        }

        // Använd team-specifik anslutning
        var positionRepository = new PositionRepository(teamConnection);
        return new PositionService(positionRepository);
    }

    [HttpGet]
    public async Task<ActionResult<List<Position>>> GetAllPositions()
    {
        var positionService = GetPositionService();
        var positions = await positionService.GetAllPositionsAsync();
        
        _logger.LogInformation("Hämtade {Count} positioner från team-databas", positions.Count);
        return Ok(positions);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Position>> GetPositionById(int id)
    {
        var positionService = GetPositionService();
        var position = await positionService.GetPositionByIdAsync(id);
        if (position == null)
        {
            return NotFound();
        }
        return Ok(position);
    }

    [HttpPost]
    public async Task<ActionResult<Position>> CreatePosition(Position position)
    {
        try
        {
            var positionService = GetPositionService();
            var createdPosition = await positionService.CreatePositionAsync(position);
            
            _logger.LogInformation("Skapade position {Name} i team-databas", createdPosition.Name);
            return CreatedAtAction(nameof(GetPositionById), new { id = createdPosition.Id }, createdPosition);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Position>> UpdatePosition(int id, Position position)
    {
        if (id != position.Id)
        {
            return BadRequest("ID mismatch");
        }

        try
        {
            var positionService = GetPositionService();
            var updatedPosition = await positionService.UpdatePositionAsync(position);
            
            _logger.LogInformation("Uppdaterade position {Name} i team-databas", updatedPosition.Name);
            return Ok(updatedPosition);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeletePosition(int id)
    {
        var positionService = GetPositionService();
        await positionService.DeletePositionAsync(id);
        
        _logger.LogInformation("Tog bort position med ID {Id} från team-databas", id);
        return NoContent();
    }
} 