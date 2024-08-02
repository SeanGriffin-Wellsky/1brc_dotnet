namespace ConsoleApp;

public static class SpanEqualityUtil
{
    public static bool Equals(ReadOnlySpan<char> x, ReadOnlySpan<char> y) => x.SequenceEqual(y);

    public static int GetHashCode(ReadOnlySpan<char> span)
    {
        unchecked
        {
            const int prime = 31;
            var hash = 0;
            foreach (var c in span)
            {
                hash = (hash * prime) + c;
            }

            return hash;
        }
    }
}
