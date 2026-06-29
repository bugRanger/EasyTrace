```

BenchmarkDotNet v0.15.8, Windows 10 (10.0.19045.6456/22H2/2022Update)
AMD Ryzen 7 5700G with Radeon Graphics 3.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.103
  [Host]     : .NET 9.0.13 (9.0.13, 9.0.1326.6317), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 9.0.13 (9.0.13, 9.0.1326.6317), X64 RyuJIT x86-64-v3


```
| Method           | Iterations | ParallelLimit | IsExporter | Mean        | Error     | StdDev     | Median      | Gen0      | Gen1    | Allocated   |
|----------------- |----------- |-------------- |----------- |------------:|----------:|-----------:|------------:|----------:|--------:|------------:|
| **ActivitySource**   | **1000**       | **4**             | **False**      |    **43.42 μs** |  **0.866 μs** |   **2.252 μs** |    **43.36 μs** |    **0.2441** |       **-** |     **2.48 KB** |
| TraceActivityRef | 1000       | 4             | False      |    16.15 μs |  0.251 μs |   0.222 μs |    16.07 μs |    0.3052 |       - |     2.48 KB |
| **ActivitySource**   | **1000**       | **4**             | **True**       |   **754.08 μs** | **14.908 μs** |  **28.364 μs** |   **754.12 μs** |  **588.8672** |  **9.7656** |  **4752.54 KB** |
| TraceActivityRef | 1000       | 4             | True       |   341.85 μs |  2.581 μs |   2.015 μs |   341.62 μs |         - |       - |     2.51 KB |
| **ActivitySource**   | **10000**      | **4**             | **False**      |   **394.35 μs** |  **7.847 μs** |  **13.744 μs** |   **392.32 μs** |         **-** |       **-** |      **2.5 KB** |
| TraceActivityRef | 10000      | 4             | False      |   139.95 μs |  2.774 μs |   4.401 μs |   139.84 μs |    0.2441 |       - |     2.49 KB |
| **ActivitySource**   | **10000**      | **4**             | **True**       | **6,722.27 μs** | **93.437 μs** |  **87.401 μs** | **6,726.45 μs** | **5882.8125** | **54.6875** | **47502.54 KB** |
| TraceActivityRef | 10000      | 4             | True       | 3,500.19 μs | 69.626 μs | 129.055 μs | 3,564.43 μs |         - |       - |     2.53 KB |
