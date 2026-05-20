```

BenchmarkDotNet v0.15.8, Windows 10 (10.0.19045.6456/22H2/2022Update)
AMD Ryzen 7 5700G with Radeon Graphics 3.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.103
  [Host]     : .NET 9.0.13 (9.0.13, 9.0.1326.6317), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 9.0.13 (9.0.13, 9.0.1326.6317), X64 RyuJIT x86-64-v3


```
| Method   | Iterations | IsExporter | Mean      | Error     | StdDev    | Gen0     | Allocated |
|--------- |----------- |----------- |----------:|----------:|----------:|---------:|----------:|
| **Activity** | **1000**       | **False**      |  **4.410 ms** | **0.0074 ms** | **0.0057 ms** |        **-** |      **32 B** |
| **Activity** | **1000**       | **True**       |  **4.781 ms** | **0.0364 ms** | **0.0340 ms** |  **93.7500** |  **816032 B** |
| **Activity** | **10000**      | **False**      | **44.042 ms** | **0.0401 ms** | **0.0335 ms** |        **-** |      **32 B** |
| **Activity** | **10000**      | **True**       | **47.702 ms** | **0.1304 ms** | **0.1156 ms** | **909.0909** | **8160032 B** |
