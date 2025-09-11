using System.Data;
using Microsoft.Data.Sqlite;
using CoachBackend.Models;

namespace CoachBackend.Repositories;

public class MatchEventRepository : BaseRepository
{
    public MatchEventRepository(IDbConnection connection) : base(connection) { }

    public async Task<List<MatchEvent>> GetAllMatchEventsAsync()
    {
        var matchEvents = new List<MatchEvent>();
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = @"
                SELECT Id, CurrentMatchId, EventType, PlayerId, PlayerOutId, PlayerInId, FieldPositionId,
                       MatchMinute, MatchSecond, Notes, EventTime
                FROM MatchEvent
                ORDER BY MatchMinute, MatchSecond, EventTime";
            
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    matchEvents.Add(MapMatchEvent(reader));
                }
            }
        }
        return matchEvents;
    }

    public async Task<List<MatchEvent>> GetMatchEventsByCurrentMatchIdAsync(int currentMatchId)
    {
        var matchEvents = new List<MatchEvent>();
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = @"
                SELECT Id, CurrentMatchId, EventType, PlayerId, PlayerOutId, PlayerInId, FieldPositionId,
                       MatchMinute, MatchSecond, Notes, EventTime
                FROM MatchEvent
                WHERE CurrentMatchId = @CurrentMatchId
                ORDER BY MatchMinute, MatchSecond, EventTime";
            
            command.Parameters.AddWithValue("@CurrentMatchId", currentMatchId);

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    matchEvents.Add(MapMatchEvent(reader));
                }
            }
        }
        return matchEvents;
    }

    public async Task<MatchEvent?> GetMatchEventByIdAsync(int id)
    {
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = @"
                SELECT Id, CurrentMatchId, EventType, PlayerId, PlayerOutId, PlayerInId, FieldPositionId,
                       MatchMinute, MatchSecond, Notes, EventTime
                FROM MatchEvent
                WHERE Id = @Id";
            
            command.Parameters.AddWithValue("@Id", id);

            using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    return MapMatchEvent(reader);
                }
            }
        }
        return null;
    }

    public async Task<MatchEvent> CreateMatchEventAsync(MatchEvent matchEvent)
    {
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = @"
                INSERT INTO MatchEvent (CurrentMatchId, EventType, PlayerId, PlayerOutId, PlayerInId, 
                                      FieldPositionId, MatchMinute, MatchSecond, Notes, EventTime)
                VALUES (@CurrentMatchId, @EventType, @PlayerId, @PlayerOutId, @PlayerInId, 
                        @FieldPositionId, @MatchMinute, @MatchSecond, @Notes, @EventTime);
                SELECT last_insert_rowid();";

            command.Parameters.AddWithValue("@CurrentMatchId", matchEvent.CurrentMatchId);
            command.Parameters.AddWithValue("@EventType", matchEvent.EventType);
            command.Parameters.AddWithValue("@PlayerId", (object?)matchEvent.PlayerId ?? DBNull.Value);
            command.Parameters.AddWithValue("@PlayerOutId", (object?)matchEvent.PlayerOutId ?? DBNull.Value);
            command.Parameters.AddWithValue("@PlayerInId", (object?)matchEvent.PlayerInId ?? DBNull.Value);
            command.Parameters.AddWithValue("@FieldPositionId", (object?)matchEvent.FieldPositionId ?? DBNull.Value);
            command.Parameters.AddWithValue("@MatchMinute", matchEvent.MatchMinute);
            command.Parameters.AddWithValue("@MatchSecond", matchEvent.MatchSecond);
            command.Parameters.AddWithValue("@Notes", (object?)matchEvent.Notes ?? DBNull.Value);
            command.Parameters.AddWithValue("@EventTime", matchEvent.EventTime.ToString("yyyy-MM-dd HH:mm:ss"));

            matchEvent.Id = Convert.ToInt32(await command.ExecuteScalarAsync());
            return matchEvent;
        }
    }

    public async Task<MatchEvent> UpdateMatchEventAsync(MatchEvent matchEvent)
    {
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = @"
                UPDATE MatchEvent
                SET CurrentMatchId = @CurrentMatchId,
                    EventType = @EventType,
                    PlayerId = @PlayerId,
                    PlayerOutId = @PlayerOutId,
                    PlayerInId = @PlayerInId,
                    FieldPositionId = @FieldPositionId,
                    MatchMinute = @MatchMinute,
                    MatchSecond = @MatchSecond,
                    Notes = @Notes,
                    EventTime = @EventTime
                WHERE Id = @Id";

            command.Parameters.AddWithValue("@Id", matchEvent.Id);
            command.Parameters.AddWithValue("@CurrentMatchId", matchEvent.CurrentMatchId);
            command.Parameters.AddWithValue("@EventType", matchEvent.EventType);
            command.Parameters.AddWithValue("@PlayerId", (object?)matchEvent.PlayerId ?? DBNull.Value);
            command.Parameters.AddWithValue("@PlayerOutId", (object?)matchEvent.PlayerOutId ?? DBNull.Value);
            command.Parameters.AddWithValue("@PlayerInId", (object?)matchEvent.PlayerInId ?? DBNull.Value);
            command.Parameters.AddWithValue("@FieldPositionId", (object?)matchEvent.FieldPositionId ?? DBNull.Value);
            command.Parameters.AddWithValue("@MatchMinute", matchEvent.MatchMinute);
            command.Parameters.AddWithValue("@MatchSecond", matchEvent.MatchSecond);
            command.Parameters.AddWithValue("@Notes", (object?)matchEvent.Notes ?? DBNull.Value);
            command.Parameters.AddWithValue("@EventTime", matchEvent.EventTime.ToString("yyyy-MM-dd HH:mm:ss"));

            await command.ExecuteNonQueryAsync();
            return matchEvent;
        }
    }

    public async Task DeleteMatchEventAsync(int id)
    {
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = "DELETE FROM MatchEvent WHERE Id = @Id";
            command.Parameters.AddWithValue("@Id", id);
            await command.ExecuteNonQueryAsync();
        }
    }

    private MatchEvent MapMatchEvent(SqliteDataReader reader)
    {
        return new MatchEvent
        {
            Id = reader.GetInt32(0),
            CurrentMatchId = reader.GetInt32(1),
            EventType = reader.GetString(2),
            PlayerId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
            PlayerOutId = reader.IsDBNull(4) ? null : reader.GetInt32(4),
            PlayerInId = reader.IsDBNull(5) ? null : reader.GetInt32(5),
            FieldPositionId = reader.IsDBNull(6) ? null : reader.GetInt32(6),
            MatchMinute = reader.GetInt32(7),
            MatchSecond = reader.GetInt32(8),
            Notes = reader.IsDBNull(9) ? null : reader.GetString(9),
            EventTime = DateTime.Parse(reader.GetString(10))
        };
    }
} 