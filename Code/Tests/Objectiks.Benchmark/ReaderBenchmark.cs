using BenchmarkDotNet.Attributes;
using Objectiks.Benchmark.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Benchmark
{
    [BenchmarkCategory(Constants.Categories.READER)]
    public class ReaderBenchmark : BenchmarkBase
    {
        [GlobalSetup]
        public void GlobalSetup()
        {
            Options = Setup.Options;
        }

        [Benchmark(Baseline = true)]
        public void DocumentParse()
        {
            Repo = new ObjectiksOf(Options);
        }

        [Benchmark]
        public void GetTypeMetaAll()
        {
            var meta = Repo.GetTypeMetaAll();
        }

        [Benchmark]
        public void TypeOfToList()
        {
            var meta = Repo.GetTypeMeta<Pages>();
            var pages = Repo.TypeOf<Pages>().ToList();
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {

        }
    }
}
