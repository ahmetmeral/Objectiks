using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Models
{
    public class DocumentTypes : List<string>
    {
        public DocumentTypes() : base() { }

        public DocumentTypes(params string[] typeOfNames)
        {
            this.AddRange(typeOfNames);
        }
    }
}
