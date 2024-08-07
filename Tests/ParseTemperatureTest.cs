using ConsoleApp;

namespace Tests;

public class ParseTemperatureTest
{
    [Theory]
    [InlineData("-20.9", -209)]
    [InlineData("-2.9", -29)]
    [InlineData("20.9", 209)]
    [InlineData("2.9", 29)]
    [InlineData("0.0", 0)]
    [InlineData("-0.0", 0)]
    public void TestCustomFloatParsing(string numAsStr, int expected)
    {
        var temp = TemperatureParser.ParseTemp(numAsStr);
        Assert.Equal(expected, temp);
    }
}