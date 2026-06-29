using System.IO;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;

namespace EasyTrace.Benchmarks
{
    internal class Program
    {
        private static void Main(string[] args) => BenchmarkSwitcher
            .FromAssembly(typeof(Program).Assembly)
            .Run(args);
    }

    public static class BenchmarkRunner
    {
        public static Summary Run<T>() => BenchmarkDotNet.Running.BenchmarkRunner.Run<T>(BenchmarkConfig.Shared);
    }

    public static class BenchmarkConfig
    {
        public static ManualConfig Shared =>
            ManualConfig.Create(DefaultConfig.Instance)
                .AddValidator(ExecutionValidator.FailOnError)
                .AddValidator(ReturnValueValidator.FailOnError)
                .AddColumn(CategoriesColumn.Default)
                .WithOptions(ConfigOptions.JoinSummary)
                .WithArtifactsPath(Directory.GetCurrentDirectory())
                .WithSummaryStyle(SummaryStyle.Default
                    .WithRatioStyle(RatioStyle.Trend));
    }
}