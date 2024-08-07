namespace ConsoleApp.Utils;

public static class SpanEqualityUtil
{
    public static bool Equals(ReadOnlySpan<byte> x, ReadOnlySpan<byte> y) => x.SequenceEqual(y);

    public static int GetHashCode(ReadOnlySpan<byte> span)
    {
        unchecked
        {
            const int prime = 31;
            var hash = 0;
            foreach (var b in span)
            {
                hash = (hash * prime) + b;
            }

            return hash;
        }
    }
}
