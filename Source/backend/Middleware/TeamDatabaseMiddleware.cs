using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using CoachBackend.Services;
using System.Data;
using Microsoft.Data.Sqlite;
using CoachBackend.Authentication;

namespace CoachBackend.Middleware;

public class TeamDatabaseMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TeamDatabaseMiddleware> _logger;

    public TeamDatabaseMiddleware(
        RequestDelegate next,
        ILogger<TeamDatabaseMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            var teamId = context.User.FindFirst("TeamId")?.Value;
            if (!string.IsNullOrEmpty(teamId))
            {
                _logger.LogInformation("Hittade TeamId i token: {TeamId}", teamId);
                
                // Hämta TeamService för att slå upp team-information
                var teamService = context.RequestServices.GetRequiredService<TeamService>();
                var team = await teamService.GetTeamByIdAsync(int.Parse(teamId));
                
                if (team == null)
                {
                    _logger.LogWarning("Team med ID {TeamId} hittades inte", teamId);
                }
                else
                {
                    _logger.LogInformation("Team hittat: {TeamName} med databas: {DatabaseName}", team.Name, team.DatabaseName);
                    
                    // Hämta TeamDatabaseService från service provider
                    var teamDatabaseService = context.RequestServices.GetRequiredService<TeamDatabaseService>();
                    
                    // Använd teamets faktiska DatabaseName
                    var teamConnection = await teamDatabaseService.GetTeamDatabaseConnectionAsync(team.DatabaseName);
                    
                    // Spara anslutningen i HttpContext för användning i controllers
                    context.Items["TeamDatabaseConnection"] = teamConnection;
                    
                    _logger.LogInformation("Team databasanslutning konfigurerad för {TeamName} (databas: {DatabaseName})", team.Name, team.DatabaseName);
                }
            }
            else
            {
                _logger.LogInformation("Inget TeamId hittades i token");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid konfigurering av team databasanslutning");
        }

        await _next(context);
    }
} 