using System.Text;

namespace ConsoleApp;

public static class Runner
{
    private const int ExpectedCityCount = 413;
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

        var finalStats = new SortedDictionary<string, RunningStats>(StringComparer.Ordinal);
        foreach (var blockTask in Interleaved(processorTasks))
        {
            var blockStats = await blockTask.Unwrap().ConfigureAwait(false);
            foreach (var (city, stats) in blockStats)
            {
                var cityAsStr = Encoding.UTF8.GetString(city.Span);
                if (!finalStats.TryGetValue(cityAsStr, out var total))
                {
                    total = new RunningStats();
                    finalStats.Add(cityAsStr, total);
                }

                total.Merge(stats);
            }
        }

        var finalBuffer = new StringBuilder(12 * 1024);
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