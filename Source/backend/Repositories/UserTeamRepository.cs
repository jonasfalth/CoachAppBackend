using System.Data;
using Microsoft.Data.Sqlite;
using CoachBackend.Models;

namespace CoachBackend.Repositories;

public class UserTeamRepository : BaseRepository
{
    public UserTeamRepository(IDbConnection connection) : base(connection) { }

    public async Task AddUserToTeamAsync(int userId, int teamId)
    {
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = @"
                INSERT INTO UserTeams (UserId, TeamId, CreatedAt)
                VALUES (@UserId, @TeamId, @CreatedAt)";

            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@TeamId", teamId);
            command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow.ToString("o"));

            await command.ExecuteNonQueryAsync();
        }
    }

    public async Task RemoveUserFromTeamAsync(int userId, int teamId)
    {
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
} 