using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AdaskoTheBeAsT.ValueSql.Runtime.Buffering;

/// <summary>
/// High-performance array buffer using ArrayPool for contiguous memory.
/// Better than List when final result needs to be a contiguous array.
/// </summary>
/// <typeparam name="T">The element type.</typeparam>
public sealed class ArrayBuffer<T> : IDisposable
{
    private T[] _buffer;
    private int _count;
    private bool _disposed;

    /// <summary>
    /// Creates a new array buffer with the specified initial capacity.
    /// </summary>
    public ArrayBuffer(int initialCapacity = 256)
    {
        _buffer = ArrayPool<T>.Shared.Rent(initialCapacity);
        _count = 0;
    }

    /// <summary>
    /// Gets the number of items in the buffer.
    /// </summary>
    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _count;
    }

    /// <summary>
    /// Gets the current capacity of the buffer.
    /// </summary>
    public int Capacity
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _buffer.Length;
    }

    /// <summary>
    /// Gets a span of the written data.
    /// </summary>
    public ReadOnlySpan<T> WrittenSpan
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _buffer.AsSpan(0, _count);
    }

    /// <summary>
    /// Gets a memory of the written data.
    /// </summary>
    public ReadOnlyMemory<T> WrittenMemory
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _buffer.AsMemory(0, _count);
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
    /// Adds multiple items to the buffer.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddRange(ReadOnlySpan<T> items)
    {
        EnsureCapacity(_count + items.Length);
        items.CopyTo(_buffer.AsSpan(_count));
        _count += items.Length;
    }

    /// <summary>
    /// Converts to an array. Creates a new array with exact size.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T[] ToArray()
    {
        if (_count == 0)
        {
            return Array.Empty<T>();
        }

        var result = new T[_count];
        Array.Copy(_buffer, result, _count);
        return result;
    }

    /// <summary>
    /// Converts to a list.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public List<T> ToList()
    {
        var list = new List<T>(_count);
        var span = WrittenSpan;
        foreach (var item in span)
        {
            list.Add(item);
        }

        return list;
    }

    /// <summary>
    /// Gets the underlying array segment.
    /// WARNING: Do not use after Dispose or Reset.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ArraySegment<T> GetArraySegment()
    {
        return new ArraySegment<T>(_buffer, 0, _count);
    }

    /// <summary>
    /// Resets the buffer for reuse without returning to pool.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reset()
    {
        _count = 0;
    }

    /// <summary>
    /// Ensures the buffer has at least the specified capacity.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureCapacity(int capacity)
    {
        if (capacity > _buffer.Length)
        {
            GrowTo(capacity);
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposed)
        {
            ArrayPool<T>.Shared.Return(_buffer, clearArray: RuntimeHelpers.IsReferenceOrContainsReferences<T>());
            _buffer = null!;
            _disposed = true;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Grow()
    {
        GrowTo(_buffer.Length * 2);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GrowTo(int minCapacity)
    {
        var newCapacity = Math.Max(minCapacity, _buffer.Length * 2);
        var newBuffer = ArrayPool<T>.Shared.Rent(newCapacity);
        Array.Copy(_buffer, newBuffer, _count);
        ArrayPool<T>.Shared.Return(_buffer, clearArray: RuntimeHelpers.IsReferenceOrContainsReferences<T>());
        _buffer = newBuffer;
    }
}
