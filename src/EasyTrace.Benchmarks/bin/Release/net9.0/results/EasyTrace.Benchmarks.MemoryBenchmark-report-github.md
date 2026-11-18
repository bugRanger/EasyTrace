```

BenchmarkDotNet v0.15.8, Windows 10 (10.0.19045.6456/22H2/2022Update)
AMD Ryzen 7 5700G with Radeon Graphics 3.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.302
  [Host]     : .NET 9.0.18 (9.0.18, 9.0.1826.31522), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 9.0.18 (9.0.18, 9.0.1826.31522), X64 RyuJIT x86-64-v3

Categories=AllowOnCI  

```
| Method                      | AttributeType | IsExporter | Mean         | Error      | StdDev     | Ratio           | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|---------------------------- |-------------- |----------- |-------------:|-----------:|-----------:|----------------:|--------:|-------:|-------:|----------:|------------:|
| **System.Diagnostics.Activity** | **-**             | **False**      |   **587.180 ns** |  **7.0929 ns** |  **6.6347 ns** |        **baseline** |        **** | **0.1450** |      **-** |    **1216 B** |            **** |
| EasyTrace.Activity          | -             | False      |     9.195 ns |  0.0423 ns |  0.0375 ns |   63.86x faster |   0.74x |      - |      - |         - |          NA |
|                             |               |            |              |            |            |                 |         |        |        |           |             |
| **System.Diagnostics.Activity** | **-**             | **True**       |   **585.602 ns** |  **6.6770 ns** |  **5.9190 ns** |        **baseline** |        **** | **0.1450** |      **-** |    **1216 B** |            **** |
| EasyTrace.Activity          | -             | True       |   292.460 ns |  1.6734 ns |  1.5653 ns |    2.00x faster |   0.02x |      - |      - |         - |          NA |
|                             |               |            |              |            |            |                 |         |        |        |           |             |
| **System.Diagnostics.Activity** | **Double**        | **False**      | **1,218.633 ns** | **22.7352 ns** | **18.9849 ns** |        **baseline** |        **** | **0.3891** | **0.0019** |    **3256 B** |            **** |
| EasyTrace.Activity          | Double        | False      |     8.634 ns |  0.0408 ns |  0.0319 ns | 141.138x faster |   2.18x |      - |      - |         - |          NA |
|                             |               |            |              |            |            |                 |         |        |        |           |             |
| **System.Diagnostics.Activity** | **Double**        | **True**       | **1,249.563 ns** | **15.0373 ns** | **14.0659 ns** |        **baseline** |        **** | **0.3891** | **0.0019** |    **3256 B** |            **** |
| EasyTrace.Activity          | Double        | True       |   738.551 ns |  2.5014 ns |  2.3398 ns |    1.69x faster |   0.02x |      - |      - |         - |          NA |
|                             |               |            |              |            |            |                 |         |        |        |           |             |
| **System.Diagnostics.Activity** | **Int32**         | **False**      | **1,157.462 ns** | **19.6824 ns** | **17.4480 ns** |        **baseline** |        **** | **0.3891** | **0.0019** |    **3256 B** |            **** |
| EasyTrace.Activity          | Int32         | False      |    10.968 ns |  0.0490 ns |  0.0435 ns | 105.534x faster |   1.59x |      - |      - |         - |          NA |
|                             |               |            |              |            |            |                 |         |        |        |           |             |
| **System.Diagnostics.Activity** | **Int32**         | **True**       | **1,168.177 ns** | **11.9781 ns** | **10.6183 ns** |        **baseline** |        **** | **0.3891** | **0.0019** |    **3256 B** |            **** |
| EasyTrace.Activity          | Int32         | True       |   691.710 ns |  2.9014 ns |  2.7140 ns |    1.69x faster |   0.02x |      - |      - |         - |          NA |
|                             |               |            |              |            |            |                 |         |        |        |           |             |
| **System.Diagnostics.Activity** | **String**        | **False**      | **1,119.679 ns** | **20.4169 ns** | **19.0979 ns** |        **baseline** |        **** | **0.3014** | **0.0019** |    **2536 B** |            **** |
| EasyTrace.Activity          | String        | False      |     8.993 ns |  0.0329 ns |  0.0292 ns | 124.513x faster |   2.09x |      - |      - |         - |          NA |
|                             |               |            |              |            |            |                 |         |        |        |           |             |
| **System.Diagnostics.Activity** | **String**        | **True**       | **1,178.177 ns** | **19.9720 ns** | **18.6818 ns** |        **baseline** |        **** | **0.3014** | **0.0019** |    **2536 B** |            **** |
| EasyTrace.Activity          | String        | True       |   718.697 ns |  3.4079 ns |  2.8457 ns |    1.64x faster |   0.03x |      - |      - |         - |          NA |
