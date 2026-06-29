```

BenchmarkDotNet v0.15.8, Windows 10 (10.0.19045.6456/22H2/2022Update)
AMD Ryzen 7 5700G with Radeon Graphics 3.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.103
  [Host]     : .NET 9.0.13 (9.0.13, 9.0.1326.6317), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 9.0.13 (9.0.13, 9.0.1326.6317), X64 RyuJIT x86-64-v3


```
| Method           | Iterations | ParallelLimit | IsExporter | Mean        | Error      | StdDev     | Gen0      | Gen1    | Allocated   |
|----------------- |----------- |-------------- |----------- |------------:|-----------:|-----------:|----------:|--------:|------------:|
| **ActivitySource**   | **1000**       | **4**             | **False**      |    **43.92 μs** |   **0.876 μs** |   **1.580 μs** |    **0.3052** |       **-** |     **2.48 KB** |
| TraceActivityRef | 1000       | 4             | False      |    15.80 μs |   0.295 μs |   0.276 μs |    0.3052 |       - |     2.47 KB |
| **ActivitySource**   | **1000**       | **4**             | **True**       |   **795.88 μs** |  **15.775 μs** |  **24.090 μs** |  **588.8672** |  **9.7656** |  **4752.54 KB** |
| TraceActivityRef | 1000       | 4             | True       |   344.26 μs |   4.812 μs |   4.501 μs |         - |       - |     2.49 KB |
| **ActivitySource**   | **10000**      | **4**             | **False**      |   **388.56 μs** |   **7.729 μs** |  **13.333 μs** |         **-** |       **-** |      **2.5 KB** |
| TraceActivityRef | 10000      | 4             | False      |   138.12 μs |   2.759 μs |   3.388 μs |    0.2441 |       - |     2.49 KB |
| **ActivitySource**   | **10000**      | **4**             | **True**       | **7,750.68 μs** | **153.829 μs** | **307.214 μs** | **5882.8125** | **46.8750** | **47502.54 KB** |
| TraceActivityRef | 10000      | 4             | True       | 3,394.63 μs |  65.616 μs |  67.382 μs |         - |       - |     2.52 KB |
