using System;
using System.Runtime.CompilerServices;
using Microsoft.Data.SqlClient;

namespace AdaskoTheBeAsT.ValueSql.Runtime;

/// <summary>
/// Unsafe high-performance reader extensions for maximum throughput.
/// </summary>
public static class UnsafeReaderExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public static T? GetNullableValue<T>(this SqlDataReader reader, int ordinal)
        where T : struct
    {
        return reader.IsDBNull(ordinal) ? null : reader.GetFieldValue<T>(ordinal);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public static string? GetNullableString(this SqlDataReader reader, int ordinal)
    {
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public static int GetInt32Fast(this SqlDataReader reader, int ordinal)
    {
        return reader.GetInt32(ordinal);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public static long GetInt64Fast(this SqlDataReader reader, int ordinal)
    {
        return reader.GetInt64(ordinal);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public static decimal GetDecimalFast(this SqlDataReader reader, int ordinal)
    {
        return reader.GetDecimal(ordinal);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public static double GetDoubleFast(this SqlDataReader reader, int ordinal)
    {
        return reader.GetDouble(ordinal);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public static bool GetBooleanFast(this SqlDataReader reader, int ordinal)
    {
        return reader.GetBoolean(ordinal);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public static DateTime GetDateTimeFast(this SqlDataReader reader, int ordinal)
    {
        return reader.GetDateTime(ordinal);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public static Guid GetGuidFast(this SqlDataReader reader, int ordinal)
    {
        return reader.GetGuid(ordinal);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public static string GetStringFast(this SqlDataReader reader, int ordinal)
    {
        return reader.GetString(ordinal);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public static bool IsDBNullFast(this SqlDataReader reader, int ordinal)
    {
        return reader.IsDBNull(ordinal);
    }
}
