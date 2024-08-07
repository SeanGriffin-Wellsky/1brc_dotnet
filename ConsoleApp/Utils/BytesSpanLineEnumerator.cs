namespace ConsoleApp.Utils;

// Taken from built-in SpanLineEnumerator but with ReadOnlySpan<byte> rather than ReadOnlySpan<char> and hard-coded
// to only handle \n as line separator, since we know that's all we'll have.
public ref struct BytesSpanLineEnumerator
{
    private ReadOnlySpan<byte> _remaining;
    private ReadOnlySpan<byte> _current;
    private bool _isEnumeratorActive;

    public BytesSpanLineEnumerator(ReadOnlySpan<byte> buffer)
    {
        _remaining = buffer;
        _current = new ReadOnlySpan<byte>();
        _isEnumeratorActive = true;
    }

    /// <summary>Gets the line at the current position of the enumerator.</summary>
    /// <returns>The line at the current position of the enumerator.</returns>
    public ReadOnlySpan<byte> Current => _current;

    /// <summary>Returns this instance as an enumerator.</summary>
    /// <returns>This instance as an enumerator.</returns>
    public BytesSpanLineEnumerator GetEnumerator() => this;

    /// <summary>Advances the enumerator to the next line of the span.</summary>
    /// <returns>
    /// <see langword="true" /> if the enumerator successfully advanced to the next line; <see langword="false" /> if the enumerator has advanced past the end of the span.</returns>
    public bool MoveNext()
    {
        if (!_isEnumeratorActive)
            return false;

        ReadOnlySpan<byte> remaining = _remaining;
        int num1 = remaining.IndexOf(Constants.NewLine);
        if ((uint) num1 < (uint) remaining.Length)
        {
            _current = remaining[..num1];
            _remaining = remaining[(num1 + 1)..];
        }
        else
        {
            _current = remaining;
            _remaining = new ReadOnlySpan<byte>();
            _isEnumeratorActive = false;
        }

        return true;
    }
}