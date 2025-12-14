#pragma warning disable CC0001 // Use var - nint requires explicit type

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace AdaskoTheBeAsT.ValueSql.Runtime;

/// <summary>
/// High-performance data reader utilities optimized for .NET 10.
/// Inspired by Ben Adams' Kestrel optimizations.
/// </summary>
public static class ValueSqlReader
{
    /// <summary>
    /// Executes a query and reads all rows into a list.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public static async ValueTask<List<T>> QueryAsync<T, TMapper>(
        SqlConnection connection,
        string sql,
        TMapper mapper,
        ValueSqlOptions? options = null,
        CancellationToken cancellationToken = default)
        where TMapper : struct, IValueSqlMapperFast<T>
    {
        options ??= ValueSqlOptions.Default;

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        }

        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        await using var reader = await command.ExecuteReaderAsync(options.ToCommandBehavior(), cancellationToken).ConfigureAwait(false);

        var estimatedCount = options.EstimatedRowCount > 0 ? options.EstimatedRowCount : 1000;
        var list = new List<T>(estimatedCount);

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            list.Add(mapper.MapFast(reader));
        }

        return list;
    }

    /// <summary>
    /// Executes a query and returns the first row or default.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public static async ValueTask<T?> QueryFirstOrDefaultAsync<T, TMapper>(
        SqlConnection connection,
        string sql,
        TMapper mapper,
        CancellationToken cancellationToken = default)
        where T : class
        where TMapper : struct, IValueSqlMapperFast<T>
    {
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        }

        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        // Optimized for single row - no SequentialAccess (overhead for small rows)
        await using var reader = await command.ExecuteReaderAsync(
            CommandBehavior.SingleResult | CommandBehavior.SingleRow,
            cancellationToken).ConfigureAwait(false);

        if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return mapper.MapFast(reader);
        }

        return null;
    }

    /// <summary>
    /// Synchronous query execution - fastest for single rows.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public static T? QueryFirstOrDefaultSync<T, TMapper>(
        SqlConnection connection,
        string sql,
        TMapper mapper)
        where T : class
        where TMapper : struct, IValueSqlMapperFast<T>
    {
        if (connection.State != ConnectionState.Open)
        {
            connection.Open();
        }

        using var command = connection.CreateCommand();
        command.CommandText = sql;

        // Sync path - no async state machine overhead
        using var reader = command.ExecuteReader(CommandBehavior.SingleResult | CommandBehavior.SingleRow);

        if (reader.Read())
        {
            return mapper.MapFast(reader);
        }

        return null;
    }

    /// <summary>
    /// Reads all rows into a list with pre-allocated capacity.
    /// Optimized for small to medium result sets.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public static async ValueTask<List<T>> ReadAllAsync<T, TMapper>(
        SqlDataReader reader,
        TMapper mapper,
        int estimatedCount = 1000,
        CancellationToken cancellationToken = default)
        where TMapper : struct, IValueSqlMapperFast<T>
    {
        // For small counts, use direct approach without extra allocations
        if (estimatedCount <= 32)
        {
            var list = new List<T>(estimatedCount);
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                list.Add(mapper.MapFast(reader));
            }

            return list;
        }

        // For larger counts, use CollectionsMarshal optimization
        var buffer = ArrayPool<T>.Shared.Rent(estimatedCount);
        var count = 0;

        try
        {
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                if (count >= buffer.Length)
                {
                    var newBuffer = ArrayPool<T>.Shared.Rent(buffer.Length * 2);
                    Array.Copy(buffer, newBuffer, count);
                    ArrayPool<T>.Shared.Return(buffer, clearArray: false);
                    buffer = newBuffer;
                }

                buffer[count++] = mapper.MapFast(reader);
            }

            var result = new List<T>(count);
            if (count > 0)
            {
                CollectionsMarshal.SetCount(result, count);
                buffer.AsSpan(0, count).CopyTo(CollectionsMarshal.AsSpan(result));
            }

            return result;
        }
        finally
        {
            ArrayPool<T>.Shared.Return(buffer, clearArray: false);
        }
    }

    /// <summary>
    /// Reads all rows into a rented array for zero-allocation scenarios.
    /// Caller must return the array to ArrayPool.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public static async ValueTask<(T[] Array, int Count)> ReadAllToArrayAsync<T, TMapper>(
        SqlDataReader reader,
        TMapper mapper,
        int estimatedCount = 1000,
        CancellationToken cancellationToken = default)
        where TMapper : struct, IValueSqlMapperFast<T>
    {
        var array = ArrayPool<T>.Shared.Rent(estimatedCount);
        var count = 0;

        try
        {
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                if (count >= array.Length)
                {
                    var newArray = ArrayPool<T>.Shared.Rent(array.Length * 2);
                    Array.Copy(array, newArray, count);
                    ArrayPool<T>.Shared.Return(array);
                    array = newArray;
                }

                array[count++] = mapper.MapFast(reader);
            }

            return (array, count);
        }
        catch
        {
            ArrayPool<T>.Shared.Return(array);
            throw;
        }
    }

    /// <summary>
    /// Synchronous high-performance bulk read with CollectionsMarshal optimization.
    /// Often faster than async for small to medium result sets.
    /// Uses nint and Unsafe.Add for maximum performance.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public static List<T> ReadAllSync<T, TMapper>(
        SqlDataReader reader,
        TMapper mapper,
        int estimatedCount = 1000)
        where TMapper : struct, IValueSqlMapperFast<T>
    {
        // For small counts, skip buffering overhead
        if (estimatedCount <= 32)
        {
            var list = new List<T>(estimatedCount);
            while (reader.Read())
            {
                list.Add(mapper.MapFast(reader));
            }

            return list;
        }

        // For larger counts, use nint + Unsafe.Add for direct buffer access
        var buffer = ArrayPool<T>.Shared.Rent(estimatedCount);
        nint count = 0;
        nint bufferLen = buffer.Length;

        try
        {
            ref T bufferRef = ref MemoryMarshal.GetArrayDataReference(buffer);

            while (reader.Read())
            {
                if (count >= bufferLen)
                {
                    var newBuffer = ArrayPool<T>.Shared.Rent((int)bufferLen * 2);
                    Array.Copy(buffer, newBuffer, (int)count);
                    ArrayPool<T>.Shared.Return(buffer, clearArray: false);
                    buffer = newBuffer;
                    bufferRef = ref MemoryMarshal.GetArrayDataReference(buffer);
                    bufferLen = buffer.Length;
                }

                Unsafe.Add(ref bufferRef, count++) = mapper.MapFast(reader);
            }

            var result = new List<T>((int)count);
            if (count > 0)
            {
                CollectionsMarshal.SetCount(result, (int)count);
                buffer.AsSpan(0, (int)count).CopyTo(CollectionsMarshal.AsSpan(result));
            }

            return result;
        }
        finally
        {
            ArrayPool<T>.Shared.Return(buffer, clearArray: false);
        }
    }

    /// <summary>
    /// Returns a rented array to the pool.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ReturnArray<T>(T[] array)
    {
        ArrayPool<T>.Shared.Return(array);
    }

    /// <summary>
    /// Executes a paged query with total count using NextResult pattern.
    /// Single round-trip, 20-40% faster than parallel queries.
    /// SQL should contain two result sets: data query first, then COUNT(*).
    /// </summary>
    /// <example>
    /// <code>
    /// var sql = @"
    ///     SELECT * FROM Products WHERE CategoryId = @catId
    ///     ORDER BY Id OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY;
    ///     SELECT COUNT(*) FROM Products WHERE CategoryId = @catId;";
    /// var result = await ValueSqlReader.QueryPagedAsync&lt;Product, ProductMapper&gt;(
    ///     connection, sql, mapper, cmd => {
    ///         cmd.Parameters.AddWithValue("@catId", categoryId);
    ///         cmd.Parameters.AddWithValue("@skip", skip);
    ///         cmd.Parameters.AddWithValue("@take", take);
    ///     }, take);
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public static async ValueTask<PagedResult<T>> QueryPagedAsync<T, TMapper>(
        SqlConnection connection,
        string sql,
        TMapper mapper,
        Action<SqlCommand>? configureCommand = null,
        int estimatedPageSize = 50,
        CancellationToken cancellationToken = default)
        where TMapper : struct, IValueSqlMapperFast<T>
    {
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        }

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        configureCommand?.Invoke(command);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        // Read data (first result set)
        var items = await ReadAllAsync<T, TMapper>(reader, mapper, estimatedPageSize, cancellationToken)
            .ConfigureAwait(false);

        // Move to count result set
        var totalCount = 0;
        if (await reader.NextResultAsync(cancellationToken).ConfigureAwait(false) &&
            await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            totalCount = reader.GetInt32(0);
        }

        return new PagedResult<T>(items, totalCount);
    }

    /// <summary>
    /// Executes a paged query with parameters using NextResult pattern.
    /// Convenience overload with typed parameters.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public static ValueTask<PagedResult<T>> QueryPagedAsync<T, TMapper>(
        SqlConnection connection,
        string sql,
        TMapper mapper,
        int skip,
        int take,
        Action<SqlCommand>? configureCommand = null,
        CancellationToken cancellationToken = default)
        where TMapper : struct, IValueSqlMapperFast<T>
    {
        return QueryPagedAsync<T, TMapper>(
            connection,
            sql,
            mapper,
            cmd =>
            {
                cmd.Parameters.AddWithValue("@skip", skip);
                cmd.Parameters.AddWithValue("@take", take);
                configureCommand?.Invoke(cmd);
            },
            take,
            cancellationToken);
    }

    /// <summary>
    /// Synchronous paged query with total count using NextResult pattern.
    /// Often faster than async for small to medium page sizes.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public static PagedResult<T> QueryPagedSync<T, TMapper>(
        SqlConnection connection,
        string sql,
        TMapper mapper,
        Action<SqlCommand>? configureCommand = null,
        int estimatedPageSize = 50)
        where TMapper : struct, IValueSqlMapperFast<T>
    {
        if (connection.State != ConnectionState.Open)
        {
            connection.Open();
        }

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        configureCommand?.Invoke(command);

        using var reader = command.ExecuteReader();

        // Read data (first result set)
        var items = ReadAllSync<T, TMapper>(reader, mapper, estimatedPageSize);

        // Move to count result set
        var totalCount = 0;
        if (reader.NextResult() && reader.Read())
        {
            totalCount = reader.GetInt32(0);
        }

        return new PagedResult<T>(items, totalCount);
    }

    /// <summary>
    /// Synchronous paged query with typed skip/take parameters.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public static PagedResult<T> QueryPagedSync<T, TMapper>(
        SqlConnection connection,
        string sql,
        TMapper mapper,
        int skip,
        int take,
        Action<SqlCommand>? configureCommand = null)
        where TMapper : struct, IValueSqlMapperFast<T>
    {
        return QueryPagedSync<T, TMapper>(
            connection,
            sql,
            mapper,
            cmd =>
            {
                cmd.Parameters.AddWithValue("@skip", skip);
                cmd.Parameters.AddWithValue("@take", take);
                configureCommand?.Invoke(cmd);
            },
            take);
    }
}
