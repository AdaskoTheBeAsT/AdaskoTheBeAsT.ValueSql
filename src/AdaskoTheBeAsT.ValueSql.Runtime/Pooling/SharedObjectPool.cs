using System.Runtime.CompilerServices;

namespace AdaskoTheBeAsT.ValueSql.Runtime.Pooling;

/// <summary>
/// Static pool accessor for common types.
/// </summary>
public static class SharedObjectPool
{
    /// <summary>
    /// Gets a shared pool for the specified type.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ObjectPool<T> Get<T>()
        where T : class, new()
    {
        return SharedPool<T>.Instance;
    }

    private static class SharedPool<T>
        where T : class, new()
    {
        public static readonly ObjectPool<T> Instance = new(maxSize: 1024);
    }
}
