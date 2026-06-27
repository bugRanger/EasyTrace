```

BenchmarkDotNet v0.15.8, Windows 10 (10.0.19045.6456/22H2/2022Update)
AMD Ryzen 3 2200G with Radeon Vega Graphics 3.50GHz, 1 CPU, 4 logical and 4 physical cores
.NET SDK 10.0.201
  [Host]     : .NET 9.0.14 (9.0.14, 9.0.1426.11910), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 9.0.14 (9.0.14, 9.0.1426.11910), X64 RyuJIT x86-64-v3


```
| Method     | Iterations | IsExporter | Mean      | Error     | StdDev    | Median    | Gen0      | Allocated |
|----------- |----------- |----------- |----------:|----------:|----------:|----------:|----------:|----------:|
| **Activity**   | **1000**       | **False**      |  **3.816 ms** | **0.0312 ms** | **0.0277 ms** |  **3.814 ms** |         **-** |      **32 B** |
| TraceScope | 1000       | False      |  3.071 ms | 0.0597 ms | 0.0965 ms |  3.049 ms |         - |      32 B |
| **Activity**   | **1000**       | **True**       |  **3.812 ms** | **0.0754 ms** | **0.1836 ms** |  **3.753 ms** |  **386.7188** |  **816032 B** |
| TraceScope | 1000       | True       |  3.886 ms | 0.0656 ms | 0.0645 ms |  3.879 ms |         - |      32 B |
| **Activity**   | **10000**      | **False**      | **37.754 ms** | **0.7475 ms** | **1.5603 ms** | **37.827 ms** |         **-** |      **32 B** |
| TraceScope | 10000      | False      | 36.388 ms | 0.7253 ms | 1.1292 ms | 36.678 ms |         - |      32 B |
| **Activity**   | **10000**      | **True**       | **43.773 ms** | **0.4279 ms** | **0.4003 ms** | **43.700 ms** | **3846.1538** | **8160032 B** |
| TraceScope | 10000      | True       | 44.052 ms | 0.6760 ms | 0.6324 ms | 43.960 ms |         - |      32 B |
