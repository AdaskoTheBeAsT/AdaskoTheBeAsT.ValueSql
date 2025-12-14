using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace AdaskoTheBeAsT.ValueSql.Runtime.Simd;

/// <summary>
/// SIMD-accelerated parsing utilities for numeric conversions.
/// </summary>
public static class SimdParser
{
    /// <summary>
    /// Parses multiple integers from a span using SIMD when available.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ParseInt32Batch(ReadOnlySpan<byte> source, Span<int> destination)
    {
        if (Avx2.IsSupported && source.Length >= 32 && destination.Length >= 8)
        {
            ParseInt32BatchAvx2(source, destination);
        }
        else if (Sse2.IsSupported && source.Length >= 16 && destination.Length >= 4)
        {
            ParseInt32BatchSse2(source, destination);
        }
        else
        {
            ParseInt32BatchScalar(source, destination);
        }
    }

    /// <summary>
    /// Fast decimal parsing optimized for database values.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public static decimal ParseDecimalFast(ReadOnlySpan<char> span)
    {
        if (span.IsEmpty)
        {
            return 0m;
        }

        if (decimal.TryParse(span, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        return 0m;
    }

    /// <summary>
    /// Fast double parsing optimized for database values.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public static double ParseDoubleFast(ReadOnlySpan<char> span)
    {
        if (span.IsEmpty)
        {
            return 0.0;
        }

        if (double.TryParse(span, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        return 0.0;
    }

    /// <summary>
    /// Fast integer parsing using span.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public static int ParseInt32Fast(ReadOnlySpan<char> span)
    {
        if (span.IsEmpty)
        {
            return 0;
        }

        var result = 0;
        var negative = false;
        var i = 0;

        if (span[0] == '-')
        {
            negative = true;
            i = 1;
        }
        else if (span[0] == '+')
        {
            i = 1;
        }

        for (; i < span.Length; i++)
        {
            var c = span[i];
            if (c is < '0' or > '9')
            {
                break;
            }

            result = (result * 10) + (c - '0');
        }

        return negative ? -result : result;
    }

    /// <summary>
    /// Fast long parsing using span.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public static long ParseInt64Fast(ReadOnlySpan<char> span)
    {
        if (span.IsEmpty)
        {
            return 0L;
        }

        var result = 0L;
        var negative = false;
        var i = 0;

        if (span[0] == '-')
        {
            negative = true;
            i = 1;
        }
        else if (span[0] == '+')
        {
            i = 1;
        }

        for (; i < span.Length; i++)
        {
            var c = span[i];
            if (c is < '0' or > '9')
            {
                break;
            }

            result = (result * 10) + (c - '0');
        }

        return negative ? -result : result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ParseInt32BatchAvx2(ReadOnlySpan<byte> source, Span<int> destination)
    {
        ref var srcRef = ref MemoryMarshal.GetReference(source);
        ref var dstRef = ref MemoryMarshal.GetReference(destination);

        var ascii0 = Vector256.Create((byte)'0');
        var vec = Vector256.LoadUnsafe(ref srcRef);
        var digits = Avx2.Subtract(vec, ascii0);

        for (var i = 0; i < Math.Min(8, destination.Length); i++)
        {
            Unsafe.Add(ref dstRef, i) = digits.GetElement(i * 4);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ParseInt32BatchSse2(ReadOnlySpan<byte> source, Span<int> destination)
    {
        ref var srcRef = ref MemoryMarshal.GetReference(source);
        ref var dstRef = ref MemoryMarshal.GetReference(destination);

        var ascii0 = Vector128.Create((byte)'0');
        var vec = Vector128.LoadUnsafe(ref srcRef);
        var digits = Sse2.Subtract(vec, ascii0);

        for (var i = 0; i < Math.Min(4, destination.Length); i++)
        {
            Unsafe.Add(ref dstRef, i) = digits.GetElement(i * 4);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ParseInt32BatchScalar(ReadOnlySpan<byte> source, Span<int> destination)
    {
        var count = Math.Min(source.Length / 4, destination.Length);
        for (var i = 0; i < count; i++)
        {
            destination[i] = BitConverter.ToInt32(source.Slice(i * 4, 4));
        }
    }
}
