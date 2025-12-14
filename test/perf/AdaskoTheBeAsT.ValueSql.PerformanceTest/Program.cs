using System;
using System.Linq;
using AdaskoTheBeAsT.ValueSql.PerformanceTest.Benchmarks;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

if (args.Contains("--quick", StringComparer.OrdinalIgnoreCase))
{
    var quickConfig = ManualConfig.Create(DefaultConfig.Instance)
        .AddJob(Job.ShortRun
            .WithWarmupCount(1)
            .WithIterationCount(3)
            .WithInvocationCount(16))
        .WithOptions(ConfigOptions.JoinSummary);

    BenchmarkRunner.Run<QuickBenchmarks>(quickConfig);
}
else
{
    var config = DefaultConfig.Instance
        .WithOptions(ConfigOptions.JoinSummary);

    BenchmarkRunner.Run(
        [
            typeof(QueryBenchmarks),
            typeof(LargeDatasetBenchmarks),
        ],
        config,
        args);
}
