using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CoachBackend.Models;
using CoachBackend.Services;
using CoachBackend.Repositories;

namespace CoachBackend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TeamsController : ControllerBase
{
    private readonly TeamService _teamService;
    private readonly UserService _userService;
    private readonly UserTeamRepository _userTeamRepository;

    public TeamsController(TeamService teamService, UserService userService, UserTeamRepository userTeamRepository)
    {
        _teamService = teamService;
        _userService = userService;
        _userTeamRepository = userTeamRepository;
    }

    // GET: api/Teams
    [HttpGet]
    public async Task<ActionResult<List<Team>>> GetAllTeams()
    {
        var teams = await _teamService.GetAllTeamsAsync();
        return Ok(teams);
    }

    // GET: api/Teams/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Team>> GetTeamById(int id)
    {
        var team = await _teamService.GetTeamByIdAsync(id);
        if (team == null)
        {
            return NotFound();
        }
        
        return Ok(team);
    }

    // GET: api/Teams/database/{databaseName}
    [HttpGet("database/{databaseName}")]
    public async Task<ActionResult<Team>> GetTeamByDatabaseName(string databaseName)
    {
        var team = await _teamService.GetTeamByDatabaseNameAsync(databaseName);
        if (team == null)
        {
            return NotFound();
        }
        
        return Ok(team);
    }

    // GET: api/Teams/5/users
    [HttpGet("{id}/users")]
    public async Task<ActionResult<List<User>>> GetUsersByTeamId(int id)
    {
        // Kontrollera först att teamet finns
        var team = await _teamService.GetTeamByIdAsync(id);
        if (team == null)
        {
            return NotFound("Team hittades inte");
        }

        var users = await _teamService.GetUsersByTeamIdAsync(id);
        
        // Ta bort lösenordshashen från alla användare
        foreach (var user in users)
        {
            user.PasswordHash = null!;
        }
        
        return Ok(users);
    }

    // POST: api/Teams
    [HttpPost]
    public async Task<ActionResult<Team>> CreateTeam(Team team)
    {
        try
        {
            var createdTeam = await _teamService.CreateTeamAsync(team);
            return CreatedAtAction(nameof(GetTeamById), new { id = createdTeam.Id }, createdTeam);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // PUT: api/Teams/5
    [HttpPut("{id}")]
    public async Task<ActionResult<Team>> UpdateTeam(int id, Team team)
    {
        if (id != team.Id)
        {
            return BadRequest("ID i URL matchar inte ID i request body");
        }

        try
        {
            var updatedTeam = await _teamService.UpdateTeamAsync(team);
            return Ok(updatedTeam);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // DELETE: api/Teams/5
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTeam(int id)
    {
        try
        {
            await _teamService.DeleteTeamAsync(id);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // POST: api/Teams/5/users/3
    [HttpPost("{teamId}/users/{userId}")]
    public async Task<ActionResult> AddUserToTeam(int teamId, int userId)
    {
        // Kontrollera att teamet finns
        var team = await _teamService.GetTeamByIdAsync(teamId);
        if (team == null)
        {
            return NotFound("Team hittades inte");
        }

        // Kontrollera att användaren finns
        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
        {
            return NotFound("Användare hittades inte");
        }

        try
        {
            await _userTeamRepository.AddUserToTeamAsync(userId, teamId);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // DELETE: api/Teams/5/users/3
    [HttpDelete("{teamId}/users/{userId}")]
    public async Task<ActionResult> RemoveUserFromTeam(int teamId, int userId)
    {
        // Kontrollera att teamet finns
        var team = await _teamService.GetTeamByIdAsync(teamId);
        if (team == null)
        {
            return NotFound("Team hittades inte");
        }

        // Kontrollera att användaren finns
        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
        {
            return NotFound("Användare hittades inte");
        }

        try
        {
            await _userTeamRepository.RemoveUserFromTeamAsync(userId, teamId);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
} 