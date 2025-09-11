using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CoachBackend.Models;
using CoachBackend.Repositories;
using System.Data;

namespace CoachBackend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MatchEventsController : ControllerBase
{
    private readonly ILogger<MatchEventsController> _logger;

    public MatchEventsController(ILogger<MatchEventsController> logger)
    {
        _logger = logger;
    }

    private MatchEventRepository GetMatchEventRepository()
    {
        var teamConnection = HttpContext.Items["TeamDatabaseConnection"] as IDbConnection;
        
        if (teamConnection == null)
        {
            var globalConnection = HttpContext.RequestServices.GetRequiredService<IDbConnection>();
            return new MatchEventRepository(globalConnection);
        }

        return new MatchEventRepository(teamConnection);
    }

    [HttpGet]
    public async Task<ActionResult<List<MatchEvent>>> GetAllMatchEvents()
    {
        var repository = GetMatchEventRepository();
        var matchEvents = await repository.GetAllMatchEventsAsync();
        
        _logger.LogInformation("Hämtade {Count} match events", matchEvents.Count);
        return Ok(matchEvents);
    }

    [HttpGet("currentmatch/{currentMatchId}")]
    public async Task<ActionResult<List<MatchEvent>>> GetMatchEventsByCurrentMatchId(int currentMatchId)
    {
        var repository = GetMatchEventRepository();
        var matchEvents = await repository.GetMatchEventsByCurrentMatchIdAsync(currentMatchId);
        
        _logger.LogInformation("Hämtade {Count} match events för aktuell match {CurrentMatchId}", matchEvents.Count, currentMatchId);
        return Ok(matchEvents);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MatchEvent>> GetMatchEventById(int id)
    {
        var repository = GetMatchEventRepository();
        var matchEvent = await repository.GetMatchEventByIdAsync(id);
        if (matchEvent == null)
        {
            return NotFound();
        }
        return Ok(matchEvent);
    }

    [HttpPost]
    public async Task<ActionResult<MatchEvent>> CreateMatchEvent(MatchEvent matchEvent)
    {
        try
        {
            var repository = GetMatchEventRepository();
            var createdEvent = await repository.CreateMatchEventAsync(matchEvent);
            
            _logger.LogInformation("Skapade match event {EventType} för match {CurrentMatchId}", 
                createdEvent.EventType, createdEvent.CurrentMatchId);
            return CreatedAtAction(nameof(GetMatchEventById), new { id = createdEvent.Id }, createdEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid skapande av match event");
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<MatchEvent>> UpdateMatchEvent(int id, MatchEvent matchEvent)
    {
        if (id != matchEvent.Id)
        {
            return BadRequest("ID mismatch");
        }

        try
        {
            var repository = GetMatchEventRepository();
            var updatedEvent = await repository.UpdateMatchEventAsync(matchEvent);
            
            _logger.LogInformation("Uppdaterade match event {Id}", updatedEvent.Id);
            return Ok(updatedEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid uppdatering av match event {Id}", id);
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMatchEvent(int id)
    {
        var repository = GetMatchEventRepository();
        await repository.DeleteMatchEventAsync(id);
        
        _logger.LogInformation("Tog bort match event {Id}", id);
        return NoContent();
    }
} 