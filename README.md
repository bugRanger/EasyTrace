![Alt text](./images/logo.png)

[![build](https://github.com/bugRanger/EasyTrace/actions/workflows/ci.yml/badge.svg)](https://github.com/bugRanger/EasyTrace/actions/workflows/ci.yml)

# EasyTrace
A lightweight project for tracing high-load systems.

## Description
The goal of the repository is to offer an alternative [System.Diagnostics.Activity](https://learn.microsoft.com/ru-ru/dotnet/api/system.diagnostics.activity?view=net-9.0) that is less expensive in terms of RAM and garbage collection load.

### Benchmarks
```
BenchmarkDotNet v0.15.8, Windows 10 (10.0.19045.6456/22H2/2022Update)
AMD Ryzen 7 5700G with Radeon Graphics 3.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.103
  [Host]     : .NET 9.0.13 (9.0.13, 9.0.1326.6317), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 9.0.13 (9.0.13, 9.0.1326.6317), X64 RyuJIT x86-64-v3
  
| Method           | Iterations | ParallelLimit | IsExporter | Mean        | Error      | StdDev     | Gen0      | Gen1    | Allocated   |
|----------------- |----------- |-------------- |----------- |------------:|-----------:|-----------:|----------:|--------:|------------:|
| ActivitySource   | 1000       | 4             | False      |    43.92 us |   0.876 us |   1.580 us |    0.3052 |       - |     2.48 KB |
| TraceActivityRef | 1000       | 4             | False      |    15.80 us |   0.295 us |   0.276 us |    0.3052 |       - |     2.47 KB |
| ActivitySource   | 1000       | 4             | True       |   795.88 us |  15.775 us |  24.090 us |  588.8672 |  9.7656 |  4752.54 KB |
| TraceActivityRef | 1000       | 4             | True       |   344.26 us |   4.812 us |   4.501 us |         - |       - |     2.49 KB |
| ActivitySource   | 10000      | 4             | False      |   388.56 us |   7.729 us |  13.333 us |         - |       - |      2.5 KB |
| TraceActivityRef | 10000      | 4             | False      |   138.12 us |   2.759 us |   3.388 us |    0.2441 |       - |     2.49 KB |
| ActivitySource   | 10000      | 4             | True       | 7,750.68 us | 153.829 us | 307.214 us | 5882.8125 | 46.8750 | 47502.54 KB |
| TraceActivityRef | 10000      | 4             | True       | 3,394.63 us |  65.616 us |  67.382 us |         - |       - |     2.52 KB |
```

## 💡 Usage

Provide a quick example of how to use your code or run the application:

```csharp
var source = new TraceActivitySourceBuilder()
    .SetBatchExportOptions(new BatchExportOptions())
    // your impl for export.
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

## 📄 License
This project is licensed under the [MIT License](LICENSE-MIT)


