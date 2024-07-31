using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices.JavaScript;

namespace ConsoleApp;

public readonly ref struct Block
{
    public Block()
    {
        Length = 0;
        Bytes = ReadOnlySpan<byte>.Empty;
    }

    public Block(byte[] initialBuffer, byte[] supplementalBuffer)
    {
        Debug.Assert(initialBuffer.Length > 0);

        Length = initialBuffer.Length + supplementalBuffer.Length;

        var totalBlock = new byte[Length];

        initialBuffer.CopyTo(totalBlock, 0);
        supplementalBuffer.CopyTo(totalBlock, initialBuffer.Length);

        Bytes = totalBlock.AsSpan();
    }

    public bool IsEmpty => Length == 0;

    public ReadOnlySpan<byte> Bytes { get; }

    public int Length { get; }
}

public sealed class BlockReader(Stream reader, int bufferSize)
{
    private readonly byte[] _buffer = new byte[bufferSize];
    private readonly byte[] _supplementalBuffer = new byte[105];

    public Block ReadNextBlock()
    {
        var numRead = reader.Read(_buffer);
        if (numRead == 0)
        {
            return new Block();
        }

        if (numRead < bufferSize)
        {
            return new Block(_buffer[..numRead], Array.Empty<byte>());
        }

        var supplementalBufferPos = -1;
        var nextByte = reader.ReadByte();

        while (nextByte != -1)
        {
            _supplementalBuffer[++supplementalBufferPos] = (byte) nextByte;

            if (nextByte == '\n')
                break;

            nextByte = reader.ReadByte();
        }

        return new Block(_buffer, _supplementalBuffer[..(supplementalBufferPos+1)]);
    }
}