```

BenchmarkDotNet v0.15.8, Windows 10 (10.0.19045.6456/22H2/2022Update)
AMD Ryzen 7 5700G with Radeon Graphics 3.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.302
  [Host]     : .NET 9.0.18 (9.0.18, 9.0.1826.31522), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 9.0.18 (9.0.18, 9.0.1826.31522), X64 RyuJIT x86-64-v3


```
| Method         | Mean    | Error    | StdDev   | Ratio        | RatioSD | Allocated | Alloc Ratio |
|--------------- |--------:|---------:|---------:|-------------:|--------:|----------:|------------:|
| ActivitySource | 6.258 s | 0.0362 s | 0.0338 s |     baseline |         | 319.57 KB |             |
| TraceActivity  | 6.272 s | 0.0078 s | 0.0073 s | 1.00x slower |   0.01x |  10.09 KB | 31.68x less |
