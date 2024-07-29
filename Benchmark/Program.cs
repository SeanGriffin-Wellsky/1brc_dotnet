// See https://aka.ms/new-console-template for more information

using System.Text;
using Benchmark;
using BenchmarkDotNet.Running;
using Bogus;
using ConsoleApp;

// Randomizer.Seed = new Random(10);
//
// List<ReadOnlyMemory<byte>> sampleMemories = new(500);
//
// for (int i = 0; i < 500; ++i)
// {
//     var city = new Bogus.DataSets.Address().City();
//     sampleMemories.Add(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(city)));
// }
//
// var dict = new Dictionary<ReadOnlyMemory<byte>, int>(500, new SpanHashCodeBenchmark.PortFromStringComparer());
// foreach (var sample in sampleMemories)
// {
//     dict.TryAdd(sample, 13);
// }
//
// return;
BenchmarkRunner.Run<FloatParsing>();