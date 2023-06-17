using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using MeterFactoryDemo.Microbenchmarks;

//var counterBenchmark = new PublisherBenchmark();
//counterBenchmark.TagCount = 20;
//counterBenchmark.GlobalSetup();

//for (int i = 0; i < 1000 * 1000 * 1000; i++)
//{
//    counterBenchmark.RecordDuration();
//}

var config = ManualConfig.Create(DefaultConfig.Instance).WithOptions(ConfigOptions.JoinSummary);

var summary = BenchmarkRunner.Run(new []
{
    BenchmarkConverter.TypeToBenchmarks(typeof(BuiltInBenchmark), config),
    BenchmarkConverter.TypeToBenchmarks(typeof(PublisherBenchmark), config)
});