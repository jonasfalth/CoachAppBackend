using System.Data;
using Microsoft.Data.Sqlite;
using CoachBackend.Models;

namespace CoachBackend.Repositories;

public class FieldPositionRepository : BaseRepository
{
    public FieldPositionRepository(IDbConnection connection) : base(connection) { }

    public async Task<List<FieldPosition>> GetAllFieldPositionsAsync()
    {
        var fieldPositions = new List<FieldPosition>();
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = @"
                SELECT Id, Name, Abbreviation, Zone, SortOrder, Description
                FROM FieldPositions
                ORDER BY SortOrder, Name";
            
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    fieldPositions.Add(new FieldPosition
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Abbreviation = reader.GetString(2),
                        Zone = reader.GetString(3),
                        SortOrder = reader.GetInt32(4),
                        Description = reader.IsDBNull(5) ? null : reader.GetString(5)
                    });
                }
            }
        }
        return fieldPositions;
    }

    public async Task<List<FieldPosition>> GetFieldPositionsByZoneAsync(string zone)
    {
        var fieldPositions = new List<FieldPosition>();
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = @"
                SELECT Id, Name, Abbreviation, Zone, SortOrder, Description
                FROM FieldPositions
                WHERE Zone = @Zone
                ORDER BY SortOrder, Name";
            
            command.Parameters.AddWithValue("@Zone", zone);

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    fieldPositions.Add(new FieldPosition
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Abbreviation = reader.GetString(2),
                        Zone = reader.GetString(3),
                        SortOrder = reader.GetInt32(4),
                        Description = reader.IsDBNull(5) ? null : reader.GetString(5)
                    });
                }
            }
        }
        return fieldPositions;
    }

    public async Task<FieldPosition?> GetFieldPositionByIdAsync(int id)
    {
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = @"
                SELECT Id, Name, Abbreviation, Zone, SortOrder, Description
                FROM FieldPositions
                WHERE Id = @Id";
            
            command.Parameters.AddWithValue("@Id", id);

            using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    return new FieldPosition
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Abbreviation = reader.GetString(2),
                        Zone = reader.GetString(3),
                        SortOrder = reader.GetInt32(4),
                        Description = reader.IsDBNull(5) ? null : reader.GetString(5)
                    };
                }
            }
        }
        return null;
    }
} 