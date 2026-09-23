using System.Linq;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace EasyTrace.Benchmarks.Metrics;

public class SocketSendColumn(string metricName) : IColumn
{
    public string Id => nameof(SocketSendColumn);
    public string ColumnName => "Socket.Send";
    public string Legend => "Number of Socket.Send calls per operation";
    public bool AlwaysShow => true;
    public ColumnCategory Category => ColumnCategory.Metric;
    public int PriorityInCategory => 0;
    public bool IsNumeric => true;
    public UnitType UnitType => UnitType.Dimensionless;

    public string GetValue(Summary summary, BenchmarkCase benchmarkCase)
    {
        var report = summary.Reports.FirstOrDefault(r => r.BenchmarkCase == benchmarkCase);
        if (report == null)
        {
            return "0";
        }

        var metricLines = report.ExecuteResults
            .SelectMany(result => result.StandardOutput)
            .Where(line => line.StartsWith(metricName))
            .ToList();

        double totalCalls = 0;
        var count = 0;

        foreach (var valueStr in metricLines.Select(line => line[metricName.Length..]))
        {
            if (!double.TryParse(valueStr, out var value)) continue;

            totalCalls += value;
            count++;
        }

        if (count == 0)
        {
            return "0";
        }

        var averageCallsPerIteration = totalCalls / count;
        var callsPerOperation = averageCallsPerIteration / benchmarkCase.Descriptor.OperationsPerInvoke;

        return callsPerOperation.ToString("0.##");
    }

    public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style) =>
        GetValue(summary, benchmarkCase);

    public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => false;

    public bool IsAvailable(Summary summary) => true;
}