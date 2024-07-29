using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace ConsoleApp;

public sealed class RunningStatsDictionary(int capacity) : IEnumerable<KeyValuePair<ReadOnlyMemory<byte>, RunningStats>>
{
    private readonly int _hashTableSize = capacity * 4 + 1;

    private readonly List<KeyValuePair<ReadOnlyMemory<byte>, RunningStats>>?[] _dict =
        new List<KeyValuePair<ReadOnlyMemory<byte>, RunningStats>>?[capacity * 4 + 1];

    public int Count => GetEnumerable().Count();

    public int GetHashCode(ReadOnlySpan<byte> key) =>
        (SpanEqualityUtil.GetHashCode(key) & int.MaxValue) % _hashTableSize;

    public bool TryGetValue(int keyHashCode, ReadOnlySpan<byte> key, [MaybeNullWhen(false)] out RunningStats stats)
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

    public void Add(int keyHashCode, ReadOnlySpan<byte> key, RunningStats value)
    {
        Debug.Assert(keyHashCode == GetHashCode(key));

        var matches = _dict[keyHashCode];
        if (matches is not null)
        {
            Debug.Assert(FindMatch(key, matches) is null);
        }
        else
        {
            matches = new List<KeyValuePair<ReadOnlyMemory<byte>, RunningStats>>(5);
            _dict[keyHashCode] = matches;
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

    public SortedDictionary<string, RunningStats> ToFinalDictionary()
    {
        var finalDict = new SortedDictionary<string, RunningStats>(StringComparer.Ordinal);
        foreach (var kv in this)
        {
            finalDict.Add(Encoding.UTF8.GetString(kv.Key.Span), kv.Value.FinalizeStats());
        }

        return finalDict;
    }

    public void DumpDict()
    {
        Console.WriteLine(string.Join(',', _dict.Select(entry => entry?.Count ?? 0)));
    }

    public IEnumerator<KeyValuePair<ReadOnlyMemory<byte>, RunningStats>> GetEnumerator() => GetEnumerable().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private IEnumerable<KeyValuePair<ReadOnlyMemory<byte>, RunningStats>> GetEnumerable() =>
        _dict.Where(v => v is not null).SelectMany(x => x!);
}