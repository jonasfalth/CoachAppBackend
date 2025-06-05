using System.Data;
using Microsoft.Data.Sqlite;
using CoachBackend.Models;

namespace CoachBackend.Repositories;

public class UserRepository : BaseRepository
{
    public UserRepository(IDbConnection connection) : base(connection) { }

    public async Task<List<User>> GetAllUsersAsync()
    {
        EnsureConnectionOpen();
        var users = new List<User>();
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = "SELECT Id, Username, Email, LastLogin, LastUpdated, CreatedAt FROM Users";
            
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

    public async Task<User?> GetUserByIdAsync(int id)
    {
        EnsureConnectionOpen();
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = "SELECT Id, Username, Email, LastLogin, LastUpdated, CreatedAt FROM Users WHERE Id = @Id";
            command.Parameters.AddWithValue("@Id", id);

            using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    return new User
                    {
                        Id = reader.GetInt32(0),
                        Username = reader.GetString(1),
                        Email = reader.GetString(2),
                        LastLogin = DateTime.Parse(reader.GetString(3)),
                        LastUpdated = DateTime.Parse(reader.GetString(4)),
                        CreatedAt = DateTime.Parse(reader.GetString(5))
                    };
                }
            }
        }
        return null;
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        EnsureConnectionOpen();
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = "SELECT Id, Username, Email, PasswordHash, LastLogin, LastUpdated, CreatedAt FROM Users WHERE Username = @Username";
            command.Parameters.AddWithValue("@Username", username);

            using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    return new User
                    {
                        Id = reader.GetInt32(0),
                        Username = reader.GetString(1),
                        Email = reader.GetString(2),
                        PasswordHash = reader.GetString(3),
                        LastLogin = DateTime.Parse(reader.GetString(4)),
                        LastUpdated = DateTime.Parse(reader.GetString(5)),
                        CreatedAt = DateTime.Parse(reader.GetString(6))
                    };
                }
            }
        }
        return null;
    }

    public async Task<User> CreateUserAsync(User user)
    {
        EnsureConnectionOpen();
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = @"
                INSERT INTO Users (Username, Email, PasswordHash, LastLogin, LastUpdated, CreatedAt)
                VALUES (@Username, @Email, @PasswordHash, @LastLogin, @LastUpdated, @CreatedAt);
                SELECT last_insert_rowid();";

            command.Parameters.AddWithValue("@Username", user.Username);
            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            command.Parameters.AddWithValue("@LastLogin", user.LastLogin.ToString("o"));
            command.Parameters.AddWithValue("@LastUpdated", user.LastUpdated.ToString("o"));
            command.Parameters.AddWithValue("@CreatedAt", user.CreatedAt.ToString("o"));

            user.Id = Convert.ToInt32(await command.ExecuteScalarAsync());
            return user;
        }
    }

    public async Task<User> UpdateUserAsync(User user)
    {
        EnsureConnectionOpen();
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = @"
                UPDATE Users
                SET Username = @Username,
                    Email = @Email,
                    LastUpdated = @LastUpdated
                WHERE Id = @Id";

            command.Parameters.AddWithValue("@Id", user.Id);
            command.Parameters.AddWithValue("@Username", user.Username);
            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@LastUpdated", user.LastUpdated.ToString("o"));

            await command.ExecuteNonQueryAsync();
            return user;
        }
    }

    public async Task UpdateUserPasswordAsync(int userId, string passwordHash)
    {
        EnsureConnectionOpen();
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = @"
                UPDATE Users
                SET PasswordHash = @PasswordHash,
                    LastUpdated = @LastUpdated
                WHERE Id = @Id";

            command.Parameters.AddWithValue("@Id", userId);
            command.Parameters.AddWithValue("@PasswordHash", passwordHash);
            command.Parameters.AddWithValue("@LastUpdated", DateTime.UtcNow.ToString("o"));

            await command.ExecuteNonQueryAsync();
        }
    }

    public async Task UpdateLastLoginAsync(int userId)
    {
        EnsureConnectionOpen();
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = @"
                UPDATE Users
                SET LastLogin = @LastLogin
                WHERE Id = @Id";

            command.Parameters.AddWithValue("@Id", userId);
            command.Parameters.AddWithValue("@LastLogin", DateTime.UtcNow.ToString("o"));

            await command.ExecuteNonQueryAsync();
        }
    }

    public async Task DeleteUserAsync(int id)
    {
        EnsureConnectionOpen();
        using (var command = (SqliteCommand)_connection.CreateCommand())
        {
            command.CommandText = "DELETE FROM Users WHERE Id = @Id";
            command.Parameters.AddWithValue("@Id", id);
            await command.ExecuteNonQueryAsync();
        }
    }
} 