using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.CsProj;
using BenchmarkDotNet.Validators;
using System;
using System.IO;

namespace Objectiks.Benchmark
{
    class Program
    {
        public static void Main(string[] args)
        {

            var path = Path.Combine(Directory.GetCurrentDirectory(), "Benchmarks");

            var configs = DefaultConfig.Instance
                .WithArtifactsPath(path)
                .WithOption(ConfigOptions.DisableOptimizationsValidator, true)
                .AddJob(Job.Default.WithRuntime(CoreRuntime.Core31))
                .AddValidator(ExecutionValidator.FailOnError)
                ;

            BenchmarkRunner.Run(typeof(Program).Assembly, configs);


            Console.ReadLine();
        }
        //[Obsolete]
        //static void Main(string[] args)
        //{
        //    BenchmarkRunner.Run(typeof(Program).Assembly, 
        //       DefaultConfig.Instance
        //     .With(Job.Default.With(CoreRuntime.Core31)
        //     .With(Jit.RyuJit)
        //     .With(CsProjCoreToolchain.NetCoreApp31)
        //     .WithGcForce(true))
        // .With(MemoryDiagnoser.Default)
        // .With(BenchmarkReportExporter.Default, HtmlExporter.Default, MarkdownExporter.GitHub)
        // .KeepBenchmarkFiles());
        //}
    }
}
