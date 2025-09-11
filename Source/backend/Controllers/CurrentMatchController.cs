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
public class CurrentMatchController : ControllerBase
{
    private readonly ILogger<CurrentMatchController> _logger;

    public CurrentMatchController(ILogger<CurrentMatchController> logger)
    {
        _logger = logger;
    }

    private CurrentMatchService GetCurrentMatchService()
    {
        var teamConnection = HttpContext.Items["TeamDatabaseConnection"] as IDbConnection;
        
        if (teamConnection == null)
        {
            var globalConnection = HttpContext.RequestServices.GetRequiredService<IDbConnection>();
            var currentMatchRepo = new CurrentMatchRepository(globalConnection);
            var matchRepo = new MatchRepository(globalConnection);
            return new CurrentMatchService(currentMatchRepo, matchRepo);
        }

        var currentMatchRepository = new CurrentMatchRepository(teamConnection);
        var matchRepository = new MatchRepository(teamConnection);
        return new CurrentMatchService(currentMatchRepository, matchRepository);
    }

    [HttpGet("active")]
    public async Task<ActionResult<CurrentMatch>> GetActiveCurrentMatch()
    {
        var service = GetCurrentMatchService();
        var currentMatch = await service.GetActiveCurrentMatchAsync();
        
        if (currentMatch == null)
        {
            return NotFound();
        }
        
        _logger.LogInformation("Hämtade aktiv match {Id}", currentMatch.Id);
        return Ok(currentMatch);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CurrentMatch>> GetCurrentMatchById(int id)
    {
        var service = GetCurrentMatchService();
        var currentMatch = await service.GetCurrentMatchByIdAsync(id);
        if (currentMatch == null)
        {
            return NotFound();
        }
        return Ok(currentMatch);
    }

    [HttpPost]
    public async Task<ActionResult<CurrentMatch>> CreateCurrentMatch(CurrentMatch currentMatch)
    {
        try
        {
            var service = GetCurrentMatchService();
            var createdMatch = await service.CreateCurrentMatchAsync(currentMatch);
            
            _logger.LogInformation("Skapade ny aktuell match {Id} för match {MatchId}", 
                createdMatch.Id, createdMatch.MatchId);
            return CreatedAtAction(nameof(GetCurrentMatchById), new { id = createdMatch.Id }, createdMatch);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CurrentMatch>> UpdateCurrentMatch(int id, CurrentMatch currentMatch)
    {
        if (id != currentMatch.Id)
        {
            return BadRequest("ID mismatch");
        }

        try
        {
            var service = GetCurrentMatchService();
            var updatedMatch = await service.UpdateCurrentMatchAsync(currentMatch);
            
            _logger.LogInformation("Uppdaterade aktuell match {Id}", updatedMatch.Id);
            return Ok(updatedMatch);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
} 