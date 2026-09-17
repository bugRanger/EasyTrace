```

BenchmarkDotNet v0.15.8, Windows 10 (10.0.19045.6456/22H2/2022Update)
AMD Ryzen 3 2200G with Radeon Vega Graphics 3.50GHz, 1 CPU, 4 logical and 4 physical cores
.NET SDK 10.0.302
  [Host]     : .NET 9.0.14 (9.0.14, 9.0.1426.11910), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 9.0.14 (9.0.14, 9.0.1426.11910), X64 RyuJIT x86-64-v3

Categories=AllowOnCI  

```
| Method             | AttributeType | IsExporter | Mean        | Error     | StdDev    | Ratio           | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------- |-------------- |----------- |------------:|----------:|----------:|----------------:|--------:|-------:|----------:|------------:|
| **ActivitySource**     | **-**             | **False**      |   **970.66 ns** | **13.600 ns** | **11.356 ns** |        **baseline** |        **** | **0.5798** |    **1216 B** |            **** |
| TraceActivityScope | -             | False      |    14.83 ns |  0.256 ns |  0.239 ns |   65.47x faster |   1.27x |      - |         - |          NA |
|                    |               |            |             |           |           |                 |         |        |           |             |
| **ActivitySource**     | **-**             | **True**       |   **976.52 ns** | **15.535 ns** | **12.972 ns** |        **baseline** |        **** | **0.5798** |    **1216 B** |            **** |
| TraceActivityScope | -             | True       |   502.57 ns |  9.701 ns | 11.548 ns |    1.94x faster |   0.05x |      - |         - |          NA |
|                    |               |            |             |           |           |                 |         |        |           |             |
| **ActivitySource**     | **Double**        | **False**      | **2,485.53 ns** | **47.860 ns** | **51.209 ns** |        **baseline** |        **** | **1.5564** |    **3256 B** |            **** |
| TraceActivityScope | Double        | False      |    14.83 ns |  0.261 ns |  0.244 ns | 167.606x faster |   4.30x |      - |         - |          NA |
|                    |               |            |             |           |           |                 |         |        |           |             |
| **ActivitySource**     | **Double**        | **True**       | **2,360.21 ns** | **40.339 ns** | **41.425 ns** |        **baseline** |        **** | **1.5564** |    **3256 B** |            **** |
| TraceActivityScope | Double        | True       | 1,213.14 ns | 19.811 ns | 18.531 ns |    1.95x faster |   0.04x |      - |         - |          NA |
|                    |               |            |             |           |           |                 |         |        |           |             |
| **ActivitySource**     | **Int32**         | **False**      | **2,355.44 ns** | **45.743 ns** | **44.925 ns** |        **baseline** |        **** | **1.5564** |    **3256 B** |            **** |
| TraceActivityScope | Int32         | False      |    14.97 ns |  0.340 ns |  0.334 ns | 157.411x faster |   4.45x |      - |         - |          NA |
|                    |               |            |             |           |           |                 |         |        |           |             |
| **ActivitySource**     | **Int32**         | **True**       | **2,507.98 ns** | **49.951 ns** | **55.521 ns** |        **baseline** |        **** | **1.5564** |    **3256 B** |            **** |
| TraceActivityScope | Int32         | True       | 1,217.93 ns | 22.905 ns | 21.426 ns |    2.06x faster |   0.06x |      - |         - |          NA |
|                    |               |            |             |           |           |                 |         |        |           |             |
| **ActivitySource**     | **String**        | **False**      | **2,314.45 ns** | **46.350 ns** | **66.474 ns** |        **baseline** |        **** | **1.2093** |    **2536 B** |            **** |
| TraceActivityScope | String        | False      |    14.83 ns |  0.324 ns |  0.303 ns | 156.123x faster |   5.37x |      - |         - |          NA |
|                    |               |            |             |           |           |                 |         |        |           |             |
| **ActivitySource**     | **String**        | **True**       | **2,287.18 ns** | **41.196 ns** | **36.519 ns** |        **baseline** |        **** | **1.2093** |    **2536 B** |            **** |
| TraceActivityScope | String        | True       | 1,253.09 ns | 20.884 ns | 19.535 ns |    1.83x faster |   0.04x |      - |         - |          NA |
