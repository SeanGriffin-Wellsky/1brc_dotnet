using System.Text;

namespace ConsoleApp;

public static class Runner
{
    private const int ExpectedCityCount = 10000;
    private const int BufferSize = 64 * 1024 * 1024;

    public static async Task<StringBuilder> Run(string filePath)
    {
        await using var reader = File.OpenRead(filePath);
        using var fileHandle = reader.SafeFileHandle;

        var partitions = FilePartitioner.PartitionFile(reader, BufferSize);
        var processorTasks = new Task<RunningStatsDictionary>[partitions.Count];

        for (var i = 0; i < partitions.Count; i++)
        {
            var state = new ProcessingState(ExpectedCityCount, fileHandle, partitions[i]);
            processorTasks[i] = Task.Factory.StartNew(BlockProcessor.ProcessBlock, state);
        }

        var totalStats = new RunningStatsDictionary(ExpectedCityCount);
        foreach (var blockTask in Interleaved(processorTasks))
        {
            var blockStats = await blockTask.Unwrap().ConfigureAwait(false);

            foreach (var (city, stats) in blockStats)
            {
                var cityHashCode = SpanEqualityUtil.GetHashCode(city.Span);
                if (!totalStats.TryGetValue(cityHashCode, city.Span, out var runningStats))
                {
                    runningStats = new RunningStats();
                    totalStats.Add(cityHashCode, city.Span, runningStats);
                }

                runningStats.Merge(stats);
            }
        }

        Console.WriteLine($"c({string.Join(',', totalStats.DumpCountsPerBucket())})");
        // var upperA = 65;
        // var j = 0;
        // foreach (var bucketCnts in totalStats.DumpCountsPerBucket())
        // {
        //     Console.WriteLine($"{(char) (upperA + j)} <- c({bucketCnts})");
        //     j++;
        // }

        var finalStats = new SortedDictionary<string, RunningStats>(StringComparer.Ordinal);
        foreach (var kv in totalStats)
        {
            finalStats.Add(Encoding.UTF8.GetString(kv.Key.Span), kv.Value);
        }

        var finalBuffer = new StringBuilder(12 * 1024 * 25);
        finalBuffer.Append('{');
        finalBuffer.AppendJoin(", ",
            finalStats.Select(kv => $"{kv.Key}={kv.Value}"));
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