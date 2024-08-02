using System.Collections;
using System.ComponentModel;
using System.Numerics;
using System.Text;
using BenchmarkDotNet.Attributes;
using Bogus;

namespace Benchmark;

public class SpanHashCodeBenchmark
{
    private readonly List<ReadOnlyMemory<byte>> _sampleMemories = new(10000);
    private readonly List<string> _sampleStrings = new(10000);

    [GlobalSetup]
    public void Setup()
    {
        using var cityNameReader = File.OpenText("./resources/SampleCityNames_10K.txt");

        var line = cityNameReader.ReadLine();
        while (line is not null)
        {
            _sampleStrings.Add(line);
            _sampleMemories.Add(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(line)));
            line = cityNameReader.ReadLine();
        }
    }

    [Benchmark(Baseline = true)]
    public void BaselineString()
    {
        var dict = new Dictionary<string, int>(10000);
        foreach (var sample in _sampleStrings)
        {
            dict.TryAdd(sample, 13);
        }
    }

    // [Benchmark]
    // public void HashFirstAndLastBytePlusLength()
    // {
    //     var dict = new Dictionary<ReadOnlyMemory<byte>, int>(10000, new HashFirstAndLastBytePlusLengthComparer());
    //     foreach (var sample in _sampleMemories)
    //     {
    //         dict.TryAdd(sample, 13);
    //     }
    // }
    //
    // [Benchmark]
    // public void HashMultiByte()
    // {
    //     var dict = new Dictionary<ReadOnlyMemory<byte>, int>(10000, new HashMultiByteComparer());
    //     foreach (var sample in _sampleMemories)
    //     {
    //         dict.TryAdd(sample, 13);
    //     }
    // }
    //

    // [Benchmark]
    // public void HashFirstFourBytes()
    // {
    //     var dict = new Dictionary<ReadOnlyMemory<byte>, int>(10000, new HashFirstFourBytesComparer());
    //     foreach (var sample in _sampleMemories)
    //     {
    //         dict.TryAdd(sample, 13);
    //     }
    // }

    // [Benchmark]
    // public void HashUsingBitConverter()
    // {
    //     var dict = new Dictionary<ReadOnlyMemory<byte>, int>(10000, new HashBitConverterComparer());
    //     foreach (var sample in _sampleMemories)
    //     {
    //         dict.TryAdd(sample, 13);
    //     }
    // }
    //
    // [Benchmark]
    // public void HashUsingBitConverter2()
    // {
    //     var dict = new Dictionary<ReadOnlyMemory<byte>, int>(10000, new HashBitConverterComparer2());
    //     foreach (var sample in _sampleMemories)
    //     {
    //         dict.TryAdd(sample, 13);
    //     }
    // }

    // [Benchmark]
    // public void HashUsingBitConverter3()
    // {
    //     var dict = new Dictionary<ReadOnlyMemory<byte>, int>(10000, new HashBitConverterComparer3());
    //     foreach (var sample in _sampleMemories)
    //     {
    //         dict.TryAdd(sample, 13);
    //     }
    // }

    [Benchmark]
    public void HashUsingBitConverter4()
    {
        var dict = new Dictionary<ReadOnlyMemory<byte>, int>(10000, new HashBitConverterComparer4());
        foreach (var sample in _sampleMemories)
        {
            dict.TryAdd(sample, 13);
        }
    }

    [Benchmark]
    public void HashUsingCopilotSuggestion()
    {
        var dict = new Dictionary<ReadOnlyMemory<byte>, int>(10000, new HashBitConverterComparerCopilot());
        foreach (var sample in _sampleMemories)
        {
            dict.TryAdd(sample, 13);
        }
    }

    // [Benchmark]
    // public void HashUsingHashCodeClass()
    // {
    //     var dict = new Dictionary<ReadOnlyMemory<byte>, int>(10000, new HashCodeClassComparer());
    //     foreach (var sample in _sampleMemories)
    //     {
    //         dict.TryAdd(sample, 13);
    //     }
    // }
    //
    // [Benchmark]
    // public void RotatingHash()
    // {
    //     var dict = new Dictionary<ReadOnlyMemory<byte>, int>(10000, new RotatingHashComparer());
    //     foreach (var sample in _sampleMemories)
    //     {
    //         dict.TryAdd(sample, 13);
    //     }
    // }

    // [Benchmark]
    // public void OneAtATimeHash()
    // {
    //     var dict = new Dictionary<ReadOnlyMemory<byte>, int>(10000, new OneAtATimeHashComparer());
    //     foreach (var sample in _sampleMemories)
    //     {
    //         dict.TryAdd(sample, 13);
    //     }
    // }

    // [Benchmark]
    // public void BurtleBurtleHash()
    // {
    //     var dict = new Dictionary<ReadOnlyMemory<byte>, int>(10000, new BurtleBurtleHashComparer());
    //     foreach (var sample in _sampleMemories)
    //     {
    //         dict.TryAdd(sample, 13);
    //     }
    // }

    private class HashFirstAndLastBytePlusLengthComparer : IEqualityComparer<ReadOnlyMemory<byte>>
    {
        public bool Equals(ReadOnlyMemory<byte> x, ReadOnlyMemory<byte> y) => x.Span.SequenceEqual(y.Span);
        public int GetHashCode(ReadOnlyMemory<byte> obj)
        {
            var span = obj.Span;
            return HashCode.Combine(span[0], span[^1], span.Length);
        }
    }

    private class HashFirstFourBytesComparer : IEqualityComparer<ReadOnlyMemory<byte>>
    {
        public bool Equals(ReadOnlyMemory<byte> x, ReadOnlyMemory<byte> y) => x.Span.SequenceEqual(y.Span);
        public int GetHashCode(ReadOnlyMemory<byte> obj)
        {
            var span = obj.Span;
            return span.Length >= 4 ? BitConverter.ToInt32(span[..4]) : span.Length;
        }
    }

    private class HashMultiByteComparer : IEqualityComparer<ReadOnlyMemory<byte>>
    {
        public bool Equals(ReadOnlyMemory<byte> x, ReadOnlyMemory<byte> y) => x.Span.SequenceEqual(y.Span);
        public int GetHashCode(ReadOnlyMemory<byte> obj)
        {
            var span = obj.Span;
            return obj.Length switch
            {
                1 => span[0].GetHashCode(),
                2 => HashCode.Combine(span[0], span[1]),
                3 => HashCode.Combine(span[0], span[1], span[2]),
                4 => HashCode.Combine(span[0], span[1], span[2], span[3]),
                5 => HashCode.Combine(span[0], span[1], span[2], span[3], span[4]),
                6 => HashCode.Combine(span[0], span[1], span[2], span[3], span[4], span[5]),
                7 => HashCode.Combine(span[0], span[1], span[2], span[3], span[4], span[5], span[6]),
                _ => HashCode.Combine(span[0], span[1], span[2], span[3], span[4], span[5], span[6], span[7])
            };
        }
    }

    private class HashBitConverterComparer : IEqualityComparer<ReadOnlyMemory<byte>>
    {
        public bool Equals(ReadOnlyMemory<byte> x, ReadOnlyMemory<byte> y) => x.Span.SequenceEqual(y.Span);
        public int GetHashCode(ReadOnlyMemory<byte> obj)
        {
            var span = obj.Span;
            var div = span.Length / 4;

            var hash = 0;
            for (var i = 0; i < div; ++i)
            {
                hash ^= BitConverter.ToInt32(span[..4]);
                span = span[4..];
            }

            switch (span.Length)
            {
                case 3:
                {
                    hash ^= BitConverter.ToInt16(span[..2]);
                    hash ^= span[2];
                    break;
                }
                case 2:
                    hash ^= BitConverter.ToInt16(span[..2]);
                    break;
                case 1:
                    hash ^= span[0];
                    break;
            }

            return hash;
        }
    }

    private class HashBitConverterComparer2 : IEqualityComparer<ReadOnlyMemory<byte>>
    {
        public bool Equals(ReadOnlyMemory<byte> x, ReadOnlyMemory<byte> y) => x.Span.SequenceEqual(y.Span);
        public int GetHashCode(ReadOnlyMemory<byte> obj)
        {
            var span = obj.Span;
            var div = span.Length / 4;

            var hash = 0;
            for (var i = 0; i < div; ++i)
            {
                hash ^= BitConverter.ToInt32(span.Slice(i * 4, 4));
            }

            return HashCode.Combine(hash, span.Length);
        }
    }

    private class HashBitConverterComparer3 : IEqualityComparer<ReadOnlyMemory<byte>>
    {
        public bool Equals(ReadOnlyMemory<byte> x, ReadOnlyMemory<byte> y) => x.Span.SequenceEqual(y.Span);
        public int GetHashCode(ReadOnlyMemory<byte> obj)
        {
            var span = obj.Span;
            var div = span.Length / 4;

            var hash = 0;
            for (var i = 0; i < div; ++i)
            {
                hash ^= BitConverter.ToInt32(span.Slice(i * 4, 4));
            }

            return hash ^ span.Length;
        }
    }

    private class HashBitConverterComparer4 : IEqualityComparer<ReadOnlyMemory<byte>>
    {
        public bool Equals(ReadOnlyMemory<byte> x, ReadOnlyMemory<byte> y) => x.Span.SequenceEqual(y.Span);
        public int GetHashCode(ReadOnlyMemory<byte> obj)
        {
            var span = obj.Span;
            var div = span.Length / 4;

            var hash = 31;
            for (var i = 0; i < div; ++i)
            {
                hash ^= BitConverter.ToInt32(span.Slice(i * 4, 4));
            }

            return hash ^ span.Length;
        }
    }

    private class HashBitConverterComparerCopilot : IEqualityComparer<ReadOnlyMemory<byte>>
    {
        public bool Equals(ReadOnlyMemory<byte> x, ReadOnlyMemory<byte> y) => x.Span.SequenceEqual(y.Span);
        public int GetHashCode(ReadOnlyMemory<byte> obj)
        {
            var span = obj.Span;
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


    private class HashCodeClassComparer : IEqualityComparer<ReadOnlyMemory<byte>>
    {
        public bool Equals(ReadOnlyMemory<byte> x, ReadOnlyMemory<byte> y) => x.Span.SequenceEqual(y.Span);
        public int GetHashCode(ReadOnlyMemory<byte> obj)
        {
            var hashCode = new HashCode();
            hashCode.AddBytes(obj.Span);
            return hashCode.ToHashCode();
        }
    }

    private class RotatingHashComparer : IEqualityComparer<ReadOnlyMemory<byte>>
    {
        public bool Equals(ReadOnlyMemory<byte> x, ReadOnlyMemory<byte> y) => x.Span.SequenceEqual(y.Span);
        public int GetHashCode(ReadOnlyMemory<byte> obj)
        {
            const int prime = 13;

            var span = obj.Span;
            var len = span.Length;

            int hash;
            int i;
            for (hash = len, i = 0; i < len; ++i)
            {
                hash = (hash << 4) ^ (hash >> 28) ^ span[i];
            }

            return hash % prime;
        }
    }

    private class OneAtATimeHashComparer : IEqualityComparer<ReadOnlyMemory<byte>>
    {
        public bool Equals(ReadOnlyMemory<byte> x, ReadOnlyMemory<byte> y) => x.Span.SequenceEqual(y.Span);
        public int GetHashCode(ReadOnlyMemory<byte> obj)
        {
            var span = obj.Span;
            var len = span.Length;

            int hash;
            int i;
            for (hash = 0, i = 0; i < len; ++i)
            {
                hash += span[i];
                hash += hash << 10;
                hash ^= hash >> 6;
            }

            hash += hash << 3;
            hash ^= hash >> 11;
            hash += hash << 15;

            return hash;
        }
    }

    public class BurtleBurtleHashComparer : IEqualityComparer<ReadOnlyMemory<byte>>
    {
        public bool Equals(ReadOnlyMemory<byte> x, ReadOnlyMemory<byte> y) => x.Span.SequenceEqual(y.Span);
        public int GetHashCode(ReadOnlyMemory<byte> obj)
        {
            var span = obj.Span;
            uint length = (uint) obj.Length;
            uint initval = 0x32193759; // Arbitrary

            uint a, b, c, len;

            len = length;
            a = b = 0x9e3779b9; // Arbitrary
            c = initval;

            while (len >= 12)
            {
                a += span[0] + ((uint)span[1] << 8) + ((uint)span[2] << 16) + ((uint)span[3] << 24);
                b += span[4] + ((uint)span[5] << 8) + ((uint)span[6] << 16) + ((uint)span[7] << 24);
                c += span[8] + ((uint)span[9] << 8) + ((uint)span[10] << 16) + ((uint)span[11] << 24);
                //Mix(ref a, ref b, ref c);

                span = span[12..];
                len -= 12;
            }

            c += length;
            switch (len)
            {
                case 11: c += (uint)span[10] << 24;
                    goto case 10;
                case 10: c += (uint)span[9] << 16;
                    goto case 9;
                case 9: c += (uint)span[8] << 8;
                    // first byte of c is reserved for the length
                    goto case 8;
                case 8: b += (uint)span[7] << 24;
                    goto case 7;
                case 7: b += (uint)span[6] << 16;
                    goto case 6;
                case 6: b += (uint)span[5] << 8;
                    goto case 5;
                case 5: b += span[4];
                    goto case 4;
                case 4: a += (uint)span[3] << 24;
                    goto case 3;
                case 3: a += (uint)span[2] << 16;
                    goto case 2;
                case 2: a += (uint)span[1] << 8;
                    goto case 1;
                case 1: a += span[0];
                    break;
            }

            //Mix(ref a, ref b, ref c);
            return (int) c;
        }

        private static void Mix(ref uint a, ref uint b, ref uint c)
        {
            a -= b;
            a -= c;
            a ^= c >> 13;
            b -= c;
            b -= a;
            b ^= a << 8;
            c -= a;
            c -= b;
            c ^= b >> 13;
            a -= b;
            a -= c;
            a ^= c >> 12;
            b -= c;
            b -= a;
            b ^= a << 16;
            c -= a;
            c -= b;
            c ^= b >> 5;
            a -= b;
            a -= c;
            a ^= c >> 3;
            b -= c;
            b -= a;
            b ^= a << 10;
            c -= a;
            c -= b;
            c ^= b >> 15;
        }
    }
}