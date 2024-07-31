using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices.JavaScript;

namespace ConsoleApp;

public readonly struct Block
{
    public Block()
    {
        Length = 0;
        Chars = ImmutableArray<char>.Empty;
    }

    public Block(char[] initialBuffer, char[] supplementalBuffer)
    {
        Debug.Assert(initialBuffer.Length > 0);

        Length = initialBuffer.Length + supplementalBuffer.Length;

        var totalBlock = new char[Length];

        initialBuffer.CopyTo(totalBlock, 0);
        supplementalBuffer.CopyTo(totalBlock, initialBuffer.Length);

        Chars = totalBlock.ToImmutableArray();
    }

    public bool IsEmpty => Length == 0;

    public ImmutableArray<char> Chars { get; }

    public int Length { get; }
}

public sealed class BlockReader(TextReader reader, int bufferSize)
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
            return new Block(_buffer[..numRead], Array.Empty<char>());
        }

        var supplementalBufferPos = -1;
        var nextChar = reader.Read();

        while (nextChar != -1)
        {
            _supplementalBuffer[++supplementalBufferPos] = (char) nextChar;

            if (nextChar == '\n')
                break;

            nextChar = reader.Read();
        }

        return new Block(_buffer, _supplementalBuffer[..(supplementalBufferPos+1)]);
    }
}