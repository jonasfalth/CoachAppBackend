using System.Data;
using Microsoft.Data.Sqlite;
using CoachBackend.Models;

namespace CoachBackend.Repositories;

public class PlayerRepository : BaseRepository
{
    public PlayerRepository(IDbConnection connection) : base(connection) { }

    public async Task<List<Player>> GetAllPlayersAsync()
    {
        var players = new List<Player>();
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = @"
                SELECT p.Id, p.Name, p.PositionId, p.Notes, pos.Name as PositionName
                FROM Players p
                JOIN Positions pos ON p.PositionId = pos.Id";
            
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    players.Add(new Player
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        PositionId = reader.GetInt32(2),
                        Notes = reader.IsDBNull(3) ? null : reader.GetString(3)
                    });
                }
            }
        }
        return players;
    }

    public async Task<Player?> GetPlayerByIdAsync(int id)
    {
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = @"
                SELECT p.Id, p.Name, p.PositionId, p.Notes, pos.Name as PositionName
                FROM Players p
                JOIN Positions pos ON p.PositionId = pos.Id
                WHERE p.Id = @Id";
            
            command.Parameters.AddWithValue("@Id", id);

            using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    return new Player
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        PositionId = reader.GetInt32(2),
                        Notes = reader.IsDBNull(3) ? null : reader.GetString(3)
                    };
                }
            }
        }
        return null;
    }

    public async Task<Player> CreatePlayerAsync(Player player)
    {
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = @"
                INSERT INTO Players (Name, PositionId, Notes)
                VALUES (@Name, @PositionId, @Notes);
                SELECT last_insert_rowid();";

            command.Parameters.AddWithValue("@Name", player.Name);
            command.Parameters.AddWithValue("@PositionId", player.PositionId);
            command.Parameters.AddWithValue("@Notes", (object?)player.Notes ?? DBNull.Value);

            player.Id = Convert.ToInt32(await command.ExecuteScalarAsync());
            return player;
        }
    }

    public async Task<Player> UpdatePlayerAsync(Player player)
    {
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = @"
                UPDATE Players
                SET Name = @Name,
                    PositionId = @PositionId,
                    Notes = @Notes
                WHERE Id = @Id";

            command.Parameters.AddWithValue("@Id", player.Id);
            command.Parameters.AddWithValue("@Name", player.Name);
            command.Parameters.AddWithValue("@PositionId", player.PositionId);
            command.Parameters.AddWithValue("@Notes", (object?)player.Notes ?? DBNull.Value);

            await command.ExecuteNonQueryAsync();
            return player;
        }
    }

    public async Task DeletePlayerAsync(int id)
    {
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = "DELETE FROM Players WHERE Id = @Id";
            command.Parameters.AddWithValue("@Id", id);
            await command.ExecuteNonQueryAsync();
        }
    }
} 