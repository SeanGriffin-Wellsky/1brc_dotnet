using System.Diagnostics;

namespace ConsoleApp;

public class Program
{
    private static readonly string FullInputFile = "./resources/measurements_1B.txt";

    public static async Task Main(string[] args)
    {
        var inputFile = args.Length > 0 ? args[0] : FullInputFile;

        var sw = Stopwatch.StartNew();

<<<<<<< HEAD
        var output = await Runner.Run(inputFile);
        Console.WriteLine(output);
=======
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
>>>>>>> main

        sw.Stop();
        Console.WriteLine($"Total time: {sw.ElapsedMilliseconds}ms");
    }
}
