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
public class DataController : ControllerBase
{
    private readonly ILogger<DataController> _logger;

    public DataController(ILogger<DataController> logger)
    {
        _logger = logger;
    }

    private PlayerService GetPlayerService(IDbConnection connection)
    {
        var playerRepository = new PlayerRepository(connection);
        var positionRepository = new PositionRepository(connection);
        return new PlayerService(playerRepository, positionRepository);
    }

    private MatchService GetMatchService(IDbConnection connection)
    {
        var matchRepository = new MatchRepository(connection);
        return new MatchService(matchRepository);
    }

    private PositionService GetPositionService(IDbConnection connection)
    {
        var positionRepository = new PositionRepository(connection);
        return new PositionService(positionRepository);
    }

    [HttpGet("all")]
    public async Task<ActionResult<AllDataResponse>> GetAllData()
    {
        // Hämta team-specifik databasanslutning från middleware
        var teamConnection = HttpContext.Items["TeamDatabaseConnection"] as IDbConnection;
        
        if (teamConnection == null)
        {
            _logger.LogWarning("Team-databasanslutning hittades inte, använder global anslutning");
            // Fallback till global anslutning
            teamConnection = HttpContext.RequestServices.GetRequiredService<IDbConnection>();
        }

        var playerService = GetPlayerService(teamConnection);
        var matchService = GetMatchService(teamConnection);
        var positionService = GetPositionService(teamConnection);

        var players = await playerService.GetAllPlayersAsync();
        var matches = await matchService.GetAllMatchesAsync();
        var positions = await positionService.GetAllPositionsAsync();

        _logger.LogInformation("Hämtade all data från team-databas: {PlayerCount} spelare, {MatchCount} matcher, {PositionCount} positioner", 
            players.Count, matches.Count, positions.Count);

        return Ok(new AllDataResponse
        {
            Players = players,
            Matches = matches,
            Positions = positions
        });
    }
}

public class AllDataResponse
{
    public IEnumerable<Player> Players { get; set; } = new List<Player>();
    public IEnumerable<Match> Matches { get; set; } = new List<Match>();
    public IEnumerable<Position> Positions { get; set; } = new List<Position>();
} 