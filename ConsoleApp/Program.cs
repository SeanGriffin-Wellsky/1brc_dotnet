using System.Diagnostics;

namespace ConsoleApp;

public class Program
{
    private static readonly string InputFile = "./resources/measurements_1B.txt";

    // 21.6% in native code - ??
    public static async Task Main(string[] args)
    {
        var sw = Stopwatch.StartNew();

        var output = await Runner.Run(InputFile);
        Console.WriteLine(output);

        sw.Stop();
        Console.WriteLine($"Total time: {sw.ElapsedMilliseconds}ms");
    }
}
