using System.Data;
using Microsoft.Data.Sqlite;
using CoachBackend.Models;

namespace CoachBackend.Repositories;

public class PositionRepository : BaseRepository
{
    public PositionRepository(IDbConnection connection) : base(connection) { }

    public async Task<List<Position>> GetAllPositionsAsync()
    {
        var positions = new List<Position>();
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = "SELECT Id, Name, Abbreviation FROM Positions";
            
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    positions.Add(new Position
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Abbreviation = reader.GetString(2)
                    });
                }
            }
        }
        return positions;
    }

    public async Task<Position?> GetPositionByIdAsync(int id)
    {
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = "SELECT Id, Name, Abbreviation FROM Positions WHERE Id = @Id";
            command.Parameters.AddWithValue("@Id", id);

            using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    return new Position
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Abbreviation = reader.GetString(2)
                    };
                }
            }
        }
        return null;
    }

    public async Task<Position> CreatePositionAsync(Position position)
    {
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = @"
                INSERT INTO Positions (Name, Abbreviation)
                VALUES (@Name, @Abbreviation);
                SELECT last_insert_rowid();";

            command.Parameters.AddWithValue("@Name", position.Name);
            command.Parameters.AddWithValue("@Abbreviation", position.Abbreviation);

            position.Id = Convert.ToInt32(await command.ExecuteScalarAsync());
            return position;
        }
    }

    public async Task<Position> UpdatePositionAsync(Position position)
    {
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = @"
                UPDATE Positions
                SET Name = @Name,
                    Abbreviation = @Abbreviation
                WHERE Id = @Id";

            command.Parameters.AddWithValue("@Id", position.Id);
            command.Parameters.AddWithValue("@Name", position.Name);
            command.Parameters.AddWithValue("@Abbreviation", position.Abbreviation);

            await command.ExecuteNonQueryAsync();
            return position;
        }
    }

    public async Task DeletePositionAsync(int id)
    {
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = "DELETE FROM Positions WHERE Id = @Id";
            command.Parameters.AddWithValue("@Id", id);
            await command.ExecuteNonQueryAsync();
        }
    }
} 