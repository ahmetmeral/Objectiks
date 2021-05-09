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

    public class DocumentTypes : List<DocumentType>
    {
        public DocumentTypes() : base() { }

        public DocumentTypes(params DocumentType[] types)
        {
            foreach (var item in types)
            {
                if (String.IsNullOrWhiteSpace(item.ParseOf))
                {
                    item.ParseOf = "Document";
                }

                this.Add(item);
            }
        }
    }

    public class DocumentType : IDisposable
    {
        public string TypeOf { get; set; }
        public string ParseOf { get; set; } = "Document";
        public string WorkOf { get; set; }
        public string UserOf { get; set; }
        public string PrimaryOf { get; set; } = DocumentDefaults.DocumentPrimaryOf;
        public DocumentKeyOfNames KeyOf { get; set; } = new DocumentKeyOfNames();
        public DocumentCacheInfo Cache { get; set; } = new DocumentCacheInfo();

        public DocumentType() { }

        public DocumentType(string typeOf)
        {
            TypeOf = typeOf;
        }

        public static DocumentType Default()
        {
            return new DocumentType();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
