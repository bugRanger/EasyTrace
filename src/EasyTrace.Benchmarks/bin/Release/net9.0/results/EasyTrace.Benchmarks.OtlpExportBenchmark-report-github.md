```

BenchmarkDotNet v0.15.8, Windows 10 (10.0.19045.6456/22H2/2022Update)
AMD Ryzen 7 5700G with Radeon Graphics 3.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.302
  [Host]     : .NET 9.0.18 (9.0.18, 9.0.1826.31522), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 9.0.18 (9.0.18, 9.0.1826.31522), X64 RyuJIT x86-64-v3


```
| Method                      | Mean     | Error    | StdDev   | Median   | Ratio        | RatioSD | Gen0   | Socket.Send | Allocated | Alloc Ratio |
|---------------------------- |---------:|---------:|---------:|---------:|-------------:|--------:|-------:|------------:|----------:|------------:|
| System.Diagnostics.Activity | 749.4 ns | 19.27 ns | 56.53 ns | 723.5 ns |     baseline |         | 0.1450 |      108931 |    1239 B |             |
| EasyTrace.Activity          | 302.8 ns |  2.21 ns |  1.96 ns | 302.1 ns | 2.48x faster |   0.19x |      - |      137690 |         - |          NA |
