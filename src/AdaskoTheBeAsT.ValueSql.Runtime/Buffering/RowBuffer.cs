using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace AdaskoTheBeAsT.ValueSql.Runtime.Buffering;

/// <summary>
/// High-performance row buffer for bulk reads.
/// </summary>
/// <typeparam name="T">The element type.</typeparam>
public ref struct RowBuffer<T>
{
    private T[] _buffer;
    private int _count;
    private int _index;

    /// <summary>
    /// Creates a new row buffer.
    /// </summary>
    public RowBuffer(int bufferSize = 256)
    {
        _buffer = ArrayPool<T>.Shared.Rent(bufferSize);
        _count = 0;
        _index = 0;
    }

    /// <summary>
    /// Gets the current count of items in the buffer.
    /// </summary>
    public readonly int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _count;
    }

    /// <summary>
    /// Gets whether the buffer has more items.
    /// </summary>
    public readonly bool HasMore
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _index < _count;
    }

    /// <summary>
    /// Adds an item to the buffer.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T item)
    {
        if (_count >= _buffer.Length)
        {
            Grow();
        }

        _buffer[_count++] = item;
    }

    /// <summary>
    /// Gets the next item from the buffer.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetNext()
    {
        return _buffer[_index++];
    }

    /// <summary>
    /// Gets a span of all buffered items.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ReadOnlySpan<T> AsSpan()
    {
        return _buffer.AsSpan(0, _count);
    }

    /// <summary>
    /// Copies buffered items to a list.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void CopyTo(System.Collections.Generic.List<T> list)
    {
        var span = AsSpan();
        foreach (var item in span)
        {
            list.Add(item);
        }
    }

    /// <summary>
    /// Resets the buffer for reuse.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reset()
    {
        _count = 0;
        _index = 0;
    }

    /// <summary>
    /// Disposes the buffer and returns the array to the pool.
    /// </summary>
    public void Dispose()
    {
        if (_buffer != null)
        {
            ArrayPool<T>.Shared.Return(_buffer, clearArray: true);
            _buffer = null!;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Grow()
    {
        var newBuffer = ArrayPool<T>.Shared.Rent(_buffer.Length * 2);
        Array.Copy(_buffer, newBuffer, _count);
        ArrayPool<T>.Shared.Return(_buffer, clearArray: true);
        _buffer = newBuffer;
    }
}
