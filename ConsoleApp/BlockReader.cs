using System.Diagnostics;

namespace ConsoleApp;

public readonly ref struct Block
{
    public Block()
    {
        Length = 0;
        Chars = ReadOnlySpan<char>.Empty;
    }

    public Block(char[] initialBuffer, char[] supplementalBuffer)
    {
        Debug.Assert(initialBuffer.Length > 0);

        Length = initialBuffer.Length + supplementalBuffer.Length;

        var totalBlock = new char[Length];

        initialBuffer.CopyTo(totalBlock, 0);
        supplementalBuffer.CopyTo(totalBlock, initialBuffer.Length);

        Chars = totalBlock.AsSpan();
    }

    public bool IsEmpty => Length == 0;

    public ReadOnlySpan<char> Chars { get; }

    public int Length { get; }
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
            return new Block(_buffer[..numRead], Array.Empty<char>());
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

        return new Block(_buffer, _supplementalBuffer[..(supplementalBufferPos+1)]);
    }
}