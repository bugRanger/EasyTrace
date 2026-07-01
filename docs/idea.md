* **Author:** [@bugRanger](https://github.com/bugRanger)
* **Status:** Done
* **Date:** 20.05.2026

## Motivation

The motivation for writing this project was the problems with the load on the garbage collector.

A turnkey solution based on [OpenTelemetry .NET](https://github.com/open-telemetry/opentelemetry-dotnet) places a
significant burden on the garbage collector.
In high-load APIs with tens and hundreds of thousands of microrequests per second, memory allocation for objects is
constantly occurring. This activity generates a huge flow of short-lived data.
This triggers partial garbage collections in Gen 0, unnecessarily consumes CPU resources, and distorts metric results.

## Solution

To reduce the load on the GC, it's necessary to reuse trace objects.
However,
the [System.Diagnostics.Activity](https://learn.microsoft.com/ru-ru/dotnet/api/system.diagnostics.activity?view=net-9.0)
class doesn't support state resets, so this causes problems.
This also makes it impossible to use OpenTelemetry .NET for data export: the library is tightly coupled
to the standard Activity and doesn't provide an API for regional custom alternatives.

### Goals

1. Write an alternative activity that stores it in a pool;
2. Write exporters for activity (Jaeger, etc.).

## Alternatives

There are no other alternatives on the .NET platform.

## Risks and Limitations

- **Time and Budget:** Developing, debugging, and maintaining your own solution requires significant resources.
- **Lag:** Ready-made solutions are updated by developers (adding new standards, fixing bugs). Your own solution will
  have to be developed and updated manually.