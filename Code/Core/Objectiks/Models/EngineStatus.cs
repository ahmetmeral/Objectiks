using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Models
{
    public class EngineStatus
    {
        public string EngineOf { get; set; }
        public bool Loaded { get; set; }

        public EngineStatus() { }

        public EngineStatus(string engineOf, bool loaded)
        {
            EngineOf = engineOf;
            Loaded = loaded;
        }
    }
}
