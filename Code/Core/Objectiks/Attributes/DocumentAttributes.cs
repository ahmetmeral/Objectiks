using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Attributes
{
    public class DocumentAttributes
    {
        public TypeOfAttribute TypeOf { get; set; }
        public PrimaryAttribute Primary { get; set; }
        public List<KeyOfAttribute> KeyOf { get; set; }
        public List<RequriedAttribute> Requried { get; set; }
        public List<IgnoreAttribute> Ignored { get; set; }
        public List<string> KeyOfValues { get; set; }
        public bool IsNew { get; set; }

        public DocumentAttributes(TypeOfAttribute typeOf)
        {
            TypeOf = typeOf;
            KeyOf = new List<KeyOfAttribute>();
            Requried = new List<RequriedAttribute>();
            Ignored = new List<IgnoreAttribute>();
            KeyOfValues = new List<string>();
        }

        public void Add(KeyOfAttribute attr)
        {
            KeyOf.Add(attr);
        }

        public void Add(RequriedAttribute attr)
        {
            Requried.Add(attr);
        }

        public void Add(IgnoreAttribute attr)
        {
            Ignored.Add(attr);
        }
    }
}
