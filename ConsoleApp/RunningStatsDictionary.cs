using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ConsoleApp;

public sealed class RunningStatsDictionary(int capacity) : IEnumerable<KeyValuePair<ReadOnlyMemory<byte>, RunningStats>>
{
    // A list of prime numbers to use as the size of the hash table. Stolen from System.Collections.HashHelpers within the runtime.
    private static ReadOnlySpan<int> Primes =>
    [
        431, 521, 631, 761, 919, 1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861,
        5839, 7013, 8419, 10103, 12143, 14591, 17519, 21023, 25229, 30293, 36353, 43627
    ];

    private static int GetPrime(int min)
    {
        foreach (var prime in Primes)
        {
            if (prime >= min)
            {
                return prime;
            }
        }
        return min;
    }

    // Increase the size of the hash table to reduce chance of clashes.
    private readonly List<KeyValuePair<ReadOnlyMemory<byte>, RunningStats>>?[] _dict =
        new List<KeyValuePair<ReadOnlyMemory<byte>, RunningStats>>?[GetPrime(capacity * 4)];

    public int Count => GetEnumerable().Count();

    // Always use this to get the value of the hash code for the key passed in TryGetValue or Add!
    public int GetKeyHashCode(ReadOnlySpan<byte> key) =>
        (SpanEqualityUtil.GetHashCode(key) & int.MaxValue) % _dict.Length;

    public bool TryGetValue(int keyHashCode, ReadOnlySpan<byte> key, [MaybeNullWhen(false)] out RunningStats stats)
    {
        Debug.Assert(keyHashCode == GetKeyHashCode(key));

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
        Debug.Assert(keyHashCode == GetKeyHashCode(key));

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

    public List<int> GetDumpCountsPerBucket() => _dict.Select(entry => entry?.Count ?? 0).ToList();

    public IEnumerator<KeyValuePair<ReadOnlyMemory<byte>, RunningStats>> GetEnumerator() => GetEnumerable().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private IEnumerable<KeyValuePair<ReadOnlyMemory<byte>, RunningStats>> GetEnumerable() =>
        _dict.Where(v => v is not null).SelectMany(x => x!);
}