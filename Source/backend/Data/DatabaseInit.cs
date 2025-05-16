using System.Data;

namespace CoachBackend.Data
{
    public static class DatabaseInit
    {
        public static void InitializeDatabase(IDbConnection connection)
        {
            // Skapa Positions-tabellen
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Positions (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        Abbreviation TEXT NOT NULL
                    )";
                command.ExecuteNonQuery();
            }

            // Skapa Players-tabellen med foreign key till Positions
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Players (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        PositionId INTEGER NOT NULL,
                        Notes TEXT,
                        FOREIGN KEY (PositionId) REFERENCES Positions(Id)
                    )";
                command.ExecuteNonQuery();
            }

            // Skapa Matches-tabellen
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Matches (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Date TEXT NOT NULL,
                        Opponent TEXT NOT NULL,
                        HomeGame BOOLEAN NOT NULL,
                        Result TEXT,
                        Notes TEXT
                    )";
                command.ExecuteNonQuery();
            }

            // Skapa Gameplay-tabellen med foreign keys till Players och Matches
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Gameplay (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        MatchId INTEGER NOT NULL,
                        PlayerId INTEGER NOT NULL,
                        MinutesPlayed INTEGER,
                        Goals INTEGER DEFAULT 0,
                        Assists INTEGER DEFAULT 0,
                        YellowCards INTEGER DEFAULT 0,
                        RedCards INTEGER DEFAULT 0,
                        Rating REAL,
                        Notes TEXT,
                        FOREIGN KEY (MatchId) REFERENCES Matches(Id) ON DELETE CASCADE,
                        FOREIGN KEY (PlayerId) REFERENCES Players(Id) ON DELETE CASCADE
                    )";
                command.ExecuteNonQuery();
            }

            // Kontrollera om det redan finns positioner
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM Positions";
                var count = Convert.ToInt32(command.ExecuteScalar());

                if (count == 0)
                {
                    // Lägg till grundläggande positioner
                    command.CommandText = @"
                        INSERT INTO Positions (Name, Abbreviation) VALUES 
                        ('Målvakt', 'GK'),
                        ('Försvarare', 'DEF'),
                        ('Mittfältare', 'MID'),
                        ('Anfallare', 'FWD')";
                    command.ExecuteNonQuery();
                }
            }
        }
    }
} 