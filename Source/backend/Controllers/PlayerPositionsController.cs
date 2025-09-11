using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CoachBackend.Models;
using CoachBackend.Repositories;
using System.Data;

namespace CoachBackend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PlayerPositionsController : ControllerBase
{
    private readonly ILogger<PlayerPositionsController> _logger;

    public PlayerPositionsController(ILogger<PlayerPositionsController> logger)
    {
        _logger = logger;
    }

    private PlayerPositionRepository GetPlayerPositionRepository()
    {
        var teamConnection = HttpContext.Items["TeamDatabaseConnection"] as IDbConnection;
        
        if (teamConnection == null)
        {
            var globalConnection = HttpContext.RequestServices.GetRequiredService<IDbConnection>();
            return new PlayerPositionRepository(globalConnection);
        }

        return new PlayerPositionRepository(teamConnection);
    }

    [HttpGet("currentmatch/{currentMatchId}")]
    public async Task<ActionResult<List<PlayerPosition>>> GetPlayerPositionsByCurrentMatchId(int currentMatchId)
    {
        var repository = GetPlayerPositionRepository();
        var playerPositions = await repository.GetPlayerPositionsByCurrentMatchIdAsync(currentMatchId);
        
        _logger.LogInformation("Hämtade {Count} spelarpositioner för aktuell match {CurrentMatchId}", 
            playerPositions.Count, currentMatchId);
        return Ok(playerPositions);
    }

    [HttpGet("currentmatch/{currentMatchId}/active")]
    public async Task<ActionResult<List<PlayerPosition>>> GetActivePlayerPositions(int currentMatchId)
    {
        var repository = GetPlayerPositionRepository();
        var playerPositions = await repository.GetActivePlayerPositionsAsync(currentMatchId);
        
        _logger.LogInformation("Hämtade {Count} aktiva spelarpositioner för match {CurrentMatchId}", 
            playerPositions.Count, currentMatchId);
        return Ok(playerPositions);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PlayerPosition>> GetPlayerPositionById(int id)
    {
        var repository = GetPlayerPositionRepository();
        var playerPosition = await repository.GetPlayerPositionByIdAsync(id);
        if (playerPosition == null)
        {
            return NotFound();
        }
        return Ok(playerPosition);
    }

    [HttpPost]
    public async Task<ActionResult<PlayerPosition>> CreatePlayerPosition(PlayerPosition playerPosition)
    {
        try
        {
            var repository = GetPlayerPositionRepository();
            var createdPosition = await repository.CreatePlayerPositionAsync(playerPosition);
            
            _logger.LogInformation("Skapade spelarposition för spelare {PlayerId} i match {CurrentMatchId}", 
                createdPosition.PlayerId, createdPosition.CurrentMatchId);
            return CreatedAtAction(nameof(GetPlayerPositionById), new { id = createdPosition.Id }, createdPosition);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid skapande av spelarposition");
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PlayerPosition>> UpdatePlayerPosition(int id, PlayerPosition playerPosition)
    {
        if (id != playerPosition.Id)
        {
            return BadRequest("ID mismatch");
        }

        try
        {
            var repository = GetPlayerPositionRepository();
            var updatedPosition = await repository.UpdatePlayerPositionAsync(playerPosition);
            
            _logger.LogInformation("Uppdaterade spelarposition {Id}", updatedPosition.Id);
            return Ok(updatedPosition);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid uppdatering av spelarposition {Id}", id);
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeletePlayerPosition(int id)
    {
        var repository = GetPlayerPositionRepository();
        await repository.DeletePlayerPositionAsync(id);
        
        _logger.LogInformation("Tog bort spelarposition {Id}", id);
        return NoContent();
    }
} 