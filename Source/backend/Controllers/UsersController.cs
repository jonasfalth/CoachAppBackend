using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CoachBackend.Models;
using CoachBackend.Services;
using CoachBackend.Authentication;
using BCrypt.Net;

namespace CoachBackend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;
    private readonly IAuthenticationService _authService;

    public UsersController(UserService userService, IAuthenticationService authService)
    {
        _userService = userService;
        _authService = authService;
    }

    // GET: api/Users
    [HttpGet]
    public async Task<ActionResult<List<User>>> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    // GET: api/Users/5
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUserById(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        
        // Ta bort lösenordshashet innan användarobjektet returneras till klienten
        user.PasswordHash = null!;
        return Ok(user);
    }

    // GET: api/Users/username/{username}
    [HttpGet("username/{username}")]
    public async Task<ActionResult<User>> GetUserByUsername(string username)
    {
        var user = await _userService.GetUserByUsernameAsync(username);
        if (user == null)
        {
            return NotFound();
        }
        
        // Ta bort lösenordshashet innan användarobjektet returneras till klienten
        user.PasswordHash = null!;
        return Ok(user);
    }

    // POST: api/Users
    [HttpPost]
    public async Task<ActionResult<User>> CreateUser(User user)
    {
        try
        {
            var createdUser = await _userService.CreateUserAsync(user);
            
            // Ta bort lösenordshashet innan användarobjektet returneras till klienten
            createdUser.PasswordHash = null!;
            return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createdUser);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // PUT: api/Users/5
    [HttpPut("{id}")]
    public async Task<ActionResult<User>> UpdateUser(int id, User user)
    {
        if (id != user.Id)
        {
            return BadRequest("ID i URL matchar inte ID i request body");
        }

        try
        {
            var updatedUser = await _userService.UpdateUserAsync(user);
            
            // Ta bort lösenordshashet innan användarobjektet returneras till klienten
            updatedUser.PasswordHash = null!;
            return Ok(updatedUser);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // PUT: api/Users/5/password
    [HttpPut("{id}/password")]
    public async Task<ActionResult> UpdateUserPassword(int id, [FromBody] string passwordHash)
    {
        try
        {
            await _userService.UpdateUserPasswordAsync(id, passwordHash);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // PUT: api/Users/5/login
    [HttpPut("{id}/login")]
    public async Task<ActionResult> UpdateLastLogin(int id)
    {
        try
        {
            await _userService.UpdateLastLoginAsync(id);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // DELETE: api/Users/5
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteUser(int id)
    {
        try
        {
            await _userService.DeleteUserAsync(id);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // POST: api/Users/register
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<User>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            // Kontrollera om användarnamnet redan finns
            var existingUser = await _userService.GetUserByUsernameAsync(request.Username);
            if (existingUser != null)
            {
                return BadRequest("Användarnamnet är redan taget");
            }

            // Skapa ny användare
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
                LastLogin = DateTime.UtcNow
            };

            var createdUser = await _userService.CreateUserAsync(user);
            
            // Ta bort lösenordshashet innan användarobjektet returneras till klienten
            createdUser.PasswordHash = null!;
            return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createdUser);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
} 