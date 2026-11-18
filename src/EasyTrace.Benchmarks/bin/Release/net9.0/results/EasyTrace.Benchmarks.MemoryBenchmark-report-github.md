```

BenchmarkDotNet v0.15.8, Windows 10 (10.0.19045.6456/22H2/2022Update)
AMD Ryzen 7 5700G with Radeon Graphics 3.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.302
  [Host]     : .NET 9.0.18 (9.0.18, 9.0.1826.31522), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 9.0.18 (9.0.18, 9.0.1826.31522), X64 RyuJIT x86-64-v3

Categories=AllowOnCI  

```
| Method                      | AttributeType | IsExporter | Mean         | Error      | StdDev      | Median       | Ratio           | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|---------------------------- |-------------- |----------- |-------------:|-----------:|------------:|-------------:|----------------:|--------:|-------:|-------:|----------:|------------:|
| **System.Diagnostics.Activity** | **-**             | **False**      |   **659.856 ns** | **12.2705 ns** |  **15.0693 ns** |   **658.272 ns** |        **baseline** |        **** | **0.1450** |      **-** |    **1216 B** |            **** |
| EasyTrace.Activity          | -             | False      |     9.054 ns |  0.0604 ns |   0.0535 ns |     9.046 ns |   72.88x faster |   1.68x |      - |      - |         - |          NA |
|                             |               |            |              |            |             |              |                 |         |        |        |           |             |
| **System.Diagnostics.Activity** | **-**             | **True**       |   **599.834 ns** | **11.7947 ns** |  **20.6575 ns** |   **595.283 ns** |        **baseline** |        **** | **0.1450** |      **-** |    **1216 B** |            **** |
| EasyTrace.Activity          | -             | True       |   520.333 ns | 10.4079 ns |  11.1364 ns |   518.003 ns |    1.15x faster |   0.05x |      - |      - |         - |          NA |
|                             |               |            |              |            |             |              |                 |         |        |        |           |             |
| **System.Diagnostics.Activity** | **Double**        | **False**      | **1,183.501 ns** | **12.9617 ns** |  **11.4902 ns** | **1,185.027 ns** |        **baseline** |        **** | **0.3891** | **0.0019** |    **3256 B** |            **** |
| EasyTrace.Activity          | Double        | False      |     8.642 ns |  0.0688 ns |   0.0574 ns |     8.636 ns | 136.960x faster |   1.56x |      - |      - |         - |          NA |
|                             |               |            |              |            |             |              |                 |         |        |        |           |             |
| **System.Diagnostics.Activity** | **Double**        | **True**       | **1,242.208 ns** | **21.5322 ns** |  **19.0878 ns** | **1,240.172 ns** |        **baseline** |        **** | **0.3891** | **0.0019** |    **3256 B** |            **** |
| EasyTrace.Activity          | Double        | True       | 5,651.973 ns | 87.1449 ns | 113.3130 ns | 5,605.788 ns |    4.55x slower |   0.11x |      - |      - |         - |          NA |
|                             |               |            |              |            |             |              |                 |         |        |        |           |             |
| **System.Diagnostics.Activity** | **Int32**         | **False**      | **1,228.026 ns** | **24.0189 ns** |  **22.4673 ns** | **1,222.679 ns** |        **baseline** |        **** | **0.3891** | **0.0019** |    **3256 B** |            **** |
| EasyTrace.Activity          | Int32         | False      |     8.634 ns |  0.0499 ns |   0.0442 ns |     8.640 ns | 142.237x faster |   2.62x |      - |      - |         - |          NA |
|                             |               |            |              |            |             |              |                 |         |        |        |           |             |
| **System.Diagnostics.Activity** | **Int32**         | **True**       | **1,180.309 ns** | **23.1978 ns** |  **18.1113 ns** | **1,178.154 ns** |        **baseline** |        **** | **0.3891** | **0.0019** |    **3256 B** |            **** |
| EasyTrace.Activity          | Int32         | True       | 2,030.253 ns | 39.8854 ns |  35.3574 ns | 2,021.441 ns |    1.72x slower |   0.04x |      - |      - |         - |          NA |
|                             |               |            |              |            |             |              |                 |         |        |        |           |             |
| **System.Diagnostics.Activity** | **String**        | **False**      | **1,163.366 ns** | **21.3846 ns** |  **20.0032 ns** | **1,163.441 ns** |        **baseline** |        **** | **0.3014** | **0.0019** |    **2536 B** |            **** |
| EasyTrace.Activity          | String        | False      |     9.818 ns |  0.0466 ns |   0.0389 ns |     9.814 ns | 118.494x faster |   2.02x |      - |      - |         - |          NA |
|                             |               |            |              |            |             |              |                 |         |        |        |           |             |
| **System.Diagnostics.Activity** | **String**        | **True**       | **1,229.192 ns** | **15.3604 ns** |  **11.9924 ns** | **1,228.685 ns** |        **baseline** |        **** | **0.3014** | **0.0019** |    **2536 B** |            **** |
| EasyTrace.Activity          | String        | True       | 1,996.728 ns | 37.4288 ns |  87.4886 ns | 1,961.883 ns |    1.62x slower |   0.07x |      - |      - |         - |          NA |
