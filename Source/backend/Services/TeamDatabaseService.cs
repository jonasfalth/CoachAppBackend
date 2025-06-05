using System.Data;
using Microsoft.Data.Sqlite;
using CoachBackend.Repositories;
using CoachBackend.Data;
using System.Collections.Concurrent;

namespace CoachBackend.Services;

public class TeamDatabaseService
{
    private readonly IDbConnection _userConnection;
    private readonly TeamRepository _teamRepository;
    private readonly ILogger<TeamDatabaseService> _logger;
    private readonly string _databaseDirectory;
    private readonly TeamDatabaseConnectionManager _connectionManager;

    public TeamDatabaseService(
        IDbConnection userConnection,
        TeamRepository teamRepository,
        ILogger<TeamDatabaseService> logger,
        IConfiguration configuration,
        TeamDatabaseConnectionManager connectionManager)
    {
        _userConnection = userConnection;
        _teamRepository = teamRepository;
        _logger = logger;
        _connectionManager = connectionManager;
        
        // Använd samma katalog som coach-app databasen för team-databaser
        var coachAppDbPath = configuration.GetValue<string>("Database:CoachAppDbPath") ?? "CoachAppDb.db";
        _databaseDirectory = Path.GetDirectoryName(Path.GetFullPath(coachAppDbPath)) ?? Directory.GetCurrentDirectory();
    }

    public async Task<IDbConnection> GetTeamDatabaseConnectionAsync(string teamDatabaseName)
    {
        var team = await _teamRepository.GetTeamByDatabaseNameAsync(teamDatabaseName);
        if (team == null)
        {
            throw new InvalidOperationException($"Team with database name {teamDatabaseName} not found");
        }

        return await _connectionManager.GetConnectionAsync(team.DatabaseName, _databaseDirectory, _logger);
    }
}

/// <summary>
/// Singleton service som hanterar alla team-databasanslutningar
/// </summary>
public class TeamDatabaseConnectionManager : IDisposable
{
    // Cache för att undvika att skapa flera anslutningar till samma databas
    private readonly ConcurrentDictionary<string, SqliteConnection> _teamConnections = new();
    private bool _disposed = false;
    private readonly object _lock = new object();

    public async Task<IDbConnection> GetConnectionAsync(string databaseName, string databaseDirectory, ILogger logger)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(TeamDatabaseConnectionManager));
        }

        // Kontrollera om vi redan har en anslutning för denna databas
        if (_teamConnections.TryGetValue(databaseName, out var existingConnection))
        {
            if (existingConnection.State == ConnectionState.Open)
            {
                logger.LogDebug("Återanvänder befintlig anslutning för databas: {DatabaseName}", databaseName);
                return existingConnection;
            }
            else
            {
                // Anslutningen är stängd, ta bort den från cache
                lock (_lock)
                {
                    if (_teamConnections.TryRemove(databaseName, out var oldConnection))
                    {
                        oldConnection?.Dispose();
                    }
                }
            }
        }

        // Skapa sökväg för team-databasen
        var teamDbPath = Path.Combine(databaseDirectory, $"{databaseName}.db");
        
        logger.LogInformation("Skapar/ansluter till team-databas: {DatabasePath}", teamDbPath);

        // Skapa ny anslutning
        var connection = new SqliteConnection($"Data Source={teamDbPath}");
        connection.Open();

        // Initialisera databasen om den inte redan har rätt struktur
        await InitializeTeamDatabaseAsync(connection, databaseName, logger);

        // Lägg till i cache (thread-safe)
        lock (_lock)
        {
            if (!_disposed)
            {
                _teamConnections.TryAdd(databaseName, connection);
            }
            else
            {
                // Service är redan disposed, stäng anslutningen
                connection.Close();
                connection.Dispose();
                throw new ObjectDisposedException(nameof(TeamDatabaseConnectionManager));
            }
        }

        logger.LogInformation("Team-databasanslutning skapad och initialiserad för: {DatabaseName}", databaseName);
        
        return connection;
    }

    private Task InitializeTeamDatabaseAsync(SqliteConnection connection, string databaseName, ILogger logger)
    {
        logger.LogInformation("Initialiserar team-databas: {DatabaseName}", databaseName);
        
        try
        {
            // Använd befintlig DatabaseInit för att skapa tabellstrukturen
            DatabaseInit.InitializeDatabase(connection);
            
            logger.LogInformation("Team-databas initialiserad framgångsrikt: {DatabaseName}", databaseName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fel vid initialisering av team-databas: {DatabaseName}", databaseName);
            throw;
        }
        
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            lock (_lock)
            {
                if (!_disposed)
                {
                    foreach (var kvp in _teamConnections)
                    {
                        try
                        {
                            if (kvp.Value.State != ConnectionState.Closed)
                            {
                                kvp.Value.Close();
                            }
                            kvp.Value.Dispose();
                        }
                        catch (Exception ex)
                        {
                            // Log warning but continue cleanup
                            Console.WriteLine($"Fel vid stängning av anslutning för databas {kvp.Key}: {ex.Message}");
                        }
                    }
                    
                    _teamConnections.Clear();
                    _disposed = true;
                }
            }
        }
    }
} 