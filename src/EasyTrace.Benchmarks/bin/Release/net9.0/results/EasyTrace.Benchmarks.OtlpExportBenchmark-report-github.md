```

BenchmarkDotNet v0.15.8, Windows 10 (10.0.19045.6456/22H2/2022Update)
AMD Ryzen 7 5700G with Radeon Graphics 3.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.302
  [Host]     : .NET 9.0.18 (9.0.18, 9.0.1826.31522), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 9.0.18 (9.0.18, 9.0.1826.31522), X64 RyuJIT x86-64-v3


```
| Method                         | Async | ActivityCount | Mean      | Error     | StdDev    | Median    | Ratio        | RatioSD | Gen0   | Socket.Send | Gen1   | Allocated | Alloc Ratio |
|------------------------------- |------ |-------------- |----------:|----------:|----------:|----------:|-------------:|--------:|-------:|------------:|-------:|----------:|------------:|
| **System.Diagnostics.Activity**    | **False** | **5**             |  **2.346 μs** | **0.0350 μs** | **0.0327 μs** |  **2.333 μs** |     **baseline** |        **** | **0.4883** |       **48427** |      **-** |    **4130 B** |            **** |
| EasyTrace.Activity.ThreadLocal | False | 5             |  1.093 μs | 0.0031 μs | 0.0029 μs |  1.093 μs | 2.15x faster |   0.03x |      - |       38481 |      - |         - |          NA |
| EasyTrace.Activity.AsyncLocal  | False | 5             |  2.560 μs | 0.0371 μs | 0.0310 μs |  2.561 μs | 1.09x slower |   0.02x | 3.1319 |       37671 | 0.2594 |   26208 B |  6.35x more |
| EasyTrace.Activity.AsyncState  | False | 5             |  2.030 μs | 0.0402 μs | 0.1008 μs |  2.007 μs | 1.16x faster |   0.06x | 0.0572 |       44832 |      - |     480 B |  8.60x less |
|                                |       |               |           |           |           |           |              |         |        |             |        |           |             |
| **System.Diagnostics.Activity**    | **False** | **25**            | **12.277 μs** | **0.2454 μs** | **0.5489 μs** | **12.052 μs** |     **baseline** |        **** | **2.4414** |       **78693** |      **-** |   **20680 B** |            **** |
| EasyTrace.Activity.ThreadLocal | False | 25            |  5.214 μs | 0.0243 μs | 0.0203 μs |  5.208 μs | 2.35x faster |   0.10x |      - |       35947 |      - |         - |          NA |
| EasyTrace.Activity.AsyncLocal  | False | 25            |  7.618 μs | 0.0534 μs | 0.0473 μs |  7.614 μs | 1.61x faster |   0.07x | 3.1281 |       38032 | 0.2594 |   26208 B |  1.27x more |
| EasyTrace.Activity.AsyncState  | False | 25            |  9.439 μs | 0.1111 μs | 0.1039 μs |  9.433 μs | 1.30x faster |   0.06x | 0.2747 |       42228 |      - |    2400 B |  8.62x less |
|                                |       |               |           |           |           |           |              |         |        |             |        |           |             |
| **System.Diagnostics.Activity**    | **False** | **100**           | **48.025 μs** | **0.8499 μs** | **0.9447 μs** | **47.596 μs** |     **baseline** |        **** | **9.7656** |       **36713** |      **-** |   **82467 B** |            **** |
| EasyTrace.Activity.ThreadLocal | False | 100           | 20.636 μs | 0.1535 μs | 0.1361 μs | 20.670 μs | 2.33x faster |   0.05x |      - |       49159 |      - |         - |          NA |
| EasyTrace.Activity.AsyncLocal  | False | 100           | 25.793 μs | 0.0896 μs | 0.0839 μs | 25.782 μs | 1.86x faster |   0.04x | 3.1128 |       38733 | 0.2441 |   26208 B |  3.15x less |
| EasyTrace.Activity.AsyncState  | False | 100           | 36.526 μs | 0.1800 μs | 0.1684 μs | 36.544 μs | 1.31x faster |   0.03x | 1.0986 |       44151 |      - |    9600 B |  8.59x less |
|                                |       |               |           |           |           |           |              |         |        |             |        |           |             |
| **System.Diagnostics.Activity**    | **True**  | **5**             |  **4.865 μs** | **0.0966 μs** | **0.1614 μs** |  **4.857 μs** |     **baseline** |        **** | **0.4883** |       **30422** |      **-** |    **4290 B** |            **** |
| EasyTrace.Activity.ThreadLocal | True  | 5             |        NA |        NA |        NA |        NA |            ? |       ? |     NA |           0 |     NA |        NA |           ? |
| EasyTrace.Activity.AsyncLocal  | True  | 5             |  5.526 μs | 0.1087 μs | 0.1660 μs |  5.552 μs | 1.14x slower |   0.05x | 3.4180 |       33860 | 0.2441 |   26433 B |  6.16x more |
| EasyTrace.Activity.AsyncState  | True  | 5             |  3.649 μs | 0.0583 μs | 0.0545 μs |  3.667 μs | 1.33x faster |   0.05x | 0.0610 |       36303 |      - |     704 B |  6.09x less |
|                                |       |               |           |           |           |           |              |         |        |             |        |           |             |
| **System.Diagnostics.Activity**    | **True**  | **25**            | **30.755 μs** | **0.5825 μs** | **0.6709 μs** | **30.622 μs** |     **baseline** |        **** | **2.4414** |       **49627** |      **-** |   **21281 B** |            **** |
| EasyTrace.Activity.ThreadLocal | True  | 25            |        NA |        NA |        NA |        NA |            ? |       ? |     NA |           0 |     NA |        NA |           ? |
| EasyTrace.Activity.AsyncLocal  | True  | 25            | 24.810 μs | 0.2349 μs | 0.1962 μs | 24.889 μs | 1.24x faster |   0.03x | 3.4180 |       44359 | 0.2441 |   26495 B |  1.25x more |
| EasyTrace.Activity.AsyncState  | True  | 25            | 26.011 μs | 0.3173 μs | 0.2968 μs | 26.053 μs | 1.18x faster |   0.03x | 0.2441 |       41555 |      - |    2688 B |  7.92x less |
|                                |       |               |           |           |           |           |              |         |        |             |        |           |             |
| **System.Diagnostics.Activity**    | **True**  | **100**           | **87.129 μs** | **1.7414 μs** | **3.4777 μs** | **85.450 μs** |     **baseline** |        **** | **9.7656** |       **65269** |      **-** |   **83345 B** |            **** |
| EasyTrace.Activity.ThreadLocal | True  | 100           |        NA |        NA |        NA |        NA |            ? |       ? |     NA |           0 |     NA |        NA |           ? |
| EasyTrace.Activity.AsyncLocal  | True  | 100           | 57.998 μs | 0.2267 μs | 0.1893 μs | 57.982 μs | 1.50x faster |   0.06x | 3.4180 |       42708 | 0.2441 |   26496 B |  3.15x less |
| EasyTrace.Activity.AsyncState  | True  | 100           | 68.312 μs | 0.3644 μs | 0.3408 μs | 68.485 μs | 1.28x faster |   0.05x | 1.0986 |       45468 |      - |    9888 B |  8.43x less |

Benchmarks with issues:
  OtlpExportBenchmark.EasyTrace.Activity.ThreadLocal: DefaultJob [Async=True, ActivityCount=5]
  OtlpExportBenchmark.EasyTrace.Activity.ThreadLocal: DefaultJob [Async=True, ActivityCount=25]
  OtlpExportBenchmark.EasyTrace.Activity.ThreadLocal: DefaultJob [Async=True, ActivityCount=100]
