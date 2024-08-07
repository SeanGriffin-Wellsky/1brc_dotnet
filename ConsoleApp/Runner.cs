using System.Text;

namespace ConsoleApp;

public static class Runner
{
    private static readonly int BufferSize = 64 * 1024 * 1024;

    public static StringBuilder Run(string filePath)
    {
        var cityWithTempStats = new SortedDictionary<string, RunningStats>(StringComparer.Ordinal);

        using var reader = File.OpenText(filePath);

        var blockReader = new BlockReader(reader, BufferSize);
        var block = blockReader.ReadNextBlock(); // 10.1% of Main time, 4.5% in IO
        while (!block.IsEmpty)
        {
            var blockChars = block.Chars;

            var lines = blockChars.EnumerateLines();
            foreach (var line in lines) // 6.09% of Main time
            {
                if (line.IsEmpty)
                    continue;

                var semicolonPos = line.IndexOf(';'); // 3.67% of Main time

                var city = line[..semicolonPos].ToString(); // 2.9% of Main time
                var tempStr = line[(semicolonPos + 1)..];
                var temp = float.Parse(tempStr); // 24.1% of Main time

                if (!cityWithTempStats.TryGetValue(city, out var temps)) // 42.5% of Main time
                {
                    temps = new RunningStats();
                    cityWithTempStats.Add(city, temps);
                }

                temps.AddTemperature(temp);
            }

            block = blockReader.ReadNextBlock();
        }

        var finalBuffer = new StringBuilder(12 * 1024);
        finalBuffer.Append('{');
        finalBuffer.AppendJoin(", ",
            cityWithTempStats.Select(kv => $"{kv.Key}={kv.Value}"));
        finalBuffer.Append('}');

        return finalBuffer;
    }
}