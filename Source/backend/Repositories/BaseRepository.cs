using System.Data;
using Microsoft.Data.Sqlite;

namespace CoachBackend.Repositories;

public abstract class BaseRepository
{
    protected readonly IDbConnection _connection;

    protected BaseRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    protected SqliteParameter CreateParameter(string name, object value)
    {
        return new SqliteParameter(name, value);
    }
} 