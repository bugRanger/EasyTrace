```

BenchmarkDotNet v0.15.8, Windows 10 (10.0.19045.6456/22H2/2022Update)
AMD Ryzen 7 5700G with Radeon Graphics 3.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.302
  [Host]     : .NET 9.0.18 (9.0.18, 9.0.1826.31522), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 9.0.18 (9.0.18, 9.0.1826.31522), X64 RyuJIT x86-64-v3


```
| Method                      | Mean     | Error   | StdDev  | Ratio        | RatioSD | Gen0   | Socket.Send | Allocated | Alloc Ratio |
|---------------------------- |---------:|--------:|--------:|-------------:|--------:|-------:|------------:|----------:|------------:|
| System.Diagnostics.Activity | 674.3 ns | 8.51 ns | 7.11 ns |     baseline |         | 0.1450 |       31540 |    1231 B |             |
| EasyTrace.Activity          | 287.3 ns | 1.68 ns | 1.31 ns | 2.35x faster |   0.03x |      - |       64263 |         - |          NA |
