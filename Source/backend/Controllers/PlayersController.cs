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
public class PlayersController : ControllerBase
{
    private readonly ILogger<PlayersController> _logger;

    public PlayersController(ILogger<PlayersController> logger)
    {
        _logger = logger;
    }

    private PlayerService GetPlayerService()
    {
        // Hämta team-specifik databasanslutning från middleware
        var teamConnection = HttpContext.Items["TeamDatabaseConnection"] as IDbConnection;
        
        if (teamConnection == null)
        {
            _logger.LogWarning("Team-databasanslutning hittades inte, använder global anslutning");
            // Fallback till global anslutning
            var globalConnection = HttpContext.RequestServices.GetRequiredService<IDbConnection>();
            var globalPlayerRepo = new PlayerRepository(globalConnection);
            var globalPositionRepo = new PositionRepository(globalConnection);
            return new PlayerService(globalPlayerRepo, globalPositionRepo);
        }

        // Använd team-specifik anslutning
        var playerRepository = new PlayerRepository(teamConnection);
        var positionRepository = new PositionRepository(teamConnection);
        return new PlayerService(playerRepository, positionRepository);
    }

    [HttpGet]
    public async Task<ActionResult<List<Player>>> GetAllPlayers()
    {
        var playerService = GetPlayerService();
        var players = await playerService.GetAllPlayersAsync();
        
        _logger.LogInformation("Hämtade {Count} spelare från team-databas", players.Count);
        return Ok(players);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Player>> GetPlayerById(int id)
    {
        var playerService = GetPlayerService();
        var player = await playerService.GetPlayerByIdAsync(id);
        if (player == null)
        {
            return NotFound();
        }
        return Ok(player);
    }

    [HttpPost]
    public async Task<ActionResult<Player>> CreatePlayer(Player player)
    {
        try
        {
            var playerService = GetPlayerService();
            var createdPlayer = await playerService.CreatePlayerAsync(player);
            
            _logger.LogInformation("Skapade spelare {Name} i team-databas", createdPlayer.Name);
            return CreatedAtAction(nameof(GetPlayerById), new { id = createdPlayer.Id }, createdPlayer);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Player>> UpdatePlayer(int id, Player player)
    {
        if (id != player.Id)
        {
            return BadRequest("ID mismatch");
        }

        try
        {
            var playerService = GetPlayerService();
            var updatedPlayer = await playerService.UpdatePlayerAsync(player);
            
            _logger.LogInformation("Uppdaterade spelare {Name} i team-databas", updatedPlayer.Name);
            return Ok(updatedPlayer);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeletePlayer(int id)
    {
        var playerService = GetPlayerService();
        await playerService.DeletePlayerAsync(id);
        
        _logger.LogInformation("Tog bort spelare med ID {Id} från team-databas", id);
        return NoContent();
    }
} 