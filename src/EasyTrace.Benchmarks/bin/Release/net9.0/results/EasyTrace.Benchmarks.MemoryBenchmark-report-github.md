```

BenchmarkDotNet v0.15.8, Windows 10 (10.0.19045.6456/22H2/2022Update)
AMD Ryzen 7 5700G with Radeon Graphics 3.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.302
  [Host]     : .NET 9.0.18 (9.0.18, 9.0.1826.31522), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 9.0.18 (9.0.18, 9.0.1826.31522), X64 RyuJIT x86-64-v3

Categories=AllowOnCI  

```
| Method             | AttributeType | IsExporter | Mean       | Error    | StdDev   | Ratio        | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------------- |-------------- |----------- |-----------:|---------:|---------:|-------------:|--------:|-------:|-------:|----------:|------------:|
| **ActivitySource**     | **-**             | **True**       |   **569.3 ns** |  **6.18 ns** |  **5.48 ns** |     **baseline** |        **** | **0.1450** |      **-** |    **1216 B** |            **** |
| TraceActivityScope | -             | True       |   301.8 ns |  0.99 ns |  0.83 ns | 1.89x faster |   0.02x |      - |      - |         - |          NA |
|                    |               |            |            |          |          |              |         |        |        |           |             |
| **ActivitySource**     | **Double**        | **True**       | **1,230.6 ns** | **24.26 ns** | **22.69 ns** |     **baseline** |        **** | **0.3891** | **0.0019** |    **3256 B** |            **** |
| TraceActivityScope | Double        | True       |   734.4 ns |  2.92 ns |  2.73 ns | 1.68x faster |   0.03x |      - |      - |         - |          NA |
|                    |               |            |            |          |          |              |         |        |        |           |             |
| **ActivitySource**     | **Int32**         | **True**       | **1,179.0 ns** |  **9.34 ns** |  **8.74 ns** |     **baseline** |        **** | **0.3891** | **0.0019** |    **3256 B** |            **** |
| TraceActivityScope | Int32         | True       |   722.8 ns |  1.56 ns |  1.46 ns | 1.63x faster |   0.01x |      - |      - |         - |          NA |
|                    |               |            |            |          |          |              |         |        |        |           |             |
| **ActivitySource**     | **String**        | **True**       | **1,126.8 ns** |  **9.43 ns** |  **8.82 ns** |     **baseline** |        **** | **0.3014** | **0.0019** |    **2536 B** |            **** |
| TraceActivityScope | String        | True       |   716.6 ns |  2.21 ns |  2.07 ns | 1.57x faster |   0.01x |      - |      - |         - |          NA |
