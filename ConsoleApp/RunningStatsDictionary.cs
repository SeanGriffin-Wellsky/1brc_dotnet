using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ConsoleApp;

public sealed class RunningStatsDictionary(int capacity) : IEnumerable<KeyValuePair<ReadOnlyMemory<char>, RunningStats>>
{
    // Use larger hashtable size To reduce # of hash code clashes
    // Ensure odd number to reduce clustering (ideally prime but calculating the prime is expensive)
    private readonly int _hashTableSize = capacity * 4 + 1;

    private readonly List<KeyValuePair<ReadOnlyMemory<char>, RunningStats>>?[] _dict =
        new List<KeyValuePair<ReadOnlyMemory<char>, RunningStats>>?[capacity * 4 + 1];

    public int Count => GetEnumerable().Count();

    public int GetHashCode(ReadOnlySpan<char> key) =>
        (SpanEqualityUtil.GetHashCode(key) & int.MaxValue) % _hashTableSize;

    public bool TryGetValue(int keyHashCode, ReadOnlySpan<char> key, [MaybeNullWhen(false)] out RunningStats stats)
    {
        Debug.Assert(keyHashCode == GetHashCode(key));

        var matches = _dict[keyHashCode];
        if (matches is not null)
        {
            stats = FindMatch(key, matches);
            return stats is not null;
        }

        stats = null;
        return false;
    }

    public void Add(int keyHashCode, ReadOnlySpan<char> key, RunningStats value)
    {
        Debug.Assert(keyHashCode == GetHashCode(key));

        var matches = _dict[keyHashCode];
        if (matches is not null)
        {
            Debug.Assert(FindMatch(key, matches) is null);
        }
        else
        {
            matches = new List<KeyValuePair<ReadOnlyMemory<char>, RunningStats>>(5);
            _dict[keyHashCode] = matches;
        }

        var keyBuffer = new char[key.Length];
        key.CopyTo(keyBuffer);
        var keyAsMemory = new ReadOnlyMemory<char>(keyBuffer);

        matches.Add(KeyValuePair.Create(keyAsMemory, value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static RunningStats? FindMatch(ReadOnlySpan<char> key, List<KeyValuePair<ReadOnlyMemory<char>, RunningStats>> potentialMatches)
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

    // public SortedDictionary<string, RunningStats> ToFinalDictionary()
    // {
    //     var finalDict = new SortedDictionary<string, RunningStats>(StringComparer.Ordinal);
    //     foreach (var kv in this)
    //     {
    //         finalDict.Add(kv.Key.ToString(), kv.Value);
    //     }
    //
    //     return finalDict;
    // }

    public void DumpDict()
    {
        Console.WriteLine(string.Join(',', _dict.Select(entry => entry?.Count ?? 0)));
    }

    public IEnumerator<KeyValuePair<ReadOnlyMemory<char>, RunningStats>> GetEnumerator() => GetEnumerable().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private IEnumerable<KeyValuePair<ReadOnlyMemory<char>, RunningStats>> GetEnumerable() =>
        _dict.Where(v => v is not null).SelectMany(x => x!);
}