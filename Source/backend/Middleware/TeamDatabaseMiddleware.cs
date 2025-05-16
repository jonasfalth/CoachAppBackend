using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using CoachBackend.Services;

namespace CoachBackend.Middleware;

public class TeamDatabaseMiddleware
{
    private readonly RequestDelegate _next;
    private readonly TeamDatabaseService _teamDatabaseService;

    public TeamDatabaseMiddleware(RequestDelegate next, TeamDatabaseService teamDatabaseService)
    {
        _next = next;
        _teamDatabaseService = teamDatabaseService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skippa middleware för /api/team och /api/Auth endpoints
        if (context.Request.Path.StartsWithSegments("/api/team", StringComparison.OrdinalIgnoreCase) || 
            context.Request.Path.StartsWithSegments("/api/auth", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // Skippa middleware för /api/Auth endpoints
        if (context.Request.Path.StartsWithSegments("/api/auth", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // Om man inte är autentiserad, returnera 403
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("Not authenticated");
            return;
        }

        var teamIdClaim = context.User.FindFirst("TeamId");
        if (teamIdClaim == null || !int.TryParse(teamIdClaim.Value, out int teamId))
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("No team selected");
            return;
        }

        var databaseName = await _teamDatabaseService.GetDatabaseNameForTeamAsync(teamId);
        if (string.IsNullOrEmpty(databaseName))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Invalid team");
            return;
        }

        // Sätt database name i context items så att repositories kan använda det
        context.Items["DatabaseName"] = databaseName;

        await _next(context);
    }
} 