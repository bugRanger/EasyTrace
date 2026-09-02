```

BenchmarkDotNet v0.15.8, Windows 10 (10.0.19045.6456/22H2/2022Update)
AMD Ryzen 7 5700G with Radeon Graphics 3.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.302
  [Host]     : .NET 9.0.18 (9.0.18, 9.0.1826.31522), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 9.0.18 (9.0.18, 9.0.1826.31522), X64 RyuJIT x86-64-v3


```
| Method         | Iterations | Mean    | Error    | StdDev   | Ratio        | RatioSD | Allocated | Alloc Ratio |
|--------------- |----------- |--------:|---------:|---------:|-------------:|--------:|----------:|------------:|
| ActivitySource | 100        | 6.273 s | 0.0159 s | 0.0149 s |     baseline |         | 319.57 KB |             |
| TraceActivity  | 100        | 6.278 s | 0.0069 s | 0.0065 s | 1.00x slower |   0.00x |  10.09 KB | 31.68x less |
