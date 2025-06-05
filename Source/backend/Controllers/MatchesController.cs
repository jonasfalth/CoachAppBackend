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
public class MatchesController : ControllerBase
{
    private readonly ILogger<MatchesController> _logger;

    public MatchesController(ILogger<MatchesController> logger)
    {
        _logger = logger;
    }

    private MatchService GetMatchService()
    {
        // Hämta team-specifik databasanslutning från middleware
        var teamConnection = HttpContext.Items["TeamDatabaseConnection"] as IDbConnection;
        
        if (teamConnection == null)
        {
            _logger.LogWarning("Team-databasanslutning hittades inte, använder global anslutning");
            // Fallback till global anslutning
            var globalConnection = HttpContext.RequestServices.GetRequiredService<IDbConnection>();
            var globalMatchRepo = new MatchRepository(globalConnection);
            return new MatchService(globalMatchRepo);
        }

        // Använd team-specifik anslutning
        var matchRepository = new MatchRepository(teamConnection);
        return new MatchService(matchRepository);
    }

    [HttpGet]
    public async Task<ActionResult<List<Match>>> GetAllMatches()
    {
        var matchService = GetMatchService();
        var matches = await matchService.GetAllMatchesAsync();
        
        _logger.LogInformation("Hämtade {Count} matcher från team-databas", matches.Count);
        return Ok(matches);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Match>> GetMatchById(int id)
    {
        var matchService = GetMatchService();
        var match = await matchService.GetMatchByIdAsync(id);
        if (match == null)
        {
            return NotFound();
        }
        
        return Ok(match);
    }

    [HttpPost]
    public async Task<ActionResult<Match>> CreateMatch(Match match)
    {
        try
        {
            var matchService = GetMatchService();
            var createdMatch = await matchService.CreateMatchAsync(match);
            
            _logger.LogInformation("Skapade match {Title} i team-databas", createdMatch.Opponent);
            return CreatedAtAction(nameof(GetMatchById), new { id = createdMatch.Id }, createdMatch);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Match>> UpdateMatch(int id, Match match)
    {
        if (id != match.Id)
        {
            return BadRequest("ID i URL matchar inte ID i request body");
        }

        try
        {
            var matchService = GetMatchService();
            var updatedMatch = await matchService.UpdateMatchAsync(match);
            
            _logger.LogInformation("Uppdaterade match {Title} i team-databas", updatedMatch.Opponent);
            return Ok(updatedMatch);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMatch(int id)
    {
        try
        {
            var matchService = GetMatchService();
            await matchService.DeleteMatchAsync(id);
            
            _logger.LogInformation("Tog bort match med ID {Id} från team-databas", id);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
} 