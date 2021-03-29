using Objectiks.Engine;
using Objectiks.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Models
{
    public class DocumentPartitions : Dictionary<int, int>
    {
        public int Current { get; set; }
        public int Next { get; set; }

    }

    

   
}
