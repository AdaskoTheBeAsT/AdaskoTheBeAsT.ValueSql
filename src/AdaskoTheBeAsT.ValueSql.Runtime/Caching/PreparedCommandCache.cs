using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Data.SqlClient;

namespace AdaskoTheBeAsT.ValueSql.Runtime.Caching;

/// <summary>
/// High-performance cache for prepared SQL commands.
/// Reuses execution plans for repeated queries.
/// </summary>
public sealed class PreparedCommandCache : IDisposable
{
    private static readonly PreparedCommandCache SharedInstance = new(maxSize: 100);

    private readonly ConcurrentDictionary<int, PreparedCommandEntry> _cache;
    private readonly int _maxSize;
    private int _count;
    private bool _disposed;

    /// <summary>
    /// Creates a new prepared command cache.
    /// </summary>
    public PreparedCommandCache(int maxSize = 100)
    {
        _maxSize = maxSize;
        _cache = new ConcurrentDictionary<int, PreparedCommandEntry>();
    }

    /// <summary>
    /// Gets the shared instance of the prepared command cache.
    /// </summary>
    public static PreparedCommandCache Shared => SharedInstance;

    /// <summary>
    /// Creates a command without caching (for one-off queries).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SqlCommand CreateCommand(SqlConnection connection, string sql)
    {
        var command = connection.CreateCommand();
        command.CommandText = sql;
        return command;
    }

    /// <summary>
    /// Gets or creates a prepared command for the given SQL.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SqlCommand GetOrCreateCommand(SqlConnection connection, string sql)
    {
        var key = HashCode.Combine(connection.ConnectionString.GetHashCode(StringComparison.Ordinal), sql.GetHashCode(StringComparison.Ordinal));

        if (_cache.TryGetValue(key, out var entry) && entry.IsValid(connection))
        {
            entry.Command.Connection = connection;
            return entry.Command;
        }

        return CreateAndCacheCommand(connection, sql, key);
    }

    /// <summary>
    /// Returns a command to the cache for reuse.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReturnCommand(SqlCommand command)
    {
        command.Parameters.Clear();
        command.Connection = null;
    }

    /// <summary>
    /// Clears all cached commands.
    /// </summary>
    public void Clear()
    {
        foreach (var entry in _cache.Values)
        {
            entry.Command.Dispose();
        }

        _cache.Clear();
        _count = 0;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposed)
        {
            Clear();
            _disposed = true;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private SqlCommand CreateAndCacheCommand(SqlConnection connection, string sql, int key)
    {
        var command = connection.CreateCommand();
        command.CommandText = sql;

        // Prepare the command to cache the execution plan
        try
        {
            command.Prepare();
        }
        catch (InvalidOperationException ex)
        {
            // Prepare may fail for some queries (e.g., temp tables), that's OK
            System.Diagnostics.Debug.WriteLine($"Command.Prepare() failed: {ex.Message}");
        }

        // Only cache if we have room
        if (Volatile.Read(ref _count) < _maxSize)
        {
            var entry = new PreparedCommandEntry(command, connection.ConnectionString);
            if (_cache.TryAdd(key, entry))
            {
                Interlocked.Increment(ref _count);
            }
        }

        return command;
    }

    private sealed class PreparedCommandEntry
    {
        private readonly string _connectionString;

        public PreparedCommandEntry(SqlCommand command, string connectionString)
        {
            Command = command;
            _connectionString = connectionString;
        }

        public SqlCommand Command { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsValid(SqlConnection connection)
        {
            return string.Equals(_connectionString, connection.ConnectionString, StringComparison.Ordinal);
        }
    }
}
