using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;

var configuration = ManualConfig.Create(DefaultConfig.Instance)
    .WithOptions(ConfigOptions.JoinSummary)
    .AddLogger(ConsoleLogger.Default)
    .WithOptions(ConfigOptions.DisableLogFile)
    .AddColumnProvider(DefaultColumnProviders.Instance);
BenchmarkRunner.Run(typeof(Program).Assembly, configuration);