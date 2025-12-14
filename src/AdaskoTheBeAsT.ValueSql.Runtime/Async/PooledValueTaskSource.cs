using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks.Sources;

namespace AdaskoTheBeAsT.ValueSql.Runtime.Async;

/// <summary>
/// Pooled IValueTaskSource implementation for zero-allocation async operations.
/// Based on patterns from System.IO.Pipelines and Kestrel.
/// </summary>
/// <typeparam name="T">The result type.</typeparam>
public sealed class PooledValueTaskSource<T> : IValueTaskSource<T>, IValueTaskSource
{
    private static readonly Pool SharedPool = new();

    private ManualResetValueTaskSourceCore<T> _core;
    private volatile int _isInUse;

    private PooledValueTaskSource()
    {
        _core = default;
    }

    /// <summary>
    /// Gets the current version token.
    /// </summary>
    public short Version
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _core.Version;
    }

    /// <summary>
    /// Rents a pooled value task source.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PooledValueTaskSource<T> Rent()
    {
        var source = SharedPool.RentFromPool();
        source._core.Reset();
        source._isInUse = 1;
        return source;
    }

    /// <summary>
    /// Sets the result and completes the task.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetResult(T result)
    {
        _core.SetResult(result);
    }

    /// <summary>
    /// Sets an exception and completes the task.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetException(Exception exception)
    {
        _core.SetException(exception);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetResult(short token)
    {
        try
        {
            return _core.GetResult(token);
        }
        finally
        {
            ReturnToPool();
        }
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void IValueTaskSource.GetResult(short token)
    {
        try
        {
            _core.GetResult(token);
        }
        finally
        {
            ReturnToPool();
        }
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTaskSourceStatus GetStatus(short token)
    {
        return _core.GetStatus(token);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
    {
        _core.OnCompleted(continuation, state, token, flags);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ReturnToPool()
    {
        if (Interlocked.Exchange(ref _isInUse, 0) == 1)
        {
            SharedPool.ReturnToPool(this);
        }
    }

    private sealed class Pool
    {
        private const int MaxPoolSize = 64;
        private readonly PooledValueTaskSource<T>?[] _items = new PooledValueTaskSource<T>?[MaxPoolSize];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PooledValueTaskSource<T> RentFromPool()
        {
            for (var i = 0; i < _items.Length; i++)
            {
                var item = Interlocked.Exchange(ref _items[i], null);
                if (item != null)
                {
                    return item;
                }
            }

            return new PooledValueTaskSource<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReturnToPool(PooledValueTaskSource<T> item)
        {
            for (var i = 0; i < _items.Length; i++)
            {
                if (Interlocked.CompareExchange(ref _items[i], item, null) == null)
                {
                    return;
                }
            }
        }
    }
}
