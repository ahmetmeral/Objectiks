using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Models
{
    public class DocumentKeyOfNames : List<string>
    {
        public DocumentKeyOfNames() : base() { }

        public DocumentKeyOfNames(params string[] keyOfNames)
        {
            this.AddRange(keyOfNames);
        }
    }

    public class DocumentSchemes : List<DocumentSchema>
    {
        public DocumentSchemes() : base() { }

        public DocumentSchemes(params DocumentSchema[] schemes)
        {
            foreach (var item in schemes)
            {
                if (String.IsNullOrWhiteSpace(item.ParseOf))
                {
                    item.ParseOf = "Document";
                }

                this.Add(item);
            }
        }
    }

    public class DocumentSchema : IDisposable
    {
        public string TypeOf { get; set; }
        public string ParseOf { get; set; }
        public string WorkOf { get; set; }
        public string UserOf { get; set; }
        public string PrimaryOf { get; set; }
        public DocumentKeyOfNames KeyOf { get; set; }
        public DocumentCacheInfo Cache { get; set; }
        public DocumentVars Vars { get; set; }

        public static DocumentSchema Default()
        {
            return new DocumentSchema();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
