using System.Diagnostics;

namespace ConsoleApp;

public class Program
{
    private static readonly string WarmupInputFile = "./resources/measurements_5M.txt";
    private static readonly string FullInputFile = "./resources/measurements_1B.txt";

    public static async Task Main(string[] args)
    {
        var inputFile = args.Length > 0 && args[0] == "warmup" ? WarmupInputFile : FullInputFile;

        var sw = Stopwatch.StartNew();

        var output = await Runner.Run(inputFile);
        Console.WriteLine(output);

        sw.Stop();
        Console.WriteLine($"Total time: {sw.ElapsedMilliseconds}ms");
    }
}
