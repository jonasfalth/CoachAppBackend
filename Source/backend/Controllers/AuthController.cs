using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CoachBackend.Models;
using CoachBackend.Services;
using CoachBackend.Authentication;
using System.Security.Claims;
using System.Text.Json;

namespace CoachBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{ 
    private readonly IAuthenticationService _authService;
    private readonly UserService _userService;
    private readonly TeamService _teamService;

    public AuthController(
        IAuthenticationService authService, 
        UserService userService,
        TeamService teamService)
    {
        _authService = authService;
        _userService = userService;
        _teamService = teamService;
    }

    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] LoginRequest request)
    {
        Console.WriteLine("Validate endpoint anropad!");
        Console.WriteLine($"Request: {System.Text.Json.JsonSerializer.Serialize(request)}");
        try
        {
            Console.WriteLine($"Försöker logga in användare: {request.Username}");
            Console.WriteLine($"Request body: {System.Text.Json.JsonSerializer.Serialize(request)}");
            
            var user = await _userService.GetUserByUsernameAsync(request.Username);
            if (user == null)
            {
                Console.WriteLine("Användaren hittades inte");
                return Unauthorized("Felaktigt användarnamn eller lösenord");
            }

            Console.WriteLine($"Användare hittad: {user.Username}");
            Console.WriteLine($"Lösenordshash: {user.PasswordHash}");

            // Verifiera lösenord
            if (!_authService.VerifyPassword(request.Password, user.PasswordHash))
            {
                Console.WriteLine("Felaktigt lösenord");
                return Unauthorized("Felaktigt användarnamn eller lösenord");
            }

            Console.WriteLine("Lösenord verifierat");

            // Generera temporär JWT token (utan team)
            var token = _authService.GenerateJwtToken(user, null);

            // Sätt token som en cookie
            Response.Cookies.Append("jwt", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = HttpContext.Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(1)
            });

            // Hämta användarens team
            var teams = await _teamService.GetTeamsByUserIdAsync(user.Id);

            return Ok(new LoginResponse
            {
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email
                },
                Teams = teams.Select(t => new TeamDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    DatabaseName = t.DatabaseName
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fel vid inloggning: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        // Ta bort JWT-cookien genom att sätta den till utgången
        Response.Cookies.Append("jwt", "", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(-1) // Sätt utgångsdatum till igår
        });

        return Ok(new { message = "Utloggning lyckades" });
    }

    [HttpPost("select-team")]
    [Authorize]
    public async Task<IActionResult> SelectTeam([FromBody] SelectTeamRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return Unauthorized("Användar-ID hittades inte i token");
        }

        var userId = int.Parse(userIdClaim.Value);
        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
        {
            return Unauthorized("Användaren hittades inte");
        }

        // Kontrollera att användaren tillhör teamet
        var isInTeam = await _teamService.IsUserInTeamAsync(userId, request.TeamId);
        if (!isInTeam)
        {
            return StatusCode(403, new { message = "Användaren tillhör inte detta team" });
        }

        var team = await _teamService.GetTeamByIdAsync(request.TeamId);
        if (team == null)
        {
            return NotFound("Team hittades inte");
        }

        // Generera ny JWT med team-information
        var token = _authService.GenerateJwtToken(user, team.Id);
        Console.WriteLine($"Ny JWT genererad vid teamval: {token}");

        // Sätt ny token som cookie
        Response.Cookies.Append("jwt", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = HttpContext.Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddHours(1)
        });
        Console.WriteLine("Ny JWT satt som cookie");

        return Ok(new SelectTeamResponse
        {
            Team = new TeamDto
            {
                Id = team.Id,
                Name = team.Name,
                DatabaseName = team.DatabaseName
            }
        });
    }
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public UserDto User { get; set; } = null!;
    public List<TeamDto> Teams { get; set; } = new List<TeamDto>();
}

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class TeamDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
}

public class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class SelectTeamRequest
{
    public int TeamId { get; set; }
}

public class SelectTeamResponse
{
    public TeamDto Team { get; set; } = null!;
} 