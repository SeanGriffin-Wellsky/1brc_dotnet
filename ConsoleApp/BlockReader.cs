using System.Buffers;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices.JavaScript;

namespace ConsoleApp;

public readonly ref struct Block
{
    private readonly char[]? _rentedArray;

    public Block()
    {
        Length = 0;
        Chars = ReadOnlySpan<char>.Empty;
        _rentedArray = null;
    }

    public Block(ReadOnlySpan<char> initialBuffer, ReadOnlySpan<char> supplementalBuffer)
    {
        Debug.Assert(initialBuffer.Length > 0);

        Length = initialBuffer.Length + supplementalBuffer.Length;

        _rentedArray = ArrayPool<char>.Shared.Rent(Length);

        initialBuffer.CopyTo(_rentedArray);
        supplementalBuffer.CopyTo(_rentedArray.AsSpan().Slice(initialBuffer.Length, supplementalBuffer.Length));

        Chars = _rentedArray.AsSpan(0, Length);
    }

    public bool IsEmpty => Length == 0;

    public ReadOnlySpan<char> Chars { get; }

    public int Length { get; }

    public void Dispose()
    {
        if (_rentedArray != null)
        {
            ArrayPool<char>.Shared.Return(_rentedArray);
        }
    }
}

public sealed class BlockReader(StreamReader reader, int bufferSize)
{
    private readonly char[] _buffer = new char[bufferSize];
    private readonly char[] _supplementalBuffer = new char[105];

    public Block ReadNextBlock()
    {
        var numRead = reader.Read(_buffer);
        if (numRead == 0)
        {
            return new Block();
        }

        if (numRead < bufferSize)
        {
            return new Block(_buffer.AsSpan()[..numRead], Array.Empty<char>());
        }

        var supplementalBufferPos = -1;
        var nextByte = reader.Read();

        while (nextByte != -1)
        {
            _supplementalBuffer[++supplementalBufferPos] = (char) nextByte;

            if (nextByte == '\n')
                break;

            nextByte = reader.Read();
        }

        return new Block(_buffer, _supplementalBuffer.AsSpan()[..(supplementalBufferPos+1)]);
    }
}