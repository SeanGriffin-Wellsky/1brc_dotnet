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
        var lines = new BytesSpanLineEnumerator(blockChars.Span);
        foreach (var line in lines) // 2.09% of time
        {
            if (line.IsEmpty)
                continue;

            var semicolonPos = line.IndexOf(Constants.Semicolon);

            var city = line[..semicolonPos];
            var tempStr = line[(semicolonPos + 1)..];
            var temp = TemperatureParser.ParseTemp(tempStr);

            var cityHashCode = cityWithTempStats.GetHashCode(city); // 1.69% of time

            if (!cityWithTempStats.TryGetValue(cityHashCode, city, out var temps))
            {
                temps = new RunningStats();
                cityWithTempStats.Add(cityHashCode, city, temps); // Negligible
            }

            temps.AddTemperature(temp);
        }

        block.Dispose();
        return cityWithTempStats;
    }
}