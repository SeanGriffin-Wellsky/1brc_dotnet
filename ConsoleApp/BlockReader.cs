using System.Buffers;
using System.Diagnostics;

namespace ConsoleApp;

public sealed class Block
{
    private readonly byte[]? _rentedArray;

    public Block()
    {
        Length = 0;
        Chars = ReadOnlyMemory<byte>.Empty;
        _rentedArray = null;
    }

    public Block(ReadOnlySpan<byte> initialBuffer, ReadOnlySpan<byte> supplementalBuffer)
    {
        Debug.Assert(initialBuffer.Length > 0);

        Length = initialBuffer.Length + supplementalBuffer.Length;

        _rentedArray = ArrayPool<byte>.Shared.Rent(Length);

        initialBuffer.CopyTo(_rentedArray);
        supplementalBuffer.CopyTo(_rentedArray.AsSpan().Slice(initialBuffer.Length, supplementalBuffer.Length));

        Chars = _rentedArray.AsMemory(0, Length);
    }

    public bool IsEmpty => Length == 0;

    public ReadOnlyMemory<byte> Chars { get; private set; }

    public int Length { get; private set; }

    public void Dispose()
    {
        if (_rentedArray != null)
        {
            ArrayPool<byte>.Shared.Return(_rentedArray);
        }

        Length = 0;
        Chars = null;
    }
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
            return new Block(_buffer.AsSpan()[..numRead], Array.Empty<byte>());
        }

        var supplementalBufferPos = -1;
        var nextByte = reader.ReadByte();

        while (nextByte != -1)
        {
            _supplementalBuffer[++supplementalBufferPos] = (byte) nextByte;

            if (nextByte == Constants.NewLine)
                break;

            nextByte = reader.ReadByte();
        }

        return new Block(_buffer, _supplementalBuffer.AsSpan()[..(supplementalBufferPos+1)]);
    }
}