using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;

namespace ConsoleApp;

public class Program
{
    private static readonly string InputFile = "./resources/measurements_1B.txt";

    // 64.4% in native code - ??
    public static void Main(string[] args)
    {
        var sw = Stopwatch.StartNew();

        var output = Runner.Run(InputFile);
        Console.WriteLine(output);

        sw.Stop();
        Console.WriteLine($"Total time: {sw.ElapsedMilliseconds}ms");
    }
}
