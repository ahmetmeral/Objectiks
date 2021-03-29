``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.18363.1440 (1909/November2018Update/19H2)
Intel Core i7-6500U CPU 2.50GHz (Skylake), 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=5.0.200
  [Host] : .NET Core 3.1.12 (CoreCLR 4.700.21.6504, CoreFX 4.700.21.6905), X64 RyuJIT


```
|             Method | Size | Partition | BufferSize | Mean | Error | Ratio | RatioSD |
|------------------- |----- |---------- |----------- |-----:|------:|------:|--------:|
| **JsonFileCreateRows** |   **10** |         **1** |        **128** |   **NA** |    **NA** |     **?** |       **?** |
| JsonFileAppendRows |   10 |         1 |        128 |   NA |    NA |     ? |       ? |
|                    |      |           |            |      |       |       |         |
| **JsonFileCreateRows** |   **10** |         **1** |        **256** |   **NA** |    **NA** |     **?** |       **?** |
| JsonFileAppendRows |   10 |         1 |        256 |   NA |    NA |     ? |       ? |
|                    |      |           |            |      |       |       |         |
| **JsonFileCreateRows** |   **10** |         **1** |        **512** |   **NA** |    **NA** |     **?** |       **?** |
| JsonFileAppendRows |   10 |         1 |        512 |   NA |    NA |     ? |       ? |
|                    |      |           |            |      |       |       |         |
| **JsonFileCreateRows** |   **10** |         **2** |        **128** |   **NA** |    **NA** |     **?** |       **?** |
| JsonFileAppendRows |   10 |         2 |        128 |   NA |    NA |     ? |       ? |
|                    |      |           |            |      |       |       |         |
| **JsonFileCreateRows** |   **10** |         **2** |        **256** |   **NA** |    **NA** |     **?** |       **?** |
| JsonFileAppendRows |   10 |         2 |        256 |   NA |    NA |     ? |       ? |
|                    |      |           |            |      |       |       |         |
| **JsonFileCreateRows** |   **10** |         **2** |        **512** |   **NA** |    **NA** |     **?** |       **?** |
| JsonFileAppendRows |   10 |         2 |        512 |   NA |    NA |     ? |       ? |
|                    |      |           |            |      |       |       |         |
| **JsonFileCreateRows** |   **10** |         **3** |        **128** |   **NA** |    **NA** |     **?** |       **?** |
| JsonFileAppendRows |   10 |         3 |        128 |   NA |    NA |     ? |       ? |
|                    |      |           |            |      |       |       |         |
| **JsonFileCreateRows** |   **10** |         **3** |        **256** |   **NA** |    **NA** |     **?** |       **?** |
| JsonFileAppendRows |   10 |         3 |        256 |   NA |    NA |     ? |       ? |
|                    |      |           |            |      |       |       |         |
| **JsonFileCreateRows** |   **10** |         **3** |        **512** |   **NA** |    **NA** |     **?** |       **?** |
| JsonFileAppendRows |   10 |         3 |        512 |   NA |    NA |     ? |       ? |

Benchmarks with issues:
  SerializerBenchmark.JsonFileCreateRows: DefaultJob [Size=10, Partition=1, BufferSize=128]
  SerializerBenchmark.JsonFileAppendRows: DefaultJob [Size=10, Partition=1, BufferSize=128]
  SerializerBenchmark.JsonFileCreateRows: DefaultJob [Size=10, Partition=1, BufferSize=256]
  SerializerBenchmark.JsonFileAppendRows: DefaultJob [Size=10, Partition=1, BufferSize=256]
  SerializerBenchmark.JsonFileCreateRows: DefaultJob [Size=10, Partition=1, BufferSize=512]
  SerializerBenchmark.JsonFileAppendRows: DefaultJob [Size=10, Partition=1, BufferSize=512]
  SerializerBenchmark.JsonFileCreateRows: DefaultJob [Size=10, Partition=2, BufferSize=128]
  SerializerBenchmark.JsonFileAppendRows: DefaultJob [Size=10, Partition=2, BufferSize=128]
  SerializerBenchmark.JsonFileCreateRows: DefaultJob [Size=10, Partition=2, BufferSize=256]
  SerializerBenchmark.JsonFileAppendRows: DefaultJob [Size=10, Partition=2, BufferSize=256]
  SerializerBenchmark.JsonFileCreateRows: DefaultJob [Size=10, Partition=2, BufferSize=512]
  SerializerBenchmark.JsonFileAppendRows: DefaultJob [Size=10, Partition=2, BufferSize=512]
  SerializerBenchmark.JsonFileCreateRows: DefaultJob [Size=10, Partition=3, BufferSize=128]
  SerializerBenchmark.JsonFileAppendRows: DefaultJob [Size=10, Partition=3, BufferSize=128]
  SerializerBenchmark.JsonFileCreateRows: DefaultJob [Size=10, Partition=3, BufferSize=256]
  SerializerBenchmark.JsonFileAppendRows: DefaultJob [Size=10, Partition=3, BufferSize=256]
  SerializerBenchmark.JsonFileCreateRows: DefaultJob [Size=10, Partition=3, BufferSize=512]
  SerializerBenchmark.JsonFileAppendRows: DefaultJob [Size=10, Partition=3, BufferSize=512]
