using System.Linq.Expressions;
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

        using var reader = File.OpenRead(filePath);

        var blockReader = new BlockReader(reader, BufferSize);
        var block = blockReader.ReadNextBlock(); // 5.76% of Main time, 2.28% in IO
        while (!block.IsEmpty)
        {
            var blockBytes = block.Bytes;

            var startOfNewLine = 0;
            for (var i = 0; i < blockBytes.Length; ++i)
            {
                if (blockBytes[i] == Constants.NewLine)
                {
                    var line = blockBytes.Slice(startOfNewLine, i - startOfNewLine);
                    if (line.IsEmpty)
                        continue;

                    var semicolonPos = line.IndexOf(Constants.Semicolon);

                    // 8.69% of Main time in these Encoding.UTF8.GetString calls
                    var city = Encoding.UTF8.GetString(line[..semicolonPos]);
                    var tempStr = Encoding.UTF8.GetString(line[(semicolonPos + 1)..]);
                    var temp = float.Parse(tempStr); // 24.8% of Main time

                    if (!cityWithTemps.TryGetValue(city, out var temps)) // 19.7% of Main time
                    {
                        temps = new List<float>(2450000);
                        cityWithTemps.Add(city, temps);
                    }

                    temps.Add(temp);
                    startOfNewLine = i + 1;
                }
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