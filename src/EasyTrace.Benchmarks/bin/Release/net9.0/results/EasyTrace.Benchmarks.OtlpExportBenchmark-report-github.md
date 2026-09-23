```

BenchmarkDotNet v0.15.8, Windows 10 (10.0.19045.6456/22H2/2022Update)
AMD Ryzen 3 2200G with Radeon Vega Graphics 3.50GHz, 1 CPU, 4 logical and 4 physical cores
.NET SDK 10.0.302
  [Host]     : .NET 9.0.14 (9.0.14, 9.0.1426.11910), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 9.0.14 (9.0.14, 9.0.1426.11910), X64 RyuJIT x86-64-v3


```
| Method                      | Mean       | Error    | StdDev   | Ratio        | RatioSD | Gen0   | Socket.Send | Allocated | Alloc Ratio |
|---------------------------- |-----------:|---------:|---------:|-------------:|--------:|-------:|------------:|----------:|------------:|
| System.Diagnostics.Activity | 1,304.7 ns | 25.63 ns | 44.88 ns |     baseline |         | 0.5798 |       29306 |    1234 B |             |
| EasyTrace.Activity          |   559.3 ns | 11.20 ns | 20.76 ns | 2.34x faster |   0.12x |      - |       40390 |         - |          NA |
