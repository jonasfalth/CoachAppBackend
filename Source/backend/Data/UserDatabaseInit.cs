using System.Data;
using Microsoft.Data.Sqlite;
using BCrypt.Net;

namespace CoachBackend.Data;

public static class UserDatabaseInit
{
    public static void InitializeUserDatabase(IDbConnection connection)
    {
        Console.WriteLine("Startar initialisering av användardatabasen...");
        
        try
        {
            // Skapa Users-tabellen
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Users (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Username TEXT NOT NULL UNIQUE,
                        Email TEXT NOT NULL UNIQUE,
                        PasswordHash TEXT NOT NULL,
                        LastLogin TEXT NOT NULL,
                        LastUpdated TEXT NOT NULL,
                        CreatedAt TEXT NOT NULL
                    )";
                command.ExecuteNonQuery();
                Console.WriteLine("Users-tabellen kontrollerad/skapat.");
            }

            // Skapa Teams-tabellen
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Teams (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL UNIQUE,
                        DatabaseName TEXT NOT NULL UNIQUE,
                        CreatedAt TEXT NOT NULL,
                        LastUpdated TEXT NOT NULL
                    )";
                command.ExecuteNonQuery();
                Console.WriteLine("Teams-tabellen kontrollerad/skapat.");
            }

            // Skapa UserTeams-tabellen (många-till-många)
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS UserTeams (
                        UserId INTEGER NOT NULL,
                        TeamId INTEGER NOT NULL,
                        Role TEXT NOT NULL DEFAULT 'Member',
                        JoinedAt TEXT NOT NULL,
                        CreatedAt TEXT NOT NULL,
                        PRIMARY KEY (UserId, TeamId),
                        FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
                        FOREIGN KEY (TeamId) REFERENCES Teams(Id) ON DELETE CASCADE
                    )";
                command.ExecuteNonQuery();
                Console.WriteLine("UserTeams-tabellen kontrollerad/skapat.");
            }

            // Verifiera att tabellerna finns
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT name FROM sqlite_master WHERE type='table'";
                using (var reader = command.ExecuteReader())
                {
                    Console.WriteLine("Befintliga tabeller i databasen:");
                    while (reader.Read())
                    {
                        Console.WriteLine($"- {reader.GetString(0)}");
                    }
                }
            }

            // Skapa testanvändare om den inte finns
            CreateTestUserIfNotExists((SqliteConnection)connection);

            Console.WriteLine("Databasinitialisering slutförd.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fel vid databasinitialisering: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    private static void CreateTestUserIfNotExists(SqliteConnection connection)
    {
        Console.WriteLine("Kontrollerar om testanvändaren finns...");
        
        using (var command = connection.CreateCommand())
        {
            command.CommandText = "SELECT COUNT(*) FROM Users WHERE Email = @Email";
            command.Parameters.AddWithValue("@Email", "jonas.falth@gmail.com");
            
            var count = Convert.ToInt32(command.ExecuteScalar());
            
            if (count == 0)
            {
                Console.WriteLine("Skapar testanvändare...");
                var now = DateTime.UtcNow.ToString("o");
                var passwordHash = BCrypt.Net.BCrypt.HashPassword("test22");
                
                command.CommandText = @"
                    INSERT INTO Users (Username, Email, PasswordHash, LastLogin, LastUpdated, CreatedAt)
                    VALUES (@Username, @Email, @PasswordHash, @LastLogin, @LastUpdated, @CreatedAt)";
                
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@Username", "jonas.falth@gmail.com");
                command.Parameters.AddWithValue("@Email", "jonas.falth@gmail.com");
                command.Parameters.AddWithValue("@PasswordHash", passwordHash);
                command.Parameters.AddWithValue("@LastLogin", now);
                command.Parameters.AddWithValue("@LastUpdated", now);
                command.Parameters.AddWithValue("@CreatedAt", now);
                
                command.ExecuteNonQuery();
                Console.WriteLine("Testanvändare skapad.");
            }
            else
            {
                Console.WriteLine("Testanvändaren finns redan.");
            }
        }
    }
} 