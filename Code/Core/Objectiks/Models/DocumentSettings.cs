using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Models
{
    public class DocumentSettings
    {
        public bool PropertyOverride { get; set; } = true;
        public bool StoragePartial { get; set; } = false;
        public int StoragePartialLimit { get; set; } = 10000;
    }
}
