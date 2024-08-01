using System.Diagnostics;

namespace ConsoleApp;

public static class BlockProcessor
{
    private const int ExpectedCityCnt = 413;

    public static Dictionary<string, RunningStats> ProcessBlock(object? state)
    {
        var block = (Block?) state;
        Debug.Assert(block is not null && !block.IsEmpty);

        var cityWithTempStats = new Dictionary<string, RunningStats>(ExpectedCityCnt);

        var blockChars = block.Chars;
        var lines = blockChars.Span.EnumerateLines();
        foreach (var line in lines) // 12.2% of Main time
        {
            if (line.IsEmpty)
                continue;

            var semicolonPos = line.IndexOf(';'); // 10.4% of Main time

            var city = line[..semicolonPos].ToString(); // 6.63% of Main time
            var tempStr = line[(semicolonPos + 1)..];
            var temp = TemperatureParser.ParseTemp(tempStr); // Negligible

            if (!cityWithTempStats.TryGetValue(city, out var temps)) // 35.8% of Main time
            {
                temps = new RunningStats();
                cityWithTempStats.Add(city, temps);
            }

            temps.AddTemperature(temp);
        }

        block.Dispose();
        return cityWithTempStats;
    }
}