using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Benchmark
{
    public class BenchmarkBase
    {
        [Params(10)]
        public int Size { get; set; }

        [Params(1, 2, 3)]
        public int Partition { get; set; }

        [Params(128, 256, 512)]
        public int BufferSize { get; set; }

        public DocumentOptions Options { get; set; }

        public ObjectiksOf Repo { get; set; }
    }
}
