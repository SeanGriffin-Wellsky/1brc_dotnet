using System.Text;

namespace ConsoleApp;

readonly record struct TemperatureStats(float Min, float Avg, float Max)
{
    public override string ToString()
    {
        return $"{Min:F1}/{Avg:F1}/{Max:F1}";
    }
}

public static class Runner
{
    public static StringBuilder Run(string filePath)
    {
        var cityWithTemps = new Dictionary<string, List<float>>();
        var cityWithTempStats = new SortedDictionary<string, TemperatureStats>(StringComparer.Ordinal);

        using var reader = File.OpenText(filePath);
        var line = reader.ReadLine(); // 87.9% of Main time

        while (line is not null)
        {
            var cityTemp = line.Split(';'); // 105 GB allocated in SOH, 2.07% of Main time
            var city = cityTemp[0];
            var temp = float.Parse(cityTemp[1]); // 2% of Main time

            if (!cityWithTemps.TryGetValue(city, out var temps)) // 47 GB allocated in SOH, 1.54% of Main time
            {
                temps = new List<float>();
                cityWithTemps.Add(city, temps); // 1.14% of Main time
            }

            temps.Add(temp); // 12.86 GB allocated in LOH

            line = reader.ReadLine();
        }

        foreach (var cityTemps in cityWithTemps)
        {
            var stats = new TemperatureStats(cityTemps.Value.Min() /* 0.47% */, cityTemps.Value.Average() /* 1.3% */, cityTemps.Value.Max() /* 0.37% */);
            cityWithTempStats.Add(cityTemps.Key, stats);
        }

        cityWithTemps = null;

        var finalBuffer = new StringBuilder(12 * 1024);
        finalBuffer.Append('{');
        finalBuffer.AppendJoin(", ",
            cityWithTempStats.Select(kv => $"{kv.Key}={kv.Value}"));
        finalBuffer.Append('}');

        return finalBuffer;
    }
}