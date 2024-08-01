using System.Diagnostics;

namespace ConsoleApp;

public static class BlockProcessor
{
    private const int ExpectedCityCnt = 413;

    public static RunningStatsDictionary ProcessBlock(object? state)
    {
        var block = (Block?) state;
        Debug.Assert(block is not null && !block.IsEmpty);

        var cityWithTempStats = new RunningStatsDictionary(ExpectedCityCnt);

        var blockChars = block.Chars;
        var lines = blockChars.Span.EnumerateLines();
        foreach (var line in lines) // 5.91% of time
        {
            if (line.IsEmpty)
                continue;

            var semicolonPos = line.IndexOf(';'); // 3.43% of time

            var city = line[..semicolonPos];
            var tempStr = line[(semicolonPos + 1)..];
            var temp = TemperatureParser.ParseTemp(tempStr); // Negligible

            var cityHashCode = SpanEqualityUtil.GetHashCode(city); // 2.44% of time

            if (!cityWithTempStats.TryGetValue(cityHashCode, city, out var temps)) // 0.02% of time
            {
                temps = new RunningStats();
                cityWithTempStats.Add(cityHashCode, city, temps); // 0.03% of time
            }

            temps.AddTemperature(temp);
        }

        block.Dispose();
        return cityWithTempStats;
    }
}