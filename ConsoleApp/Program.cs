using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;

namespace ConsoleApp;

record TemperatureStats(float Min, float Avg, float Max)
{
    public override string ToString()
    {
        return $"{Min:F1}/{Avg:F1}/{Max:F1}";
    }
}

public class Program
{
    private static readonly string InputFile = "./resources/measurements_1B.txt";

    // 63.2% in native code - ??
    public static void Main(string[] args)
    {
        var sw = Stopwatch.StartNew();

        var output = Runner.Run(InputFile);
        Console.WriteLine(output);

        sw.Stop();
        Console.WriteLine($"Total time: {sw.ElapsedMilliseconds}ms");
    }
}
