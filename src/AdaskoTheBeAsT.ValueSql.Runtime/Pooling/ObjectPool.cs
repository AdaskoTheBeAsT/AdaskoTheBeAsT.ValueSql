using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;

namespace AdaskoTheBeAsT.ValueSql.Runtime.Pooling;

/// <summary>
/// High-performance object pool for entity reuse.
/// </summary>
/// <typeparam name="T">The type of objects to pool.</typeparam>
public sealed class ObjectPool<T>
    where T : class, new()
{
    private readonly ConcurrentQueue<T> _objects;
    private readonly Func<T> _factory;
    private readonly Action<T>? _reset;
    private readonly int _maxSize;
    private int _count;

    /// <summary>
    /// Creates a new object pool.
    /// </summary>
    public ObjectPool(int maxSize = 1024, Func<T>? factory = null, Action<T>? reset = null)
    {
        _maxSize = maxSize;
        _factory = factory ?? (() => new T());
        _reset = reset;
        _objects = new ConcurrentQueue<T>();
    }

    /// <summary>
    /// Gets an object from the pool or creates a new one.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Rent()
    {
        if (_objects.TryDequeue(out var item))
        {
            Interlocked.Decrement(ref _count);
            return item;
        }

        return _factory();
    }

    /// <summary>
    /// Returns an object to the pool.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Return(T obj)
    {
        if (Volatile.Read(ref _count) < _maxSize)
        {
            _reset?.Invoke(obj);
            _objects.Enqueue(obj);
            Interlocked.Increment(ref _count);
        }
    }
}
