using System.Data;
using Microsoft.Data.Sqlite;
using CoachBackend.Models;

namespace CoachBackend.Repositories;

public class PlayerPositionRepository : BaseRepository
{
    public PlayerPositionRepository(IDbConnection connection) : base(connection) { }

    public async Task<List<PlayerPosition>> GetPlayerPositionsByCurrentMatchIdAsync(int currentMatchId)
    {
        var playerPositions = new List<PlayerPosition>();
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = @"
                SELECT Id, CurrentMatchId, PlayerId, FieldPositionId, StartTime, EndTime, IsStarting, IsActive
                FROM PlayerPosition
                WHERE CurrentMatchId = @CurrentMatchId
                ORDER BY StartTime";
            
            command.Parameters.AddWithValue("@CurrentMatchId", currentMatchId);

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    playerPositions.Add(MapPlayerPosition(reader));
                }
            }
        }
        return playerPositions;
    }

    public async Task<List<PlayerPosition>> GetActivePlayerPositionsAsync(int currentMatchId)
    {
        var playerPositions = new List<PlayerPosition>();
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = @"
                SELECT Id, CurrentMatchId, PlayerId, FieldPositionId, StartTime, EndTime, IsStarting, IsActive
                FROM PlayerPosition
                WHERE CurrentMatchId = @CurrentMatchId AND IsActive = 1
                ORDER BY StartTime";
            
            command.Parameters.AddWithValue("@CurrentMatchId", currentMatchId);

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    playerPositions.Add(MapPlayerPosition(reader));
                }
            }
        }
        return playerPositions;
    }

    public async Task<PlayerPosition?> GetPlayerPositionByIdAsync(int id)
    {
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = @"
                SELECT Id, CurrentMatchId, PlayerId, FieldPositionId, StartTime, EndTime, IsStarting, IsActive
                FROM PlayerPosition
                WHERE Id = @Id";
            
            command.Parameters.AddWithValue("@Id", id);

            using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    return MapPlayerPosition(reader);
                }
            }
        }
        return null;
    }

    public async Task<PlayerPosition> CreatePlayerPositionAsync(PlayerPosition playerPosition)
    {
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = @"
                INSERT INTO PlayerPosition (CurrentMatchId, PlayerId, FieldPositionId, StartTime, EndTime, IsStarting, IsActive)
                VALUES (@CurrentMatchId, @PlayerId, @FieldPositionId, @StartTime, @EndTime, @IsStarting, @IsActive);
                SELECT last_insert_rowid();";

            command.Parameters.AddWithValue("@CurrentMatchId", playerPosition.CurrentMatchId);
            command.Parameters.AddWithValue("@PlayerId", playerPosition.PlayerId);
            command.Parameters.AddWithValue("@FieldPositionId", playerPosition.FieldPositionId);
            command.Parameters.AddWithValue("@StartTime", playerPosition.StartTime.ToString("yyyy-MM-dd HH:mm:ss"));
            command.Parameters.AddWithValue("@EndTime", playerPosition.EndTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@IsStarting", playerPosition.IsStarting ? 1 : 0);
            command.Parameters.AddWithValue("@IsActive", playerPosition.IsActive ? 1 : 0);

            playerPosition.Id = Convert.ToInt32(await command.ExecuteScalarAsync());
            return playerPosition;
        }
    }

    public async Task<PlayerPosition> UpdatePlayerPositionAsync(PlayerPosition playerPosition)
    {
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = @"
                UPDATE PlayerPosition
                SET CurrentMatchId = @CurrentMatchId,
                    PlayerId = @PlayerId,
                    FieldPositionId = @FieldPositionId,
                    StartTime = @StartTime,
                    EndTime = @EndTime,
                    IsStarting = @IsStarting,
                    IsActive = @IsActive
                WHERE Id = @Id";

            command.Parameters.AddWithValue("@Id", playerPosition.Id);
            command.Parameters.AddWithValue("@CurrentMatchId", playerPosition.CurrentMatchId);
            command.Parameters.AddWithValue("@PlayerId", playerPosition.PlayerId);
            command.Parameters.AddWithValue("@FieldPositionId", playerPosition.FieldPositionId);
            command.Parameters.AddWithValue("@StartTime", playerPosition.StartTime.ToString("yyyy-MM-dd HH:mm:ss"));
            command.Parameters.AddWithValue("@EndTime", playerPosition.EndTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@IsStarting", playerPosition.IsStarting ? 1 : 0);
            command.Parameters.AddWithValue("@IsActive", playerPosition.IsActive ? 1 : 0);

            await command.ExecuteNonQueryAsync();
            return playerPosition;
        }
    }

    public async Task DeletePlayerPositionAsync(int id)
    {
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = "DELETE FROM PlayerPosition WHERE Id = @Id";
            command.Parameters.AddWithValue("@Id", id);
            await command.ExecuteNonQueryAsync();
        }
    }

    private PlayerPosition MapPlayerPosition(SqliteDataReader reader)
    {
        return new PlayerPosition
        {
            Id = reader.GetInt32(0),
            CurrentMatchId = reader.GetInt32(1),
            PlayerId = reader.GetInt32(2),
            FieldPositionId = reader.GetInt32(3),
            StartTime = DateTime.Parse(reader.GetString(4)),
            EndTime = reader.IsDBNull(5) ? null : DateTime.Parse(reader.GetString(5)),
            IsStarting = reader.GetInt32(6) == 1,
            IsActive = reader.GetInt32(7) == 1
        };
    }
} 