using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Attributes
{
    public class DocumentAttributes
    {
        public string TypeOf { get; set; }
        public string CacheOf { get; set; }
        public string WorkOf { get; set; }
        public string UserOf { get; set; }
        public string PrimaryOf { get; set; }
        public int Partition { get; set; }
        public List<string> KeyOfValues { get; set; }
        public List<IgnoreAttribute> Ignored { get; set; }
        public bool Exists { get; set; }

        public DocumentAttributes()
        {
            KeyOfValues = new List<string>();
            Ignored = new List<IgnoreAttribute>();
        }
    }
}
