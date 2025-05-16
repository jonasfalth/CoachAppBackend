using System.Data;
using Microsoft.Data.Sqlite;
using CoachBackend.Models;

namespace CoachBackend.Repositories;

public class TeamRepository : BaseRepository
{
    public TeamRepository(IDbConnection connection) : base(connection) { }

    public async Task<List<Team>> GetAllTeamsAsync()
    {
        var teams = new List<Team>();
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = @"
                WITH TeamUsers AS (
                    SELECT t.Id as TeamId, 
                           u.Id as UserId, 
                           u.Username, 
                           u.Email, 
                           u.LastLogin, 
                           u.LastUpdated as UserLastUpdated, 
                           u.CreatedAt as UserCreatedAt
                    FROM Teams t
                    JOIN UserTeams ut ON t.Id = ut.TeamId
                    JOIN Users u ON ut.UserId = u.Id
                )
                SELECT t.Id, t.Name, t.DatabaseName, t.CreatedAt, t.LastUpdated,
                       tu.UserId, tu.Username, tu.Email, tu.LastLogin, tu.UserLastUpdated, tu.UserCreatedAt
                FROM Teams t
                LEFT JOIN TeamUsers tu ON t.Id = tu.TeamId
                ORDER BY t.Id, tu.UserId";
            
            using (var reader = await command.ExecuteReaderAsync())
            {
                Team? currentTeam = null;
                while (await reader.ReadAsync())
                {
                    var teamId = reader.GetInt32(0);
                    
                    if (currentTeam == null || currentTeam.Id != teamId)
                    {
                        if (currentTeam != null)
                        {
                            teams.Add(currentTeam);
                        }
                        
                        currentTeam = new Team
                        {
                            Id = teamId,
                            Name = reader.GetString(1),
                            DatabaseName = reader.GetString(2),
                            CreatedAt = DateTime.Parse(reader.GetString(3)),
                            LastUpdated = DateTime.Parse(reader.GetString(4)),
                            Users = new List<User>()
                        };
                    }

                    if (!reader.IsDBNull(5)) // Om det finns en användare
                    {
                        currentTeam.Users.Add(new User
                        {
                            Id = reader.GetInt32(5),
                            Username = reader.GetString(6),
                            Email = reader.GetString(7),
                            LastLogin = DateTime.Parse(reader.GetString(8)),
                            LastUpdated = DateTime.Parse(reader.GetString(9)),
                            CreatedAt = DateTime.Parse(reader.GetString(10))
                        });
                    }
                }

                if (currentTeam != null)
                {
                    teams.Add(currentTeam);
                }
            }
        }
        return teams;
    }

    public async Task<Team?> GetTeamByIdAsync(int id)
    {
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = "SELECT Id, Name, DatabaseName, CreatedAt, LastUpdated FROM Teams WHERE Id = @Id";
            command.Parameters.AddWithValue("@Id", id);

            using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    return new Team
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        DatabaseName = reader.GetString(2),
                        CreatedAt = DateTime.Parse(reader.GetString(3)),
                        LastUpdated = DateTime.Parse(reader.GetString(4))
                    };
                }
            }
        }
        return null;
    }

    public async Task<Team?> GetTeamByDatabaseNameAsync(string databaseName)
    {
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = "SELECT Id, Name, DatabaseName, CreatedAt, LastUpdated FROM Teams WHERE DatabaseName = @DatabaseName";
            command.Parameters.AddWithValue("@DatabaseName", databaseName);

            using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    return new Team
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        DatabaseName = reader.GetString(2),
                        CreatedAt = DateTime.Parse(reader.GetString(3)),
                        LastUpdated = DateTime.Parse(reader.GetString(4))
                    };
                }
            }
        }
        return null;
    }

    public async Task<Team> CreateTeamAsync(Team team)
    {
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = @"
                INSERT INTO Teams (Name, DatabaseName, CreatedAt, LastUpdated)
                VALUES (@Name, @DatabaseName, @CreatedAt, @LastUpdated);
                SELECT last_insert_rowid();";

            command.Parameters.AddWithValue("@Name", team.Name);
            command.Parameters.AddWithValue("@DatabaseName", team.DatabaseName);
            command.Parameters.AddWithValue("@CreatedAt", team.CreatedAt.ToString("o"));
            command.Parameters.AddWithValue("@LastUpdated", team.LastUpdated.ToString("o"));

            team.Id = Convert.ToInt32(await command.ExecuteScalarAsync());
            return team;
        }
    }

    public async Task<Team> UpdateTeamAsync(Team team)
    {
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = @"
                UPDATE Teams
                SET Name = @Name,
                    DatabaseName = @DatabaseName,
                    LastUpdated = @LastUpdated
                WHERE Id = @Id";

            command.Parameters.AddWithValue("@Id", team.Id);
            command.Parameters.AddWithValue("@Name", team.Name);
            command.Parameters.AddWithValue("@DatabaseName", team.DatabaseName);
            command.Parameters.AddWithValue("@LastUpdated", team.LastUpdated.ToString("o"));

            await command.ExecuteNonQueryAsync();
            return team;
        }
    }

    public async Task DeleteTeamAsync(int id)
    {
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = "DELETE FROM Teams WHERE Id = @Id";
            command.Parameters.AddWithValue("@Id", id);
            await command.ExecuteNonQueryAsync();
        }
    }

    public async Task<List<Team>> GetTeamsByUserIdAsync(int userId)
    {
        var teams = new List<Team>();
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = @"
                WITH TeamUsers AS (
                    SELECT t.Id as TeamId, 
                           u.Id as UserId, 
                           u.Username, 
                           u.Email, 
                           u.LastLogin, 
                           u.LastUpdated as UserLastUpdated, 
                           u.CreatedAt as UserCreatedAt
                    FROM Teams t
                    JOIN UserTeams ut ON t.Id = ut.TeamId
                    JOIN Users u ON ut.UserId = u.Id
                )
                SELECT t.Id, t.Name, t.DatabaseName, t.CreatedAt, t.LastUpdated,
                       tu.UserId, tu.Username, tu.Email, tu.LastLogin, tu.UserLastUpdated, tu.UserCreatedAt
                FROM Teams t
                JOIN UserTeams ut ON t.Id = ut.TeamId
                LEFT JOIN TeamUsers tu ON t.Id = tu.TeamId
                WHERE ut.UserId = @UserId
                ORDER BY t.Id, tu.UserId";
            
            command.Parameters.AddWithValue("@UserId", userId);

            using (var reader = await command.ExecuteReaderAsync())
            {
                Team? currentTeam = null;
                while (await reader.ReadAsync())
                {
                    var teamId = reader.GetInt32(0);
                    
                    if (currentTeam == null || currentTeam.Id != teamId)
                    {
                        if (currentTeam != null)
                        {
                            teams.Add(currentTeam);
                        }
                        
                        currentTeam = new Team
                        {
                            Id = teamId,
                            Name = reader.GetString(1),
                            DatabaseName = reader.GetString(2),
                            CreatedAt = DateTime.Parse(reader.GetString(3)),
                            LastUpdated = DateTime.Parse(reader.GetString(4)),
                            Users = new List<User>()
                        };
                    }

                    if (!reader.IsDBNull(5)) // Om det finns en användare
                    {
                        currentTeam.Users.Add(new User
                        {
                            Id = reader.GetInt32(5),
                            Username = reader.GetString(6),
                            Email = reader.GetString(7),
                            LastLogin = DateTime.Parse(reader.GetString(8)),
                            LastUpdated = DateTime.Parse(reader.GetString(9)),
                            CreatedAt = DateTime.Parse(reader.GetString(10))
                        });
                    }
                }

                if (currentTeam != null)
                {
                    teams.Add(currentTeam);
                }
            }
        }
        return teams;
    }
} 