using System.Data;
using Microsoft.Data.Sqlite;
using CoachBackend.Models;

namespace CoachBackend.Repositories;

public class MatchRepository : BaseRepository
{
    public MatchRepository(IDbConnection connection) : base(connection) { }

    public async Task<List<Match>> GetAllMatchesAsync()
    {
        var matches = new List<Match>();
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = @"
                SELECT Id, Date, Opponent, HomeGame, Result, Notes 
                FROM Matches";
            
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    matches.Add(new Match
                    {
                        Id = reader.GetInt32(0),
                        Date = DateTime.Parse(reader.GetString(1)),
                        Opponent = reader.GetString(2),
                        HomeGame = reader.GetBoolean(3),
                        Result = reader.IsDBNull(4) ? null : reader.GetString(4),
                        Notes = reader.IsDBNull(5) ? null : reader.GetString(5)
                    });
                }
            }
        }
        return matches;
    }

    public async Task<Match?> GetMatchByIdAsync(int id)
    {
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = @"
                SELECT Id, Date, Opponent, HomeGame, Result, Notes 
                FROM Matches 
                WHERE Id = @Id";
            
            command.Parameters.AddWithValue("@Id", id);

            using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    return new Match
                    {
                        Id = reader.GetInt32(0),
                        Date = DateTime.Parse(reader.GetString(1)),
                        Opponent = reader.GetString(2),
                        HomeGame = reader.GetBoolean(3),
                        Result = reader.IsDBNull(4) ? null : reader.GetString(4),
                        Notes = reader.IsDBNull(5) ? null : reader.GetString(5)
                    };
                }
            }
        }
        return null;
    }

    public async Task<Match> CreateMatchAsync(Match match)
    {
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = @"
                INSERT INTO Matches (Date, Opponent, HomeGame, Result, Notes)
                VALUES (@Date, @Opponent, @HomeGame, @Result, @Notes);
                SELECT last_insert_rowid();";

            command.Parameters.AddWithValue("@Date", match.Date.ToString("o"));
            command.Parameters.AddWithValue("@Opponent", match.Opponent);
            command.Parameters.AddWithValue("@HomeGame", match.HomeGame);
            command.Parameters.AddWithValue("@Result", (object?)match.Result ?? DBNull.Value);
            command.Parameters.AddWithValue("@Notes", (object?)match.Notes ?? DBNull.Value);

            match.Id = Convert.ToInt32(await command.ExecuteScalarAsync());
            return match;
        }
    }

    public async Task<Match> UpdateMatchAsync(Match match)
    {
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = @"
                UPDATE Matches 
                SET Date = @Date, 
                    Opponent = @Opponent,
                    HomeGame = @HomeGame,
                    Result = @Result,
                    Notes = @Notes
                WHERE Id = @Id";

            command.Parameters.AddWithValue("@Id", match.Id);
            command.Parameters.AddWithValue("@Date", match.Date.ToString("o"));
            command.Parameters.AddWithValue("@Opponent", match.Opponent);
            command.Parameters.AddWithValue("@HomeGame", match.HomeGame);
            command.Parameters.AddWithValue("@Result", (object?)match.Result ?? DBNull.Value);
            command.Parameters.AddWithValue("@Notes", (object?)match.Notes ?? DBNull.Value);

            await command.ExecuteNonQueryAsync();
            return match;
        }
    }

    public async Task DeleteMatchAsync(int id)
    {
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = "DELETE FROM Matches WHERE Id = @Id";
            command.Parameters.AddWithValue("@Id", id);
            await command.ExecuteNonQueryAsync();
        }
    }
} 