using System.Data;
using Microsoft.Data.Sqlite;

namespace CoachBackend;

public static class UserDatabaseInit
{
    public static void Initialize(IDbConnection connection)
    {
        using (var command = (SqliteCommand)connection.CreateCommand())
        {
            // Skapa Users-tabellen
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT NOT NULL UNIQUE,
                    PasswordHash TEXT NOT NULL,
                    Email TEXT NOT NULL UNIQUE,
                    FirstName TEXT,
                    LastName TEXT,
                    CreatedAt DATETIME NOT NULL,
                    LastUpdated DATETIME NOT NULL
                )";
            command.ExecuteNonQuery();

            // Skapa Teams-tabellen
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Teams (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    DatabaseName TEXT NOT NULL UNIQUE,
                    CreatedAt DATETIME NOT NULL,
                    LastUpdated DATETIME NOT NULL
                )";
            command.ExecuteNonQuery();

            // Skapa UserTeams-tabellen (många-till-många relation)
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS UserTeams (
                    UserId INTEGER NOT NULL,
                    TeamId INTEGER NOT NULL,
                    CreatedAt DATETIME NOT NULL,
                    PRIMARY KEY (UserId, TeamId),
                    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
                    FOREIGN KEY (TeamId) REFERENCES Teams(Id) ON DELETE CASCADE
                )";
            command.ExecuteNonQuery();
        }
    }
} 