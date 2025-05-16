using CoachBackend.Models;
using CoachBackend.Repositories;
using System.Data;
using Microsoft.Data.Sqlite;

namespace CoachBackend.Services;

public class TeamDatabaseService
{
    private readonly IDbConnection _connection;

    public TeamDatabaseService(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<string?> GetDatabaseNameForTeamAsync(int teamId)
    {
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = "SELECT DatabaseName FROM Teams WHERE Id = @Id";
            command.Parameters.AddWithValue("@Id", teamId);

            var result = await command.ExecuteScalarAsync();
            return result?.ToString();
        }
    }
} 