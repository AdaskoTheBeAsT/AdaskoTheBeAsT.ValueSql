#pragma warning disable CC0001 // Use var - nint requires explicit type

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AdaskoTheBeAsT.ValueSql.Runtime.Parsing;

/// <summary>
/// Ultra-fast parsing utilities using SIMD-style optimizations.
/// Based on AXON parser techniques.
/// </summary>
public static class FastParsers
{
    /// <summary>
    /// Parses a long integer 4 digits at a time for maximum throughput.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    [SkipLocalsInit]
    public static long ParseLongFast(ReadOnlySpan<char> s)
    {
        if (s.IsEmpty)
        {
            return 0;
        }

        ref char r = ref MemoryMarshal.GetReference(s);
        var neg = r == '-';
        nint i = neg ? 1 : 0;
        nint len = s.Length;
        var result = 0L;

        // Process 4 digits at a time - reduces loop overhead by 4x
        while (i + 4 <= len)
        {
            result = (result * 10000)
                + ((Unsafe.Add(ref r, i) - '0') * 1000)
                + ((Unsafe.Add(ref r, i + 1) - '0') * 100)
                + ((Unsafe.Add(ref r, i + 2) - '0') * 10)
                + (Unsafe.Add(ref r, i + 3) - '0');
            i += 4;
        }

        // Handle remaining digits
        while (i < len)
        {
            result = (result * 10) + (Unsafe.Add(ref r, i++) - '0');
        }

        return neg ? -result : result;
    }

    /// <summary>
    /// Parses an int 4 digits at a time.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    [SkipLocalsInit]
    public static int ParseIntFast(ReadOnlySpan<char> s)
    {
        if (s.IsEmpty)
        {
            return 0;
        }

        ref char r = ref MemoryMarshal.GetReference(s);
        var neg = r == '-';
        nint i = neg ? 1 : 0;
        nint len = s.Length;
        var result = 0;

        // Process 4 digits at a time
        while (i + 4 <= len)
        {
            result = (result * 10000)
                + ((Unsafe.Add(ref r, i) - '0') * 1000)
                + ((Unsafe.Add(ref r, i + 1) - '0') * 100)
                + ((Unsafe.Add(ref r, i + 2) - '0') * 10)
                + (Unsafe.Add(ref r, i + 3) - '0');
            i += 4;
        }

        // Handle remaining digits
        while (i < len)
        {
            result = (result * 10) + (Unsafe.Add(ref r, i++) - '0');
        }

        return neg ? -result : result;
    }

    /// <summary>
    /// Branch-free boolean parsing. Returns true for '1', 't', 'T', 'y', 'Y'.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ParseBoolFast(ReadOnlySpan<char> s)
    {
        return !s.IsEmpty && s[0] is '1' or 't' or 'T' or 'y' or 'Y';
    }

    /// <summary>
    /// Branch-free bit to bool conversion.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool BitToBool(int bit) => bit != 0;

    /// <summary>
    /// Parses a short integer.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    [SkipLocalsInit]
    public static short ParseShortFast(ReadOnlySpan<char> s)
    {
        if (s.IsEmpty)
        {
            return 0;
        }

        ref char r = ref MemoryMarshal.GetReference(s);
        var neg = r == '-';
        nint i = neg ? 1 : 0;
        nint len = s.Length;
        var result = 0;

        while (i < len)
        {
            result = (result * 10) + (Unsafe.Add(ref r, i++) - '0');
        }

        return (short)(neg ? -result : result);
    }

    /// <summary>
    /// Parses a byte (unsigned).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    [SkipLocalsInit]
    public static byte ParseByteFast(ReadOnlySpan<char> s)
    {
        if (s.IsEmpty)
        {
            return 0;
        }

        ref char r = ref MemoryMarshal.GetReference(s);
        nint i = 0;
        nint len = s.Length;
        var result = 0;

        while (i < len)
        {
            result = (result * 10) + (Unsafe.Add(ref r, i++) - '0');
        }

        return (byte)result;
    }
}
