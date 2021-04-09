using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Models
{
    public class DocumentTypeStatus
    {
        public string TypeOf { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int Tick { get; set; }
        public bool Loaded { get; set; }
    }
}
