using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CoachBackend.Models;
using CoachBackend.Services;

namespace CoachBackend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DataController : ControllerBase
{
    private readonly PlayerService _playerService;
    private readonly MatchService _matchService;
    private readonly PositionService _positionService;

    public DataController(
        PlayerService playerService,
        MatchService matchService,
        PositionService positionService)
    {
        _playerService = playerService;
        _matchService = matchService;
        _positionService = positionService;
    }

    [HttpGet("all")]
    public async Task<ActionResult<AllDataResponse>> GetAllData()
    {
        var players = await _playerService.GetAllPlayersAsync();
        var matches = await _matchService.GetAllMatchesAsync();
        var positions = await _positionService.GetAllPositionsAsync();

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