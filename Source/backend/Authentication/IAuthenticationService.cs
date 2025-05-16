using System.Security.Claims;
using CoachBackend.Models;

namespace CoachBackend.Authentication;

public interface IAuthenticationService
{
    string GenerateJwtToken(User user, int? teamId);
    bool VerifyPassword(string password, string hashedPassword);
    Task<bool> ValidateUserCredentials(string username, string password);
    ClaimsPrincipal? ValidateToken(string token);
} 