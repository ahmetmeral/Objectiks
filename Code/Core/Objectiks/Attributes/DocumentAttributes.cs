using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Attributes
{
    public class DocumentAttributes
    {
        public string TypeOf { get; set; }
        public string Primary { get; set; }
        public string Account { get; set; }
        public string User { get; set; }
        public List<string> KeyOfValues { get; set; }
        public List<IgnoreAttribute> Ignored { get; set; }

        public DocumentAttributes()
        {
            KeyOfValues = new List<string>();
            Ignored = new List<IgnoreAttribute>();
        }
    }
}
