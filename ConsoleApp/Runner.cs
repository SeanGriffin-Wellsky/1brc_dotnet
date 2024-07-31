using System.Text;

namespace ConsoleApp;

public static class Runner
{
    private static readonly int ExpectedCityCnt = 413;
    private static readonly int BufferSize = 64 * 1024 * 1024;

    public static StringBuilder Run(string filePath)
    {
        var cityWithTemps = new Dictionary<string, List<float>>();
        var cityWithTempStats = new SortedDictionary<string, TemperatureStats>(StringComparer.Ordinal);

        using var reader = File.OpenText(filePath);

        var blockReader = new BlockReader(reader, BufferSize);
        var block = blockReader.ReadNextBlock(); // 15.3% of Main time (4.4% was in IO); 51 GB in LOH
        while (!block.IsEmpty)
        {
            var lines = block.Chars.AsSpan().EnumerateLines();
            foreach (var line in lines)
            {
                if (line.IsEmpty)
                    continue;

                var cityTemp = line.ToString().Split(';'); // 17.4% of Main time; 152 GB in SOH
                var city = cityTemp[0];
                var temp = float.Parse(cityTemp[1]); // 23.2% of Main time

                if (!cityWithTemps.TryGetValue(city, out var temps)) // 16.1% of Main time; 47 GB allocated in SOH
                {
                    temps = new List<float>(2450000); // 0.04% of Main time; 3.8 GB in SOH
                    cityWithTemps.Add(city, temps); // Negligible time
                }

                temps.Add(temp);
            }

            block = blockReader.ReadNextBlock();
        }

        foreach (var cityTemps in cityWithTemps)
        {
            var stats = new TemperatureStats(cityTemps.Value.Min(), cityTemps.Value.Average(), cityTemps.Value.Max());
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