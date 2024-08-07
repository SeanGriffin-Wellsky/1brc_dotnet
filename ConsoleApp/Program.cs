using System.Diagnostics;

namespace ConsoleApp;

public class Program
{
    private static readonly string FullInputFile = "./resources/measurements_1B.txt";

    // 54.8% in native code - ??
    public static void Main(string[] args)
    {
        var inputFile = args.Length > 0 ? args[0] : FullInputFile;

        var sw = Stopwatch.StartNew();

        var output = Runner.Run(inputFile);
        Console.WriteLine(output);

        sw.Stop();
        Console.WriteLine($"Total time: {sw.ElapsedMilliseconds}ms");
    }
}
