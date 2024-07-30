using System.Diagnostics;
using System.Text;

namespace ConsoleApp;

record TemperatureStats(float Min, float Avg, float Max)
{
    public override string ToString()
    {
        return $"{Min:F1}/{Avg:F1}/{Max:F1}";
    }
}

public class Program
{
    private static readonly int ExpectedCityCnt = 413;
    private static readonly int BufferSize = 64 * 1024 * 1024;
    private static readonly string InputFile = "./resources/measurements_1B.txt";

    // 54.8% in native code - ??
    public static void Main(string[] args)
    {
        var sw = Stopwatch.StartNew();

        var cityWithTemps = new Dictionary<string, List<float>>();
        var cityWithTempStats = new SortedDictionary<string, TemperatureStats>(StringComparer.Ordinal);

        using var reader = File.OpenText(InputFile);
        var line = reader.ReadLine(); // 86.4% of Main time

        while (line is not null)
        {
            var cityTemp = line.Split(';'); // 105 GB allocated in SOH, 3.1% of Main time
            var city = cityTemp[0];
            var temp = float.Parse(cityTemp[1]); // 3.02% of Main time

            if (!cityWithTemps.TryGetValue(city, out var temps)) // 47 GB allocated in SOH, 2.26% of Main time
            {
                temps = new List<float>(2450000); // 0.17% of Main time
                cityWithTemps.Add(city, temps); // Negligible time
            }

            temps.Add(temp); // 12.86 GB allocated in LOH

            line = reader.ReadLine();
        }

        foreach (var cityTemps in cityWithTemps)
        {
            var stats = new TemperatureStats(cityTemps.Value.Min() /* 0.47% */, cityTemps.Value.Average() /* 1.01% */, cityTemps.Value.Max() /* 0.48% */);
            cityWithTempStats.Add(cityTemps.Key, stats);
        }

        cityWithTemps = null;

        var finalBuffer = new StringBuilder(12 * 1024);
        finalBuffer.Append('{');
        finalBuffer.AppendJoin(", ",
            cityWithTempStats.Select(kv => $"{kv.Key}={kv.Value}"));
        finalBuffer.Append('}');
        Console.WriteLine(finalBuffer);

        sw.Stop();
        Console.WriteLine($"Total time: {sw.ElapsedMilliseconds}ms");
    }

    // This uses the approach described in https://devblogs.microsoft.com/pfxteam/processing-tasks-as-they-complete/
    private static Task<Task<T>>[] Interleaved<T>(Task<T>[] tasks)
    {
        var buckets = new TaskCompletionSource<Task<T>>[tasks.Length];
        var results = new Task<Task<T>>[buckets.Length];
        for (var i = 0; i < buckets.Length; i++)
        {
            buckets[i] = new TaskCompletionSource<Task<T>>();
            results[i] = buckets[i].Task;
        }

        var nextTaskIndex = -1;
        Action<Task<T>> continuation = completed =>
        {
            var bucket = buckets[Interlocked.Increment(ref nextTaskIndex)];
            bucket.TrySetResult(completed);
        };

        foreach (var inputTask in tasks)
            inputTask.ContinueWith(continuation, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);

        return results;
    }
}
