using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Models
{
    public class DocumentInfo
    {
        public string TypeOf { get; set; }
        public object PrimaryOf { get; set; }
        public int Partition { get; set; }

        [JsonIgnore]
        public bool Exists { get; set; }

        public DocumentInfo() { }

        public DocumentInfo(string typeOf)
        {
            TypeOf = typeOf;
        }

        public DocumentInfo(Document document)
        {
            TypeOf = document.TypeOf;
            PrimaryOf = document.PrimaryOf;
            Partition = document.Partition;
            Exists = document.Exists;
        }
    }
}
