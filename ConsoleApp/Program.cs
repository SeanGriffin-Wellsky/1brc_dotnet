using System.Diagnostics;
using System.Text;

namespace ConsoleApp;

public class Program
{
    private static readonly int ExpectedCityCnt = 413;
    private static readonly int BufferSize = 64 * 1024 * 1024;
    private static readonly string InputFile = "./resources/measurements_1B.txt";

    public static async Task Main(string[] args)
    {
        var sw = Stopwatch.StartNew();

        await using var reader = File.OpenRead(InputFile);
        using var fileHandle = reader.SafeFileHandle;

        var totalCalc = new CityTemperatureStatCalc(ExpectedCityCnt);
        var partitions = FilePartitioner.PartitionFile(reader, BufferSize);
        var processorTasks = new Task<CityTemperatureStatCalc>[partitions.Count];

        for (var i = 0; i < partitions.Count; i++)
        {
            var state = new ProcessingState(ExpectedCityCnt, fileHandle, partitions[i]);
            processorTasks[i] = Task<CityTemperatureStatCalc>.Factory.StartNew(
                PartitionProcessor.ProcessPartition, state);
        }

        foreach (var calcTask in Interleaved(processorTasks))
        {
            totalCalc.Merge(await calcTask.Unwrap().ConfigureAwait(false));
        }

        var finalBuffer = new StringBuilder(12 * 1024);
        finalBuffer.Append('{');
        finalBuffer.AppendJoin(", ",
            totalCalc.FinalizeStats().Select(kv =>
                $"{kv.Key}={kv.Value.Min:F1}/{kv.Value.TemperatureAvg:F1}/{kv.Value.Max:F1}"));
        finalBuffer.Append('}');
        Console.WriteLine(finalBuffer);

        sw.Stop();
        Console.WriteLine($"Num blocks: {partitions.Count}");
        Console.WriteLine($"Num threads in pool: {ThreadPool.ThreadCount}");
        Console.WriteLine($"Num cities: {totalCalc.NumCities}");
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
