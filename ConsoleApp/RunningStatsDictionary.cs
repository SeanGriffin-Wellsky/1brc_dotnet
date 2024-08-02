using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace ConsoleApp;

public sealed class RunningStatsDictionary(int capacity) : IEnumerable<KeyValuePair<ReadOnlyMemory<byte>, RunningStats>>
{
    private readonly Dictionary<int, List<KeyValuePair<ReadOnlyMemory<byte>, RunningStats>>> _dict = new(capacity);

    public int Count => GetEnumerable().Count();

    public bool TryGetValue(int keyHashCode, ReadOnlySpan<byte> key, [MaybeNullWhen(false)] out RunningStats stats)
    {
        Debug.Assert(keyHashCode == SpanEqualityUtil.GetHashCode(key));

        if (_dict.TryGetValue(keyHashCode, out var matches))
        {
            stats = FindMatch(key, matches);
            return stats is not null;
        }

        stats = null;
        return false;
    }

    public void Add(int keyHashCode, ReadOnlySpan<byte> key, RunningStats value)
    {
        Debug.Assert(keyHashCode == SpanEqualityUtil.GetHashCode(key));

        if (_dict.TryGetValue(keyHashCode, out var matches))
        {
            Debug.Assert(FindMatch(key, matches) is null);
        }
        else
        {
            matches = new List<KeyValuePair<ReadOnlyMemory<byte>, RunningStats>>(5);
            _dict.Add(keyHashCode, matches);
        }

        var keyBuffer = new byte[key.Length];
        key.CopyTo(keyBuffer);
        var keyAsMemory = new ReadOnlyMemory<byte>(keyBuffer);

        matches.Add(KeyValuePair.Create(keyAsMemory, value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static RunningStats? FindMatch(ReadOnlySpan<byte> key, List<KeyValuePair<ReadOnlyMemory<byte>, RunningStats>> potentialMatches)
    {
        foreach (var match in CollectionsMarshal.AsSpan(potentialMatches))
        {
            if (SpanEqualityUtil.Equals(key, match.Key.Span))
            {
                return match.Value;
            }
        }

        return null;
    }

    public List<int> DumpCountsPerBucket() => _dict.Select(kvp => kvp.Value.Count).ToList();

    public IEnumerator<KeyValuePair<ReadOnlyMemory<byte>, RunningStats>> GetEnumerator() => GetEnumerable().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private IEnumerable<KeyValuePair<ReadOnlyMemory<byte>, RunningStats>> GetEnumerable() =>
        _dict.Values.SelectMany(valueList => valueList);
}