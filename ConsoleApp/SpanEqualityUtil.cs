using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text;

namespace ConsoleApp;

public static class SpanEqualityUtil
{
    //private static readonly ConcurrentDictionary<int, int> HashCodes = [];

    public static bool Equals(ReadOnlySpan<byte> x, ReadOnlySpan<byte> y) => x.SequenceEqual(y);

    public static int Compare(ReadOnlySpan<byte> x, ReadOnlySpan<byte> y) => x.SequenceCompareTo(y);

    public static int GetHashCode(ReadOnlySpan<byte> span)
    {
        var div = span.Length / 4;

        var hash = 31;
        for (var i = 0; i < div; ++i)
        {
            hash ^= BitConverter.ToInt32(span.Slice(i * 4, 4));
        }

        return hash ^ span.Length;

        //HashCodes[Encoding.UTF8.GetString(span.ToArray())] = hash;
        // HashCodes[hash] = hash;
        // return hash;
    }

 //   public static IDictionary<int, int> GetHashCodes => HashCodes;
}
