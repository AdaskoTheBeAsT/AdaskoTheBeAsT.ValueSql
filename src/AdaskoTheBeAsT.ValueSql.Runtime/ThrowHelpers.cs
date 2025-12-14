using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace AdaskoTheBeAsT.ValueSql.Runtime;

/// <summary>
/// Throw helper methods marked with NoInlining to keep exception paths
/// out of hot code paths, improving JIT optimization of the happy path.
/// </summary>
public static class ThrowHelpers
{
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowArgumentNull(string paramName)
    {
        throw new ArgumentNullException(paramName);
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowArgumentOutOfRange(string paramName, string? message = null)
    {
        throw new ArgumentOutOfRangeException(paramName, message);
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowInvalidOperation(string message)
    {
        throw new InvalidOperationException(message);
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowConnectionNotOpen()
    {
        throw new InvalidOperationException("Connection must be open before executing a command.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowReaderClosed()
    {
        throw new InvalidOperationException("DataReader has been closed.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowColumnNotFound(string columnName)
    {
        throw new ArgumentException($"Column '{columnName}' not found in result set.", nameof(columnName));
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowOrdinalOutOfRange(int ordinal, int columnCount)
    {
        throw new ArgumentOutOfRangeException(
            nameof(ordinal),
            ordinal,
            $"Ordinal {ordinal} is out of range. Column count: {columnCount}");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowNullValue(string columnName)
    {
        throw new InvalidCastException($"Column '{columnName}' contains a null value.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowTypeMismatch(string columnName, Type expectedType, Type actualType)
    {
        throw new InvalidCastException(
            $"Column '{columnName}' type mismatch. Expected: {expectedType.Name}, Actual: {actualType.Name}");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowBufferTooSmall(int required, int actual)
    {
        throw new ArgumentException($"Buffer too small. Required: {required}, Actual: {actual}", nameof(actual));
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static T ThrowAndReturn<T>(Exception exception)
    {
        throw exception;
    }

    /// <summary>
    /// Validates connection state, throws if not open.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ValidateConnectionOpen(ConnectionState state)
    {
        if (state != ConnectionState.Open)
        {
            ThrowConnectionNotOpen();
        }
    }

    /// <summary>
    /// Validates that argument is not null.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ValidateNotNull<T>([NotNull] T? value, string paramName)
        where T : class
    {
        if (value is null)
        {
            ThrowArgumentNull(paramName);
        }
    }
}
