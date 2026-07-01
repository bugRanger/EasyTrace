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


| Method             | Iterations | ParallelLimit | IsExporter | Mean         | Error      | StdDev     | Median       | Gen0      | Gen1    | Allocated   |
|------------------- |----------- |-------------- |----------- |-------------:|-----------:|-----------:|-------------:|----------:|--------:|------------:|
| ActivitySource     | 1000       | 4             | False      |    45.394 us |  0.9058 us |  2.3054 us |    45.285 us |    0.2441 |       - |     2.46 KB |
| TraceActivityScope | 1000       | 4             | False      |     5.169 us |  0.0343 us |  0.0320 us |     5.181 us |    0.3052 |       - |      2.5 KB |
| ActivitySource     | 1000       | 4             | True       |   698.665 us |  6.8543 us |  6.4115 us |   698.552 us |  588.8672 |  8.7891 |  4752.48 KB |
| TraceActivityScope | 1000       | 4             | True       |   349.706 us |  6.9106 us | 15.8783 us |   341.561 us |         - |       - |     2.46 KB |
| ActivitySource     | 10000      | 4             | False      |   377.078 us |  7.3748 us |  9.8451 us |   374.799 us |         - |       - |     2.46 KB |
| TraceActivityScope | 10000      | 4             | False      |    25.074 us |  0.4772 us |  0.4230 us |    25.096 us |    0.2747 |       - |     2.45 KB |
| ActivitySource     | 10000      | 4             | True       | 7,855.249 us | 74.1372 us | 69.3480 us | 7,867.755 us | 5882.8125 | 46.8750 | 47502.54 KB |
| TraceActivityScope | 10000      | 4             | True       | 3,491.807 us | 18.0259 us | 16.8614 us | 3,494.504 us |         - |       - |     2.52 KB |
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


