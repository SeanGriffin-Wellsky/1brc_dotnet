using System.Text;

namespace ConsoleApp;

public static class Runner
{
    private static readonly int ExpectedCityCnt = 413;
    private static readonly int BufferSize = 64 * 1024 * 1024;

    public static StringBuilder Run(string filePath)
    {
        var cityWithTempStats = new Dictionary<string, RunningStats>(ExpectedCityCnt);

        using var reader = File.OpenText(filePath);

        var blockReader = new BlockReader(reader, BufferSize);
        var block = blockReader.ReadNextBlock(); // 16.1% of Main time, 11.8% in IO
        while (!block.IsEmpty)
        {
            var blockChars = block.Chars;

            var lines = blockChars.EnumerateLines();
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
            block = blockReader.ReadNextBlock();
        }

        var finalStats = new SortedDictionary<string, RunningStats>(cityWithTempStats, StringComparer.Ordinal);
        var finalBuffer = new StringBuilder(12 * 1024);
        finalBuffer.Append('{');
        finalBuffer.AppendJoin(", ",
            finalStats.Select(kv => $"{kv.Key}={kv.Value}"));
        finalBuffer.Append('}');

        return finalBuffer;
    }
}