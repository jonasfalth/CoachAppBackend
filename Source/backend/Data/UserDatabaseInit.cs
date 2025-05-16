using System.Data;

namespace CoachBackend.Data;

public static class UserDatabaseInit
{
    public static void InitializeUserDatabase(IDbConnection connection)
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
        }

        // Skapa UserTeams-tabellen (många-till-många)
        using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS UserTeams (
                    UserId INTEGER NOT NULL,
                    TeamId INTEGER NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    PRIMARY KEY (UserId, TeamId),
                    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
                    FOREIGN KEY (TeamId) REFERENCES Teams(Id) ON DELETE CASCADE
                )";
            command.ExecuteNonQuery();
        }
    }
} 