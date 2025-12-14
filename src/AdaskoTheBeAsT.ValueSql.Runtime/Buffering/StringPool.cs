using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace AdaskoTheBeAsT.ValueSql.Runtime.Buffering;

/// <summary>
/// String interning pool for deduplicating repeated string values.
/// Reduces memory for columns with repeated values (e.g., categories, statuses).
/// </summary>
public sealed class StringPool
{
    private readonly ConcurrentDictionary<int, WeakReference<string>> _pool;
    private readonly int _maxSize;

    /// <summary>
    /// Creates a new string pool.
    /// </summary>
    public StringPool(int maxSize = 10000)
    {
        _maxSize = maxSize;
        _pool = new ConcurrentDictionary<int, WeakReference<string>>();
    }

    /// <summary>
    /// Gets the shared string pool instance.
    /// </summary>
    public static StringPool Shared { get; } = new(maxSize: 10000);

    /// <summary>
    /// Gets or adds a string to the pool.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string GetOrAdd(string value)
    {
        if (string.IsNullOrEmpty(value) || value.Length > 100)
        {
            return value;
        }

        var hash = string.GetHashCode(value, StringComparison.Ordinal);

        if (_pool.TryGetValue(hash, out var weakRef) &&
            weakRef.TryGetTarget(out var existing) &&
            string.Equals(existing, value, StringComparison.Ordinal))
        {
            return existing;
        }

        if (_pool.Count < _maxSize)
        {
            _pool[hash] = new WeakReference<string>(value);
        }

        return value;
    }

    /// <summary>
    /// Gets or adds a string from a span.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string GetOrAdd(ReadOnlySpan<char> value)
    {
        if (value.IsEmpty || value.Length > 100)
        {
            return value.ToString();
        }

        var hash = string.GetHashCode(value, StringComparison.Ordinal);

        if (_pool.TryGetValue(hash, out var weakRef) &&
            weakRef.TryGetTarget(out var existing) &&
            value.SequenceEqual(existing.AsSpan()))
        {
            return existing;
        }

        var str = value.ToString();

        if (_pool.Count < _maxSize)
        {
            _pool[hash] = new WeakReference<string>(str);
        }

        return str;
    }

    /// <summary>
    /// Clears the pool.
    /// </summary>
    public void Clear()
    {
        _pool.Clear();
    }
}
