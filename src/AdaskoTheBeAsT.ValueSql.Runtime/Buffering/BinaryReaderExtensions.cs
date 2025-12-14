using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Data.SqlClient;
using Microsoft.IO;

namespace AdaskoTheBeAsT.ValueSql.Runtime.Buffering;

/// <summary>
/// Extension methods for efficient binary reading from SqlDataReader.
/// </summary>
public static class BinaryReaderExtensions
{
    private const int DefaultBufferSize = 81920;

    /// <summary>
    /// Reads a binary column into a RecyclableMemoryStream.
    /// Best for large BLOB columns to avoid LOH allocations.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RecyclableMemoryStream GetBinaryAsStream(
        this SqlDataReader reader,
        int ordinal)
    {
        var stream = BinaryBufferPool.GetStream();

        if (reader.IsDBNull(ordinal))
        {
            return stream;
        }

        var buffer = ArrayPool<byte>.Shared.Rent(DefaultBufferSize);
        try
        {
            long offset = 0;
            int bytesRead;

            while ((bytesRead = (int)reader.GetBytes(ordinal, offset, buffer, 0, buffer.Length)) > 0)
            {
                stream.Write(buffer, 0, bytesRead);
                offset += bytesRead;
            }

            stream.Position = 0;
            return stream;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    /// <summary>
    /// Reads a binary column into a rented byte array.
    /// Caller MUST return the array using ArrayPool.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (byte[] Array, int Length) GetBinaryAsRentedArray(
        this SqlDataReader reader,
        int ordinal)
    {
        if (reader.IsDBNull(ordinal))
        {
            return (Array.Empty<byte>(), 0);
        }

        var length = (int)reader.GetBytes(ordinal, 0, null, 0, 0);
        if (length == 0)
        {
            return (Array.Empty<byte>(), 0);
        }

        var array = ArrayPool<byte>.Shared.Rent(length);
        reader.GetBytes(ordinal, 0, array, 0, length);

        return (array, length);
    }

    /// <summary>
    /// Reads a binary column using streaming for very large BLOBs.
    /// Returns a stream that reads directly from the reader.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Stream? GetBinaryStream(
        this SqlDataReader reader,
        int ordinal)
    {
        if (reader.IsDBNull(ordinal))
        {
            return null;
        }

        return reader.GetStream(ordinal);
    }
}
