using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace AdaskoTheBeAsT.ValueSql.Runtime.Pooling;

/// <summary>
/// High-performance list pool to avoid List allocations during bulk reads.
/// </summary>
/// <typeparam name="T">The element type.</typeparam>
public static class ListPool<T>
{
    private const int MaxPoolSize = 16;
    private const int DefaultCapacity = 1024;

    [ThreadStatic]
    private static List<T>?[]? _tlsPool;

    /// <summary>
    /// Rents a list from the pool.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static List<T> Rent(int capacity = DefaultCapacity)
    {
        var pool = _tlsPool ??= new List<T>?[MaxPoolSize];

        for (var i = 0; i < pool.Length; i++)
        {
            var list = Interlocked.Exchange(ref pool[i], null);
            if (list != null)
            {
                if (list.Capacity < capacity)
                {
                    list.Capacity = capacity;
                }

                return list;
            }
        }

        return new List<T>(capacity);
    }

    /// <summary>
    /// Returns a list to the pool.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Return(List<T> list)
    {
        list.Clear();

        var pool = _tlsPool;
        if (pool == null)
        {
            return;
        }

        for (var i = 0; i < pool.Length; i++)
        {
            if (Interlocked.CompareExchange(ref pool[i], list, null) == null)
            {
                return;
            }
        }
    }
}
