![Alt text](./images/logo.png)

[![build](https://github.com/bugRanger/EasyTrace/actions/workflows/ci.yml/badge.svg)](https://github.com/bugRanger/EasyTrace/actions/workflows/ci.yml)

# EasyTrace

A lightweight project for tracing high-load systems.

## Description

The goal of the repository is to offer an
alternative [System.Diagnostics.Activity](https://learn.microsoft.com/ru-ru/dotnet/api/system.diagnostics.activity?view=net-9.0)
that is less expensive in terms of RAM and garbage collection load.

### Benchmarks

```

BenchmarkDotNet v0.15.8, Windows 10 (10.0.19045.6456/22H2/2022Update)
AMD Ryzen 3 2200G with Radeon Vega Graphics 3.50GHz, 1 CPU, 4 logical and 4 physical cores
.NET SDK 10.0.302
  [Host]     : .NET 9.0.14 (9.0.14, 9.0.1426.11910), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 9.0.14 (9.0.14, 9.0.1426.11910), X64 RyuJIT x86-64-v3

```

| Method             | Iterations | ParallelLimit | IsExporter |            Mean |          Error |         StdDev |           Ratio | RatioSD |          Gen0 |       Allocated |     Alloc Ratio |
|--------------------|------------|---------------|------------|----------------:|---------------:|---------------:|----------------:|--------:|--------------:|----------------:|----------------:|
| **ActivitySource** | **1000**   | **4**         | **False**  | **1,780.34 μs** |  **31.973 μs** |  **28.343 μs** |    **baseline** |    **** | **2345.7031** |  **4752.46 KB** |            **** |
| TraceActivityScope | 1000       | 4             | False      |        11.83 μs |       0.208 μs |       0.194 μs | 150.554x faster |   3.31x |        1.0681 |         2.16 KB | 2,205.036x less |
|                    |            |               |            |                 |                |                |                 |         |               |                 |                 |
| **ActivitySource** | **1000**   | **4**         | **True**   | **1,899.03 μs** |  **37.965 μs** |  **43.720 μs** |    **baseline** |    **** | **2343.7500** |  **4752.46 KB** |            **** |
| TraceActivityScope | 1000       | 4             | True       |     1,839.84 μs |      35.923 μs |      53.769 μs |    1.03x faster |   0.04x |             - |         3.73 KB | 1,273.290x less |
|                    |            |               |            |                 |                |                |                 |         |               |                 |                 |
| **ActivitySource** | **1000**   | **8**         | **False**  | **3,714.61 μs** |  **72.337 μs** |  **96.568 μs** |    **baseline** |    **** | **4683.5938** |   **9502.8 KB** |            **** |
| TraceActivityScope | 1000       | 8             | False      |        17.41 μs |       0.246 μs |       0.205 μs | 213.402x faster |   5.95x |        1.1597 |         2.35 KB | 4,041.059x less |
|                    |            |               |            |                 |                |                |                 |         |               |                 |                 |
| **ActivitySource** | **1000**   | **8**         | **True**   | **4,131.53 μs** |  **81.874 μs** | **114.776 μs** |    **baseline** |    **** | **4679.6875** |   **9502.7 KB** |            **** |
| TraceActivityScope | 1000       | 8             | True       |     3,327.28 μs |      64.202 μs |      81.195 μs |    1.24x faster |   0.05x |             - |         4.53 KB | 2,098.958x less |
|                    |            |               |            |                 |                |                |                 |         |               |                 |                 |
| **ActivitySource** | **1000**   | **16**        | **False**  | **7,640.13 μs** | **115.598 μs** | **102.475 μs** |    **baseline** |    **** | **9367.1875** | **19003.47 KB** |            **** |
| TraceActivityScope | 1000       | 16            | False      |        26.51 μs |       0.521 μs |       0.913 μs | 288.534x faster |  10.53x |        1.2817 |         2.64 KB | 7,209.911x less |
|                    |            |               |            |                 |                |                |                 |         |               |                 |                 |
| **ActivitySource** | **1000**   | **16**        | **True**   | **8,312.79 μs** | **161.504 μs** | **198.342 μs** |    **baseline** |    **** | **9359.3750** | **19003.05 KB** |            **** |
| TraceActivityScope | 1000       | 16            | True       |     6,747.51 μs |     114.312 μs |     106.928 μs |    1.23x faster |   0.03x |             - |         6.88 KB | 2,762.510x less |

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


