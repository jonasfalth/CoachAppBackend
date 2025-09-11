using System.Data;
using Microsoft.Data.Sqlite;
using CoachBackend.Models;

namespace CoachBackend.Repositories;

public class CurrentMatchRepository : BaseRepository
{
    public CurrentMatchRepository(IDbConnection connection) : base(connection) { }

    public async Task<CurrentMatch?> GetActiveCurrentMatchAsync()
    {
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = @"
                SELECT Id, MatchId, Status, MatchStartTime, FirstHalfStartTime, SecondHalfStartTime, 
                       LastPauseTime, FirstHalfDurationSeconds, SecondHalfDurationSeconds, TotalPauseSeconds,
                       HomeScore, AwayScore, Formation, CreatedAt, UpdatedAt
                FROM CurrentMatch 
                WHERE Status != 'finished'
                ORDER BY CreatedAt DESC 
                LIMIT 1";

            using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    return MapCurrentMatch(reader);
                }
            }
        }
        return null;
    }

    public async Task<CurrentMatch?> GetCurrentMatchByIdAsync(int id)
    {
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = @"
                SELECT Id, MatchId, Status, MatchStartTime, FirstHalfStartTime, SecondHalfStartTime, 
                       LastPauseTime, FirstHalfDurationSeconds, SecondHalfDurationSeconds, TotalPauseSeconds,
                       HomeScore, AwayScore, Formation, CreatedAt, UpdatedAt
                FROM CurrentMatch 
                WHERE Id = @Id";

            command.Parameters.AddWithValue("@Id", id);

            using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    return MapCurrentMatch(reader);
                }
            }
        }
        return null;
    }

    public async Task<CurrentMatch> CreateCurrentMatchAsync(CurrentMatch currentMatch)
    {
        var now = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = @"
                INSERT INTO CurrentMatch (MatchId, Status, MatchStartTime, FirstHalfStartTime, SecondHalfStartTime,
                                        LastPauseTime, FirstHalfDurationSeconds, SecondHalfDurationSeconds, TotalPauseSeconds,
                                        HomeScore, AwayScore, Formation, CreatedAt, UpdatedAt)
                VALUES (@MatchId, @Status, @MatchStartTime, @FirstHalfStartTime, @SecondHalfStartTime,
                        @LastPauseTime, @FirstHalfDurationSeconds, @SecondHalfDurationSeconds, @TotalPauseSeconds,
                        @HomeScore, @AwayScore, @Formation, @CreatedAt, @UpdatedAt);
                SELECT last_insert_rowid();";

            AddCurrentMatchParameters(command, currentMatch, now);
            currentMatch.Id = Convert.ToInt32(await command.ExecuteScalarAsync());
            return currentMatch;
        }
    }

    public async Task<CurrentMatch> UpdateCurrentMatchAsync(CurrentMatch currentMatch)
    {
        var now = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = @"
                UPDATE CurrentMatch 
                SET MatchId = @MatchId, Status = @Status, MatchStartTime = @MatchStartTime,
                    FirstHalfStartTime = @FirstHalfStartTime, SecondHalfStartTime = @SecondHalfStartTime,
                    LastPauseTime = @LastPauseTime, FirstHalfDurationSeconds = @FirstHalfDurationSeconds,
                    SecondHalfDurationSeconds = @SecondHalfDurationSeconds, TotalPauseSeconds = @TotalPauseSeconds,
                    HomeScore = @HomeScore, AwayScore = @AwayScore, Formation = @Formation, UpdatedAt = @UpdatedAt
                WHERE Id = @Id";

            command.Parameters.AddWithValue("@Id", currentMatch.Id);
            AddCurrentMatchParameters(command, currentMatch, now);
            
            await command.ExecuteNonQueryAsync();
            return currentMatch;
        }
    }

    private CurrentMatch MapCurrentMatch(SqliteDataReader reader)
    {
        return new CurrentMatch
        {
            Id = reader.GetInt32(0),
            MatchId = reader.GetInt32(1),
            Status = reader.GetString(2),
            MatchStartTime = reader.IsDBNull(3) ? null : DateTime.Parse(reader.GetString(3)),
            FirstHalfStartTime = reader.IsDBNull(4) ? null : DateTime.Parse(reader.GetString(4)),
            SecondHalfStartTime = reader.IsDBNull(5) ? null : DateTime.Parse(reader.GetString(5)),
            LastPauseTime = reader.IsDBNull(6) ? null : DateTime.Parse(reader.GetString(6)),
            FirstHalfDurationSeconds = reader.GetInt32(7),
            SecondHalfDurationSeconds = reader.GetInt32(8),
            TotalPauseSeconds = reader.GetInt32(9),
            HomeScore = reader.GetInt32(10),
            AwayScore = reader.GetInt32(11),
            Formation = reader.IsDBNull(12) ? null : reader.GetString(12),
            CreatedAt = DateTime.Parse(reader.GetString(13)),
            UpdatedAt = DateTime.Parse(reader.GetString(14))
        };
    }

    private void AddCurrentMatchParameters(SqliteCommand command, CurrentMatch currentMatch, string now)
    {
        command.Parameters.AddWithValue("@MatchId", currentMatch.MatchId);
        command.Parameters.AddWithValue("@Status", currentMatch.Status);
        command.Parameters.AddWithValue("@MatchStartTime", currentMatch.MatchStartTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@FirstHalfStartTime", currentMatch.FirstHalfStartTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@SecondHalfStartTime", currentMatch.SecondHalfStartTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@LastPauseTime", currentMatch.LastPauseTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@FirstHalfDurationSeconds", currentMatch.FirstHalfDurationSeconds);
        command.Parameters.AddWithValue("@SecondHalfDurationSeconds", currentMatch.SecondHalfDurationSeconds);
        command.Parameters.AddWithValue("@TotalPauseSeconds", currentMatch.TotalPauseSeconds);
        command.Parameters.AddWithValue("@HomeScore", currentMatch.HomeScore);
        command.Parameters.AddWithValue("@AwayScore", currentMatch.AwayScore);
        command.Parameters.AddWithValue("@Formation", (object?)currentMatch.Formation ?? DBNull.Value);
        command.Parameters.AddWithValue("@CreatedAt", currentMatch.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
        command.Parameters.AddWithValue("@UpdatedAt", now);
    }
} 