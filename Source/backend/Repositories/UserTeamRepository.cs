using System.Data;
using Microsoft.Data.Sqlite;
using CoachBackend.Models;

namespace CoachBackend.Repositories;

public class UserTeamRepository : BaseRepository
{
    public UserTeamRepository(IDbConnection connection) : base(connection) { }

    public async Task AddUserToTeamAsync(int userId, int teamId)
    {
        EnsureConnectionOpen();
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = @"
                INSERT INTO UserTeams (UserId, TeamId, Role, JoinedAt, CreatedAt)
                VALUES (@UserId, @TeamId, @Role, @JoinedAt, @CreatedAt)";

            var now = DateTime.UtcNow.ToString("o");
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@TeamId", teamId);
            command.Parameters.AddWithValue("@Role", "Member");
            command.Parameters.AddWithValue("@JoinedAt", now);
            command.Parameters.AddWithValue("@CreatedAt", now);

            await command.ExecuteNonQueryAsync();
        }
    }

    public async Task RemoveUserFromTeamAsync(int userId, int teamId)
    {
        EnsureConnectionOpen();
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = "DELETE FROM UserTeams WHERE UserId = @UserId AND TeamId = @TeamId";
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@TeamId", teamId);
            await command.ExecuteNonQueryAsync();
        }
    }

    public async Task<bool> IsUserInTeamAsync(int userId, int teamId)
    {
        EnsureConnectionOpen();
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = "SELECT COUNT(*) FROM UserTeams WHERE UserId = @UserId AND TeamId = @TeamId";
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@TeamId", teamId);

            var count = Convert.ToInt32(await command.ExecuteScalarAsync());
            return count > 0;
        }
    }

    public async Task<List<User>> GetUsersByTeamIdAsync(int teamId)
    {
        EnsureConnectionOpen();
        var users = new List<User>();
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = @"
                SELECT u.Id, u.Username, u.Email, u.LastLogin, u.LastUpdated, u.CreatedAt
                FROM Users u
                JOIN UserTeams ut ON u.Id = ut.UserId
                WHERE ut.TeamId = @TeamId";
            
            command.Parameters.AddWithValue("@TeamId", teamId);

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    users.Add(new User
                    {
                        Id = reader.GetInt32(0),
                        Username = reader.GetString(1),
                        Email = reader.GetString(2),
                        LastLogin = DateTime.Parse(reader.GetString(3)),
                        LastUpdated = DateTime.Parse(reader.GetString(4)),
                        CreatedAt = DateTime.Parse(reader.GetString(5))
                    });
                }
            }
        }
        return users;
    }

    public async Task<List<UserTeam>> GetAllUserTeamsAsync()
    {
        EnsureConnectionOpen();
        var userTeams = new List<UserTeam>();
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = "SELECT UserId, TeamId, Role, JoinedAt, CreatedAt FROM UserTeams";
            
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    userTeams.Add(new UserTeam
                    {
                        UserId = reader.GetInt32(0),
                        TeamId = reader.GetInt32(1),
                        Role = reader.GetString(2),
                        JoinedAt = DateTime.Parse(reader.GetString(3)),
                        CreatedAt = DateTime.Parse(reader.GetString(4))
                    });
                }
            }
        }
        return userTeams;
    }

    public async Task<List<UserTeam>> GetUserTeamsByUserIdAsync(int userId)
    {
        EnsureConnectionOpen();
        var userTeams = new List<UserTeam>();
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = "SELECT UserId, TeamId, Role, JoinedAt, CreatedAt FROM UserTeams WHERE UserId = @UserId";
            command.Parameters.AddWithValue("@UserId", userId);
            
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    userTeams.Add(new UserTeam
                    {
                        UserId = reader.GetInt32(0),
                        TeamId = reader.GetInt32(1),
                        Role = reader.GetString(2),
                        JoinedAt = DateTime.Parse(reader.GetString(3)),
                        CreatedAt = DateTime.Parse(reader.GetString(4))
                    });
                }
            }
        }
        return userTeams;
    }

    public async Task<List<UserTeam>> GetUserTeamsByTeamIdAsync(int teamId)
    {
        EnsureConnectionOpen();
        var userTeams = new List<UserTeam>();
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = "SELECT UserId, TeamId, Role, JoinedAt, CreatedAt FROM UserTeams WHERE TeamId = @TeamId";
            command.Parameters.AddWithValue("@TeamId", teamId);
            
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    userTeams.Add(new UserTeam
                    {
                        UserId = reader.GetInt32(0),
                        TeamId = reader.GetInt32(1),
                        Role = reader.GetString(2),
                        JoinedAt = DateTime.Parse(reader.GetString(3)),
                        CreatedAt = DateTime.Parse(reader.GetString(4))
                    });
                }
            }
        }
        return userTeams;
    }

    public async Task<UserTeam> AddUserToTeamAsync(UserTeam userTeam)
    {
        EnsureConnectionOpen();
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = @"
                INSERT INTO UserTeams (UserId, TeamId, Role, JoinedAt, CreatedAt)
                VALUES (@UserId, @TeamId, @Role, @JoinedAt, @CreatedAt)";

            command.Parameters.AddWithValue("@UserId", userTeam.UserId);
            command.Parameters.AddWithValue("@TeamId", userTeam.TeamId);
            command.Parameters.AddWithValue("@Role", userTeam.Role);
            command.Parameters.AddWithValue("@JoinedAt", userTeam.JoinedAt.ToString("o"));
            command.Parameters.AddWithValue("@CreatedAt", userTeam.CreatedAt.ToString("o"));

            await command.ExecuteNonQueryAsync();
            return userTeam;
        }
    }

    public async Task UpdateUserTeamRoleAsync(int userId, int teamId, string role)
    {
        EnsureConnectionOpen();
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = @"
                UPDATE UserTeams
                SET Role = @Role
                WHERE UserId = @UserId AND TeamId = @TeamId";

            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@TeamId", teamId);
            command.Parameters.AddWithValue("@Role", role);

            await command.ExecuteNonQueryAsync();
        }
    }
} 