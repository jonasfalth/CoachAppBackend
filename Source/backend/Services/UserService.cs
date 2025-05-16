using CoachBackend.Models;
using CoachBackend.Repositories;

namespace CoachBackend.Services;

public class UserService
{
    private readonly UserRepository _userRepository;
    private readonly UserTeamRepository _userTeamRepository;
    private User? _currentUser;
    private Team? _currentTeam;

    public UserService(UserRepository userRepository, UserTeamRepository userTeamRepository)
    {
        _userRepository = userRepository;
        _userTeamRepository = userTeamRepository;
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _userRepository.GetAllUsersAsync();
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _userRepository.GetUserByIdAsync(id);
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _userRepository.GetUserByUsernameAsync(username);
    }

    public async Task<User> CreateUserAsync(User user)
    {
        user.CreatedAt = DateTime.UtcNow;
        user.LastUpdated = DateTime.UtcNow;
        user.LastLogin = DateTime.UtcNow;
        return await _userRepository.CreateUserAsync(user);
    }

    public async Task<User> UpdateUserAsync(User user)
    {
        user.LastUpdated = DateTime.UtcNow;
        return await _userRepository.UpdateUserAsync(user);
    }

    public async Task UpdateUserPasswordAsync(int userId, string passwordHash)
    {
        await _userRepository.UpdateUserPasswordAsync(userId, passwordHash);
    }

    public async Task UpdateLastLoginAsync(int userId)
    {
        await _userRepository.UpdateLastLoginAsync(userId);
    }

    public async Task DeleteUserAsync(int id)
    {
        await _userRepository.DeleteUserAsync(id);
    }

    public async Task<bool> ValidateUserCredentialsAsync(string username, string passwordHash)
    {
        var user = await _userRepository.GetUserByUsernameAsync(username);
        if (user == null)
        {
            return false;
        }

        return user.PasswordHash == passwordHash;
    }

    public void SetCurrentUser(User user)
    {
        _currentUser = user;
    }

    public User? GetCurrentUser()
    {
        return _currentUser;
    }

    public void SetCurrentTeam(Team team)
    {
        _currentTeam = team;
    }

    public Team? GetCurrentTeam()
    {
        return _currentTeam;
    }

    public async Task<bool> IsUserInTeamAsync(int userId, int teamId)
    {
        return await _userTeamRepository.IsUserInTeamAsync(userId, teamId);
    }

    public async Task AddUserToTeamAsync(int userId, int teamId)
    {
        await _userTeamRepository.AddUserToTeamAsync(userId, teamId);
    }

    public async Task RemoveUserFromTeamAsync(int userId, int teamId)
    {
        await _userTeamRepository.RemoveUserFromTeamAsync(userId, teamId);
    }
} 