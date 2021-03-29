using Objectiks.Extentions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Models
{
    public class DocumentRefs : List<DocumentRef>
    {

    }

    public class DocumentRef
    {
        public int Index { get; set; }
        public string ParseOf { get; set; }
        public string TypeOf { get; set; }
        public DocumentKeyOfRef KeyOf { get; set; }
        public DocumentMapOfRef MapOf { get; set; }
        public bool Lazy { get; set; }
        public bool Disabled { get; set; }

        public DocumentRef() { }

        public static DocumentRef FromObject(object obj)
        {
            return obj.ConvertToType<DocumentRef>();
        }
    }

    public class DocumentKeyOfRef
    {
        public List<string> Source { get; set; }
        public List<string> Target { get; set; }
        public bool Any { get; set; }
    }

    public class DocumentMapOfRef
    {
        public string Source { get; set; }
        public string Target { get; set; }
    }
}
