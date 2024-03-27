using System.Globalization;
using System.Text;

namespace ConsoleApp;

public readonly ref struct CityTemp(ReadOnlyMemory<byte> city, ReadOnlyMemory<byte> temp)
{
    public ReadOnlyMemory<byte> City { get; } = city;

    public int Temperature { get; } = ParseTemp(temp.Span);

    public override string ToString() => $"{Encoding.UTF8.GetString(City.Span)};{Temperature / 10.0f}";

    private static int ParseTemp(ReadOnlySpan<byte> temp)
    {
        // The char encoding starts char '0' at code 48
        const int zeroCode = 48;
        
        // Code 45 is a -
        const int neg = 45;
        
        var i = temp.Length - 1;

        var tenths = temp[i] - zeroCode;
        i -= 2; // skip over decimal
        var ones = temp[i--] - zeroCode;

        var tens = 0;
        
        // If i is negative then it means we're done parsing, else parse tens position (or negative sign)
        if (i >= 0)
            tens = temp[i] - zeroCode;

        // It's a negative number if we either just parsed a -
        // or we have one more character remaining, which must be a -
        // because we'll never have a number > 99 or < 99
        var isNeg = tens == (neg - zeroCode) | i == 1;

        // Neg is a lower code than 0, so if tens is > 0 it means it's an actual number
        var asInt = (tens > 0 ? tens * 100 : 0) + (ones * 10) + tenths;
        return isNeg ? asInt * -1 : asInt;
    }
}
