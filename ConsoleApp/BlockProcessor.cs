using System.Diagnostics;
using System.Resources;
using System.Runtime.InteropServices;

namespace ConsoleApp;

public static class BlockProcessor
{
    public static unsafe RunningStatsDictionary ProcessBlock(object? blockState)
    {
        var state = (ProcessingState) blockState!;

        var cityWithTempStats = new RunningStatsDictionary(state.ExpectedCityCount);
        var block = state.Block;

        var buffer = Marshal.AllocHGlobal(block.Length);
        var bufferPtr = (byte*)buffer.ToPointer();
        var remainingBlockBytes = new Span<byte>(bufferPtr, block.Length);

        var numRead = RandomAccess.Read(state.FileHandle, remainingBlockBytes, block.Pos);

        Debug.Assert(numRead == block.Length);

        var lines = new BytesSpanLineEnumerator(remainingBlockBytes);
        foreach (var line in lines)
        {
            if (line.IsEmpty)
                continue;

            var semicolonPos = line.IndexOf(Constants.Semicolon);

            var city = line[..semicolonPos];
            var tempStr = line[(semicolonPos + 1)..];
            var temp = TemperatureParser.ParseTemp(tempStr);

            var cityHashCode = cityWithTempStats.GetHashCode(city);

            if (!cityWithTempStats.TryGetValue(cityHashCode, city, out var temps))
            {
                temps = new RunningStats();
                cityWithTempStats.Add(cityHashCode, city, temps);
            }

            temps.AddTemperature(temp);
        }

        Marshal.FreeHGlobal(buffer);
        return cityWithTempStats;
    }
}