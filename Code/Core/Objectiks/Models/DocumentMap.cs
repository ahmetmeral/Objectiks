using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Models
{
    public class DocumentMap
    {
        public string Source { get; set; }
        public string Target { get; set; }

        public DocumentMap() { }

        public DocumentMap(string source, string target)
        {
            Source = source;
            Target = target;
        }
    }
}
