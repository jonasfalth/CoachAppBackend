using System.Data;
using Microsoft.Data.Sqlite;

namespace CoachBackend.Repositories;

public abstract class BaseRepository : IDisposable
{
    protected readonly IDbConnection _connection;
    private bool _disposed;

    protected BaseRepository(IDbConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        Console.WriteLine($"Skapar repository med anslutning i tillstånd: {_connection.State}");
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Vi stänger inte anslutningen här eftersom den delas mellan flera requests
                Console.WriteLine("Repository disposad, men behåller anslutningen öppen.");
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~BaseRepository()
    {
        Dispose(false);
    }

    protected SqliteParameter CreateParameter(string name, object value)
    {
        return new SqliteParameter(name, value);
    }

    protected void EnsureConnectionOpen()
    {
        if (_connection.State != ConnectionState.Open)
        {
            Console.WriteLine("Återöppnar databasanslutning...");
            _connection.Open();
            Console.WriteLine("Databasanslutning återöppnad.");
        }
    }
} 