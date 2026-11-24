```

BenchmarkDotNet v0.15.8, Windows 10 (10.0.19045.6456/22H2/2022Update)
AMD Ryzen 7 5700G with Radeon Graphics 3.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.302
  [Host]     : .NET 9.0.18 (9.0.18, 9.0.1826.31522), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 9.0.18 (9.0.18, 9.0.1826.31522), X64 RyuJIT x86-64-v3

Categories=AllowOnCI  

```
| Method                      | AttributeType | IsExporter | Mean        | Error     | StdDev    | Ratio         | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|---------------------------- |-------------- |----------- |------------:|----------:|----------:|--------------:|--------:|-------:|-------:|----------:|------------:|
| **System.Diagnostics.Activity** | **-**             | **False**      |   **585.28 ns** | **11.552 ns** | **16.194 ns** |      **baseline** |        **** | **0.1450** |      **-** |    **1216 B** |            **** |
| EasyTrace.Activity          | -             | False      |    14.06 ns |  0.102 ns |  0.096 ns | 41.62x faster |   1.16x |      - |      - |         - |          NA |
|                             |               |            |             |           |           |               |         |        |        |           |             |
| **System.Diagnostics.Activity** | **-**             | **True**       |   **585.59 ns** |  **7.242 ns** |  **6.774 ns** |      **baseline** |        **** | **0.1450** |      **-** |    **1216 B** |            **** |
| EasyTrace.Activity          | -             | True       |   307.77 ns |  0.768 ns |  0.641 ns |  1.90x faster |   0.02x |      - |      - |         - |          NA |
|                             |               |            |             |           |           |               |         |        |        |           |             |
| **System.Diagnostics.Activity** | **Double**        | **False**      | **1,255.15 ns** | **12.854 ns** | **11.395 ns** |      **baseline** |        **** | **0.3891** | **0.0019** |    **3256 B** |            **** |
| EasyTrace.Activity          | Double        | False      |   199.75 ns |  0.289 ns |  0.241 ns |  6.28x faster |   0.06x |      - |      - |         - |          NA |
|                             |               |            |             |           |           |               |         |        |        |           |             |
| **System.Diagnostics.Activity** | **Double**        | **True**       | **1,231.23 ns** | **10.078 ns** |  **9.427 ns** |      **baseline** |        **** | **0.3891** | **0.0019** |    **3256 B** |            **** |
| EasyTrace.Activity          | Double        | True       |   684.55 ns |  1.998 ns |  1.560 ns |  1.80x faster |   0.01x |      - |      - |         - |          NA |
|                             |               |            |             |           |           |               |         |        |        |           |             |
| **System.Diagnostics.Activity** | **Int32**         | **False**      | **1,257.43 ns** | **19.991 ns** | **17.721 ns** |      **baseline** |        **** | **0.3891** | **0.0019** |    **3256 B** |            **** |
| EasyTrace.Activity          | Int32         | False      |   288.89 ns |  0.416 ns |  0.369 ns |  4.35x faster |   0.06x |      - |      - |         - |          NA |
|                             |               |            |             |           |           |               |         |        |        |           |             |
| **System.Diagnostics.Activity** | **Int32**         | **True**       | **1,210.11 ns** | **23.473 ns** | **37.231 ns** |      **baseline** |        **** | **0.3891** | **0.0019** |    **3256 B** |            **** |
| EasyTrace.Activity          | Int32         | True       |   706.29 ns |  2.880 ns |  2.405 ns |  1.71x faster |   0.05x |      - |      - |         - |          NA |
|                             |               |            |             |           |           |               |         |        |        |           |             |
| **System.Diagnostics.Activity** | **String**        | **False**      | **1,244.53 ns** | **24.431 ns** | **28.134 ns** |      **baseline** |        **** | **0.3014** | **0.0019** |    **2536 B** |            **** |
| EasyTrace.Activity          | String        | False      |   138.22 ns |  1.372 ns |  1.216 ns |  9.00x faster |   0.21x |      - |      - |         - |          NA |
|                             |               |            |             |           |           |               |         |        |        |           |             |
| **System.Diagnostics.Activity** | **String**        | **True**       | **1,150.19 ns** | **15.507 ns** | **12.107 ns** |      **baseline** |        **** | **0.3014** | **0.0019** |    **2536 B** |            **** |
| EasyTrace.Activity          | String        | True       |   741.58 ns |  3.384 ns |  3.166 ns |  1.55x faster |   0.02x |      - |      - |         - |          NA |
