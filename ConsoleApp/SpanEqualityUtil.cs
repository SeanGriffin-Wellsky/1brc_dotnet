using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text;

namespace ConsoleApp;

public static class SpanEqualityUtil
{
    public static bool Equals(ReadOnlySpan<byte> x, ReadOnlySpan<byte> y) => x.SequenceEqual(y);

    public static bool Equals(ReadOnlySpan<char> x, ReadOnlySpan<char> y) => x.SequenceEqual(y);

    public static int Compare(ReadOnlySpan<byte> x, ReadOnlySpan<byte> y) => x.SequenceCompareTo(y);

    public static int Compare(ReadOnlySpan<char> x, ReadOnlySpan<char> y) => x.SequenceCompareTo(y);

    public static int GetHashCode(ReadOnlySpan<byte> span)
    {
        var div = span.Length / 4;

        var hash = 31;
        for (var i = 0; i < div; ++i)
        {
            hash ^= BitConverter.ToInt32(span.Slice(i * 4, 4));
        }

        return hash ^ span.Length;
    }

    public static int GetHashCode(ReadOnlySpan<char> span)
    {
        var hash = 31;
        foreach (var c in span)
        {
            hash ^= c;
        }

        return hash ^ span.Length;
    }
}
