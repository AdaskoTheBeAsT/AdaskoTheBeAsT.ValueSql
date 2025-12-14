using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.ValueSql.Runtime.Buffering;
using Microsoft.Data.SqlClient;
using Microsoft.IO;

namespace AdaskoTheBeAsT.ValueSql.Runtime;

/// <summary>
/// High-performance bulk reader with internal buffering, pooling, and SIMD optimizations.
/// Uses ArrayBuffer for contiguous memory, RecyclableMemoryStream for BLOBs.
/// Uses CollectionsMarshal for direct list manipulation.
/// </summary>
public static class ValueSqlBulkReader
{
    private const int DefaultBufferSize = 256;

    /// <summary>
    /// Reads all rows with maximum performance using buffering and pooling.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public static async ValueTask<List<T>> ReadAllBufferedAsync<T, TMapper>(
        SqlDataReader reader,
        TMapper mapper,
        int estimatedCount = 1000,
        CancellationToken cancellationToken = default)
        where TMapper : struct, IValueSqlMapperFast<T>
    {
        var list = new List<T>(estimatedCount);
        var buffer = ArrayPool<T>.Shared.Rent(Math.Min(estimatedCount, DefaultBufferSize));
        var bufferIndex = 0;

        try
        {
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                buffer[bufferIndex++] = mapper.MapFast(reader);

                if (bufferIndex >= buffer.Length)
                {
                    FlushBuffer(buffer, bufferIndex, list);
                    bufferIndex = 0;
                }
            }

            if (bufferIndex > 0)
            {
                FlushBuffer(buffer, bufferIndex, list);
            }

            return list;
        }
        finally
        {
            ArrayPool<T>.Shared.Return(buffer, clearArray: true);
        }
    }

    /// <summary>
    /// Synchronous bulk read with maximum performance.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public static List<T> ReadAllBufferedSync<T, TMapper>(
        SqlDataReader reader,
        TMapper mapper,
        int estimatedCount = 1000)
        where TMapper : struct, IValueSqlMapperFast<T>
    {
        var list = new List<T>(estimatedCount);
        var buffer = ArrayPool<T>.Shared.Rent(Math.Min(estimatedCount, DefaultBufferSize));
        var bufferIndex = 0;

        try
        {
            while (reader.Read())
            {
                buffer[bufferIndex++] = mapper.MapFast(reader);

                if (bufferIndex >= buffer.Length)
                {
                    FlushBuffer(buffer, bufferIndex, list);
                    bufferIndex = 0;
                }
            }

            if (bufferIndex > 0)
            {
                FlushBuffer(buffer, bufferIndex, list);
            }

            return list;
        }
        finally
        {
            ArrayPool<T>.Shared.Return(buffer, clearArray: true);
        }
    }

    /// <summary>
    /// Ultra-fast bulk read that returns a rented array instead of a list.
    /// Caller MUST return the array using ReturnSegment.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public static async ValueTask<ArraySegment<T>> ReadAllToSegmentAsync<T, TMapper>(
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
                    ArrayPool<T>.Shared.Return(array, clearArray: true);
                    array = newArray;
                }

                array[count++] = mapper.MapFast(reader);
            }

            return new ArraySegment<T>(array, 0, count);
        }
        catch
        {
            ArrayPool<T>.Shared.Return(array, clearArray: true);
            throw;
        }
    }

    /// <summary>
    /// Streams results without buffering entire result set.
    /// Best for very large result sets.
    /// </summary>
    public static async IAsyncEnumerable<T> StreamAsync<T, TMapper>(
        SqlDataReader reader,
        TMapper mapper,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
        where TMapper : struct, IValueSqlMapperFast<T>
    {
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            yield return mapper.MapFast(reader);
        }
    }

    /// <summary>
    /// Returns a rented array segment to the pool.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ReturnSegment<T>(ArraySegment<T> segment)
    {
        if (segment.Array != null)
        {
            ArrayPool<T>.Shared.Return(segment.Array, clearArray: true);
        }
    }

    /// <summary>
    /// Reads all rows using ArrayBuffer for contiguous memory.
    /// Best when you need the result as an array or span.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public static async ValueTask<T[]> ReadAllToArrayBufferAsync<T, TMapper>(
        SqlDataReader reader,
        TMapper mapper,
        int estimatedCount = 1000,
        CancellationToken cancellationToken = default)
        where TMapper : struct, IValueSqlMapperFast<T>
    {
        using var buffer = new ArrayBuffer<T>(estimatedCount);

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            buffer.Add(mapper.MapFast(reader));
        }

        return buffer.ToArray();
    }

    /// <summary>
    /// Synchronous read using ArrayBuffer for contiguous memory.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public static T[] ReadAllToArrayBufferSync<T, TMapper>(
        SqlDataReader reader,
        TMapper mapper,
        int estimatedCount = 1000)
        where TMapper : struct, IValueSqlMapperFast<T>
    {
        using var buffer = new ArrayBuffer<T>(estimatedCount);

        while (reader.Read())
        {
            buffer.Add(mapper.MapFast(reader));
        }

        return buffer.ToArray();
    }

    /// <summary>
    /// Reads all rows with ArrayBuffer and returns as List.
    /// Combines ArrayBuffer efficiency with List convenience.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public static async ValueTask<List<T>> ReadAllWithArrayBufferAsync<T, TMapper>(
        SqlDataReader reader,
        TMapper mapper,
        int estimatedCount = 1000,
        CancellationToken cancellationToken = default)
        where TMapper : struct, IValueSqlMapperFast<T>
    {
        using var buffer = new ArrayBuffer<T>(estimatedCount);

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            buffer.Add(mapper.MapFast(reader));
        }

        return buffer.ToList();
    }

    /// <summary>
    /// Ultra-fast read that keeps data in ArrayBuffer.
    /// Caller owns the ArrayBuffer and must dispose it.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public static async ValueTask<ArrayBuffer<T>> ReadAllToOwnedBufferAsync<T, TMapper>(
        SqlDataReader reader,
        TMapper mapper,
        int estimatedCount = 1000,
        CancellationToken cancellationToken = default)
        where TMapper : struct, IValueSqlMapperFast<T>
    {
        var buffer = new ArrayBuffer<T>(estimatedCount);

        try
        {
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                buffer.Add(mapper.MapFast(reader));
            }

            return buffer;
        }
        catch
        {
            buffer.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Reads binary column data efficiently using RecyclableMemoryStream.
    /// Best for large BLOB columns to avoid LOH allocations.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RecyclableMemoryStream ReadBinaryColumn(SqlDataReader reader, int ordinal)
    {
        return reader.GetBinaryAsStream(ordinal);
    }

    /// <summary>
    /// Reads multiple rows with binary columns efficiently.
    /// Uses RecyclableMemoryStream for each binary value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public static async ValueTask<List<(T Entity, RecyclableMemoryStream BinaryData)>> ReadAllWithBinaryAsync<T, TMapper>(
        SqlDataReader reader,
        TMapper mapper,
        int binaryOrdinal,
        int estimatedCount = 100,
        CancellationToken cancellationToken = default)
        where TMapper : struct, IValueSqlMapperFast<T>
    {
        var results = new List<(T, RecyclableMemoryStream)>(estimatedCount);

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var entity = mapper.MapFast(reader);
            var binaryStream = reader.GetBinaryAsStream(binaryOrdinal);
            results.Add((entity, binaryStream));
        }

        return results;
    }

    /// <summary>
    /// Ultra-fast sync read using CollectionsMarshal for direct span writes.
    /// Best performance for known row counts.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public static List<T> ReadAllDirectSync<T, TMapper>(
        SqlDataReader reader,
        TMapper mapper,
        int estimatedCount = 1000)
        where TMapper : struct, IValueSqlMapperFast<T>
    {
        var list = new List<T>(estimatedCount);
        var count = 0;

        // First pass: count and read into temp buffer
        var buffer = ArrayPool<T>.Shared.Rent(estimatedCount);
        try
        {
            while (reader.Read())
            {
                if (count >= buffer.Length)
                {
                    // Grow buffer
                    var newBuffer = ArrayPool<T>.Shared.Rent(buffer.Length * 2);
                    Array.Copy(buffer, newBuffer, count);
                    ArrayPool<T>.Shared.Return(buffer, clearArray: true);
                    buffer = newBuffer;
                }

                buffer[count++] = mapper.MapFast(reader);
            }

            // Use CollectionsMarshal for direct copy
            if (count > 0)
            {
                CollectionsMarshal.SetCount(list, count);
                var listSpan = CollectionsMarshal.AsSpan(list);
                buffer.AsSpan(0, count).CopyTo(listSpan);
            }

            return list;
        }
        finally
        {
            ArrayPool<T>.Shared.Return(buffer, clearArray: true);
        }
    }

    /// <summary>
    /// Ultra-fast async read using CollectionsMarshal for direct span writes.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public static async ValueTask<List<T>> ReadAllDirectAsync<T, TMapper>(
        SqlDataReader reader,
        TMapper mapper,
        int estimatedCount = 1000,
        CancellationToken cancellationToken = default)
        where TMapper : struct, IValueSqlMapperFast<T>
    {
        var list = new List<T>(estimatedCount);
        var count = 0;

        var buffer = ArrayPool<T>.Shared.Rent(estimatedCount);
        try
        {
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                if (count >= buffer.Length)
                {
                    var newBuffer = ArrayPool<T>.Shared.Rent(buffer.Length * 2);
                    Array.Copy(buffer, newBuffer, count);
                    ArrayPool<T>.Shared.Return(buffer, clearArray: true);
                    buffer = newBuffer;
                }

                buffer[count++] = mapper.MapFast(reader);
            }

            if (count > 0)
            {
                CollectionsMarshal.SetCount(list, count);
                var listSpan = CollectionsMarshal.AsSpan(list);
                buffer.AsSpan(0, count).CopyTo(listSpan);
            }

            return list;
        }
        finally
        {
            ArrayPool<T>.Shared.Return(buffer, clearArray: true);
        }
    }

    /// <summary>
    /// Reads with known count - fastest possible using CollectionsMarshal.
    /// Use when you know the exact row count (e.g., from COUNT(*) or ROWCOUNT).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public static List<T> ReadAllWithKnownCountSync<T, TMapper>(
        SqlDataReader reader,
        TMapper mapper,
        int exactCount)
        where TMapper : struct, IValueSqlMapperFast<T>
    {
        var list = new List<T>(exactCount);
        CollectionsMarshal.SetCount(list, exactCount);
        var span = CollectionsMarshal.AsSpan(list);

        var index = 0;
        while (reader.Read() && index < exactCount)
        {
            span[index++] = mapper.MapFast(reader);
        }

        // Trim if we got fewer rows than expected
        if (index < exactCount)
        {
            CollectionsMarshal.SetCount(list, index);
        }

        return list;
    }

    /// <summary>
    /// Async version of ReadAllWithKnownCountSync.
    /// Uses buffer + CollectionsMarshal copy pattern (Span can't cross await).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public static async ValueTask<List<T>> ReadAllWithKnownCountAsync<T, TMapper>(
        SqlDataReader reader,
        TMapper mapper,
        int exactCount,
        CancellationToken cancellationToken = default)
        where TMapper : struct, IValueSqlMapperFast<T>
    {
        var buffer = ArrayPool<T>.Shared.Rent(exactCount);
        var index = 0;

        try
        {
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false) && index < exactCount)
            {
                buffer[index++] = mapper.MapFast(reader);
            }

            var list = new List<T>(index);
            if (index > 0)
            {
                CollectionsMarshal.SetCount(list, index);
                var listSpan = CollectionsMarshal.AsSpan(list);
                buffer.AsSpan(0, index).CopyTo(listSpan);
            }

            return list;
        }
        finally
        {
            ArrayPool<T>.Shared.Return(buffer, clearArray: true);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void FlushBuffer<T>(T[] buffer, int count, List<T> list)
    {
        // Use CollectionsMarshal for direct copy instead of Add loop
        var startIndex = list.Count;
        CollectionsMarshal.SetCount(list, startIndex + count);
        var listSpan = CollectionsMarshal.AsSpan(list);
        buffer.AsSpan(0, count).CopyTo(listSpan.Slice(startIndex));
    }
}
