using CoachBackend.Models;
using CoachBackend.Repositories;

namespace CoachBackend.Services;

public class TeamService
{
    private readonly TeamRepository _teamRepository;
    private readonly UserTeamRepository _userTeamRepository;

    public TeamService(TeamRepository teamRepository, UserTeamRepository userTeamRepository)
    {
        _teamRepository = teamRepository;
        _userTeamRepository = userTeamRepository;
    }

    public async Task<List<Team>> GetAllTeamsAsync()
    {
        return await _teamRepository.GetAllTeamsAsync();
    }

    public async Task<Team?> GetTeamByIdAsync(int id)
    {
        return await _teamRepository.GetTeamByIdAsync(id);
    }

    public async Task<Team?> GetTeamByDatabaseNameAsync(string databaseName)
    {
        return await _teamRepository.GetTeamByDatabaseNameAsync(databaseName);
    }

    public async Task<Team> CreateTeamAsync(Team team)
    {
        team.CreatedAt = DateTime.UtcNow;
        team.LastUpdated = DateTime.UtcNow;
        return await _teamRepository.CreateTeamAsync(team);
    }

    public async Task<Team> UpdateTeamAsync(Team team)
    {
        team.LastUpdated = DateTime.UtcNow;
        return await _teamRepository.UpdateTeamAsync(team);
    }

    public async Task DeleteTeamAsync(int id)
    {
        await _teamRepository.DeleteTeamAsync(id);
    }

    public async Task<List<Team>> GetTeamsByUserIdAsync(int userId)
    {
        var teams = await _teamRepository.GetTeamsByUserIdAsync(userId);
        
        // Hämta användare för varje team
        foreach (var team in teams)
        {
            team.Users = await _userTeamRepository.GetUsersByTeamIdAsync(team.Id);
        }
        
        return teams;
    }

    public async Task<List<User>> GetUsersByTeamIdAsync(int teamId)
    {
        return await _userTeamRepository.GetUsersByTeamIdAsync(teamId);
    }

    public async Task<bool> IsUserInTeamAsync(int userId, int teamId)
    {
        var teams = await _teamRepository.GetTeamsByUserIdAsync(userId);
        return teams.Any(t => t.Id == teamId);
    }
} 