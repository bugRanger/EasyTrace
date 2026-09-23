![Alt text](./images/logo.png)

[![build](https://github.com/bugRanger/EasyTrace/actions/workflows/ci.yml/badge.svg)](https://github.com/bugRanger/EasyTrace/actions/workflows/ci.yml)

# EasyTrace

A lightweight project for tracing high-load systems.

## Description

The goal of the repository is to offer an
alternative [System.Diagnostics.Activity](https://learn.microsoft.com/ru-ru/dotnet/api/system.diagnostics.activity?view=net-9.0)
that is less expensive in terms of RAM and garbage collection load.

## 🔥 Benchmarks

### Generation activity
```
BenchmarkDotNet v0.15.8, Windows 10 (10.0.19045.6456/22H2/2022Update)
AMD Ryzen 3 2200G with Radeon Vega Graphics 3.50GHz, 1 CPU, 4 logical and 4 physical cores
.NET SDK 10.0.302
  [Host]     : .NET 9.0.14 (9.0.14, 9.0.1426.11910), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 9.0.14 (9.0.14, 9.0.1426.11910), X64 RyuJIT x86-64-v3

Categories=AllowOnCI  
```
| Method                      | AttributeType | IsExporter | Mean        | Error     | StdDev     | Ratio           | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |-------------- |----------- |------------:|----------:|-----------:|----------------:|--------:|-------:|----------:|------------:|
| **System.Diagnostics.Activity** | **-**             | **False**      | **1,131.65 ns** | **22.371 ns** |  **21.971 ns** |        **baseline** |        **** | **0.5798** |    **1216 B** |            **** |
| EasyTrace.Activity          | -             | False      |    16.68 ns |  0.244 ns |   0.228 ns |   67.87x faster |   1.57x |      - |         - |          NA |
|                             |               |            |             |           |            |                 |         |        |           |             |
| **System.Diagnostics.Activity** | **-**             | **True**       | **1,074.85 ns** | **21.251 ns** |  **33.085 ns** |        **baseline** |        **** | **0.5798** |    **1216 B** |            **** |
| EasyTrace.Activity          | -             | True       |   524.01 ns | 10.300 ns |  16.337 ns |    2.05x faster |   0.09x |      - |         - |          NA |
|                             |               |            |             |           |            |                 |         |        |           |             |
| **System.Diagnostics.Activity** | **Double**        | **False**      | **2,707.28 ns** | **53.256 ns** | **101.325 ns** |        **baseline** |        **** | **1.5564** |    **3256 B** |            **** |
| EasyTrace.Activity          | Double        | False      |    15.93 ns |  0.350 ns |   0.525 ns | 170.162x faster |   8.37x |      - |         - |          NA |
|                             |               |            |             |           |            |                 |         |        |           |             |
| **System.Diagnostics.Activity** | **Double**        | **True**       | **2,612.02 ns** | **42.183 ns** |  **37.394 ns** |        **baseline** |        **** | **1.5564** |    **3256 B** |            **** |
| EasyTrace.Activity          | Double        | True       | 1,332.76 ns | 26.403 ns |  30.406 ns |    1.96x faster |   0.05x |      - |         - |          NA |
|                             |               |            |             |           |            |                 |         |        |           |             |
| **System.Diagnostics.Activity** | **Int32**         | **False**      | **2,743.77 ns** | **49.344 ns** |  **67.543 ns** |        **baseline** |        **** | **1.5564** |    **3256 B** |            **** |
| EasyTrace.Activity          | Int32         | False      |    16.01 ns |  0.359 ns |   0.504 ns | 171.556x faster |   6.79x |      - |         - |          NA |
|                             |               |            |             |           |            |                 |         |        |           |             |
| **System.Diagnostics.Activity** | **Int32**         | **True**       | **2,648.66 ns** | **49.519 ns** |  **46.320 ns** |        **baseline** |        **** | **1.5564** |    **3256 B** |            **** |
| EasyTrace.Activity          | Int32         | True       | 1,296.81 ns | 25.199 ns |  30.946 ns |    2.04x faster |   0.06x |      - |         - |          NA |
|                             |               |            |             |           |            |                 |         |        |           |             |
| **System.Diagnostics.Activity** | **String**        | **False**      | **2,465.47 ns** | **49.319 ns** |  **62.373 ns** |        **baseline** |        **** | **1.2093** |    **2536 B** |            **** |
| EasyTrace.Activity          | String        | False      |    15.70 ns |  0.346 ns |   0.496 ns | 157.187x faster |   6.17x |      - |         - |          NA |
|                             |               |            |             |           |            |                 |         |        |           |             |
| **System.Diagnostics.Activity** | **String**        | **True**       | **2,423.92 ns** | **47.133 ns** |  **57.883 ns** |        **baseline** |        **** | **1.2093** |    **2536 B** |            **** |
| EasyTrace.Activity          | String        | True       | 1,377.03 ns | 26.858 ns |  25.123 ns |    1.76x faster |   0.05x |      - |         - |          NA |

### Generation and export activity (via [.NET OpenTelemetry](https://github.com/open-telemetry/opentelemetry-dotnet))
```
BenchmarkDotNet v0.15.8, Windows 10 (10.0.19045.6456/22H2/2022Update)
AMD Ryzen 3 2200G with Radeon Vega Graphics 3.50GHz, 1 CPU, 4 logical and 4 physical cores
.NET SDK 10.0.302
  [Host]     : .NET 9.0.14 (9.0.14, 9.0.1426.11910), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 9.0.14 (9.0.14, 9.0.1426.11910), X64 RyuJIT x86-64-v3
```
| Method                      | Mean       | Error    | StdDev   | Ratio        | RatioSD | Gen0   | Socket.Send | Allocated | Alloc Ratio |
|---------------------------- |-----------:|---------:|---------:|-------------:|--------:|-------:|------------:|----------:|------------:|
| System.Diagnostics.Activity | 1,304.7 ns | 25.63 ns | 44.88 ns |     baseline |         | 0.5798 |       29306 |    1234 B |             |
| EasyTrace.Activity          |   559.3 ns | 11.20 ns | 20.76 ns | 2.34x faster |   0.12x |      - |       40390 |         - |          NA |

## 💡 Usage

Provide a quick example of how to use your code or run the application:

```csharp
var source = new TraceActivitySourceBuilder()
    // set your resources.
    .SetResources(new MyDefaultResources())
    // add your resources.
    .AddResources(new MyAdditionalResources())
    // add intercept in your impl.
    .AddInterceptor(new MyInterceptor())
    // set batch processing for all exporters. 
    .SetBatchExportOptions(new BatchExportOptions())
    // add export in OTLP (HTTP/1.1 + Protobuf)
    .AddOtlpExporter(new HttpExportParameters())
    // add export in your impl.
    .AddExporter(new MyExport())
    .Build("MySource");

using (var scope = source.Start()) 
{
    scope?.SetAttribute("tag_string", AttributeSymbols);
    scope?.SetAttribute("tag_double", 1.123456789);
    scope?.SetAttribute("tag_long", 123456789);
    // your section for measurement.
}
```

## Authors

Contributors names and contact info

[@bugRanger](https://github.com/bugRanger)

## 🤝 Contributing

Contributions are welcome. Please fork the repository, create a feature branch, and submit a pull request.

## 💖 Acknowledgments

* A huge thanks to **[NetCoreServer](https://github.com/chronoxor/NetCoreServer)**, their library helped me achieve 
  maximum export efficiency 👍

## 📄 License

This project is licensed under the [MIT License](LICENSE)


