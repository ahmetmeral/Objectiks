using Objectiks.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Benchmark.Models
{
    [Serializable]
    [TypeOf("Pages")]
    public class Pages
    {
        [Primary]
        public int Id { get; set; }

        [KeyOf]
        public string Name { get; set; }

        [KeyOf]
        public string Language { get; set; }

        [Requried]
        public string Title { get; set; }

        [Requried]
        public string FileName { get; set; }

        public int GroupRef { get; set; }

        [Ignore]
        public List<Sections> Sections { get; set; }
    }
}
