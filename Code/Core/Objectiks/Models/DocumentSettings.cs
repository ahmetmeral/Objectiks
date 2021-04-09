using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Models
{
    public class DocumentSettings
    {
        public string Extention { get; set; } = "*.json";
        public bool Watcher { get; set; } = false;
        public int BufferSize { get; set; }
        public DocumentParserSettings Parser { get; set; }
        public DocumentStorageSettings Storage { get; set; }
        public DocumentCacheInfo Cache { get; set; }
    }

    public class DocumentStorageSettings
    {
        public bool Partial { get; set; } = false;
        public int Limit { get; set; } = 1000;
    }

    public class DocumentParserSettings
    {
        public bool PropertyOverride { get; set; } = true;
    }
}
