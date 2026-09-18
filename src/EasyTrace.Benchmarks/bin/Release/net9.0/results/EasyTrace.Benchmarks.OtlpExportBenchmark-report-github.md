```

BenchmarkDotNet v0.15.8, Windows 10 (10.0.19045.6456/22H2/2022Update)
AMD Ryzen 3 2200G with Radeon Vega Graphics 3.50GHz, 1 CPU, 4 logical and 4 physical cores
.NET SDK 10.0.302
  [Host]     : .NET 9.0.14 (9.0.14, 9.0.1426.11910), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 9.0.14 (9.0.14, 9.0.1426.11910), X64 RyuJIT x86-64-v3


```
| Method         | Mean     | Error    | StdDev   | Ratio        | RatioSD | Allocated | Alloc Ratio |
|--------------- |---------:|---------:|---------:|-------------:|--------:|----------:|------------:|
| ActivitySource | 62.10 ms | 0.330 ms | 0.309 ms |     baseline |         |  119569 B |             |
| TraceActivity  | 62.48 ms | 0.298 ms | 0.279 ms | 1.01x slower |   0.01x |         - |          NA |
