using System.Text;

namespace ConsoleApp;

public static class Runner
{
    private static readonly int BufferSize = 64 * 1024 * 1024;

    public static async Task<StringBuilder> Run(string filePath)
    {
        await using var reader = File.OpenRead(filePath);
        var blockReader = new BlockReader(reader, BufferSize);

        var processorTasks = new List<Task<RunningStatsDictionary>>(205);

        var block = blockReader.ReadNextBlock(); // 7.33%, 2.7% in IO
        while (!block.IsEmpty)
        {
            processorTasks.Add(Task.Factory.StartNew(BlockProcessor.ProcessBlock, block));
            block = blockReader.ReadNextBlock();
        }

        var perBlockStats = await Task.WhenAll(processorTasks);

        var finalStats = new SortedDictionary<string, RunningStats>(StringComparer.Ordinal);
        foreach (var blockStats in perBlockStats)
        {
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
}