using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ConsoleApp;

public sealed class RunningStatsDictionary(int capacity) : IEnumerable<KeyValuePair<string, RunningStats>>
{
    private readonly Dictionary<int, List<KeyValuePair<string, RunningStats>>> _dict = new(capacity);

    public int Count => GetEnumerable().Count();

    public bool TryGetValue(int keyHashCode, ReadOnlySpan<char> key, [MaybeNullWhen(false)] out RunningStats stats)
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

    public void Add(int keyHashCode, ReadOnlySpan<char> key, RunningStats value)
    {
        Debug.Assert(keyHashCode == SpanEqualityUtil.GetHashCode(key));

        if (_dict.TryGetValue(keyHashCode, out var matches))
        {
            Debug.Assert(FindMatch(key, matches) is null);
        }
        else
        {
            matches = new List<KeyValuePair<string, RunningStats>>(5);
            _dict.Add(keyHashCode, matches);
        }

        var keyBuffer = new char[key.Length];
        key.CopyTo(keyBuffer);
        var keyAsStr = new string(keyBuffer);

        matches.Add(KeyValuePair.Create(keyAsStr, value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static RunningStats? FindMatch(ReadOnlySpan<char> key, List<KeyValuePair<string, RunningStats>> potentialMatches)
    {
        foreach (var match in CollectionsMarshal.AsSpan(potentialMatches))
        {
            if (SpanEqualityUtil.Equals(key, match.Key.AsSpan()))
            {
                return match.Value;
            }
        }

        return null;
    }

    public List<int> DumpCountsPerBucket() => _dict.Select(kvp => kvp.Value.Count).ToList();

    public IEnumerator<KeyValuePair<string, RunningStats>> GetEnumerator() => GetEnumerable().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private IEnumerable<KeyValuePair<string, RunningStats>> GetEnumerable() =>
        _dict.Values.SelectMany(valueList => valueList);
}