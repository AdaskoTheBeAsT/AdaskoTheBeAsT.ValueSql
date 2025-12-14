using System.Runtime.CompilerServices;
using Microsoft.IO;

namespace AdaskoTheBeAsT.ValueSql.Runtime.Buffering;

/// <summary>
/// High-performance binary buffer pool using RecyclableMemoryStream.
/// Optimized for reading large BLOB columns (varbinary(max), image, etc.).
/// </summary>
public static class BinaryBufferPool
{
    private static readonly RecyclableMemoryStreamManager Manager = CreateManager();

    /// <summary>
    /// Gets the shared RecyclableMemoryStreamManager instance.
    /// </summary>
    public static RecyclableMemoryStreamManager SharedManager => Manager;

    /// <summary>
    /// Gets a recyclable memory stream for binary data.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RecyclableMemoryStream GetStream()
    {
        return Manager.GetStream();
    }

    /// <summary>
    /// Gets a recyclable memory stream with initial capacity.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RecyclableMemoryStream GetStream(int capacity)
    {
        return Manager.GetStream(null, capacity);
    }

    /// <summary>
    /// Gets a recyclable memory stream with initial data.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RecyclableMemoryStream GetStream(System.ReadOnlySpan<byte> data)
    {
        var stream = Manager.GetStream();
        stream.Write(data);
        stream.Position = 0;
        return stream;
    }

    /// <summary>
    /// Gets a recyclable memory stream from existing buffer.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RecyclableMemoryStream GetStream(byte[] buffer, int offset, int count)
    {
        var stream = Manager.GetStream();
        stream.Write(buffer, offset, count);
        stream.Position = 0;
        return stream;
    }

    private static RecyclableMemoryStreamManager CreateManager()
    {
        var options = new RecyclableMemoryStreamManager.Options
        {
            BlockSize = 128 * 1024,
            LargeBufferMultiple = 1024 * 1024,
            MaximumBufferSize = 128 * 1024 * 1024,
            GenerateCallStacks = false,
            AggressiveBufferReturn = true,
            MaximumSmallPoolFreeBytes = 16 * 1024 * 1024,
            MaximumLargePoolFreeBytes = 64 * 1024 * 1024,
        };

        return new RecyclableMemoryStreamManager(options);
    }
}
