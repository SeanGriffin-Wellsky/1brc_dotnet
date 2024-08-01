using System.Diagnostics;

namespace ConsoleApp;

public class Program
{
    private static readonly string InputFile = "./resources/measurements_100M.txt";

    public static async Task Main(string[] args)
    {
        var sw = Stopwatch.StartNew();

        var output = await Runner.Run(InputFile);
        Console.WriteLine(output);

        sw.Stop();
        Console.WriteLine($"Total time: {sw.ElapsedMilliseconds}ms");
    }
}
