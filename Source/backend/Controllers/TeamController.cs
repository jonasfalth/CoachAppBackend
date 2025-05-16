using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using CoachBackend.Models;
using CoachBackend.Services;

namespace CoachBackend.Controllers;

/// <summary>
/// Controller för att hantera team-relaterade operationer
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class TeamController : ControllerBase
{
    private readonly TeamService _teamService;

    public TeamController(TeamService teamService)
    {
        _teamService = teamService;
    }

    /// <summary>
    /// Hämtar alla team som den inloggade användaren tillhör
    /// </summary>
    /// <returns>En lista med team</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<Team>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTeams()
    {
        var userIdClaim = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (userIdClaim == null)
        {
            return Unauthorized("Användar-ID hittades inte i token");
        }

        var userId = int.Parse(userIdClaim.Value);
        var teams = await _teamService.GetTeamsByUserIdAsync(userId);
        return Ok(teams);
    }
} 