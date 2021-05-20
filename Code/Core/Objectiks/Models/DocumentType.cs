using Objectiks.Attributes;
using Objectiks.Extentions;
using System;
using System.Collections.Generic;
using System.Reflection;
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

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static DocumentType Default()
        {
            var type = new DocumentType();
            type.PrimaryOf = DocumentDefaults.DocumentPrimaryOf;
            return type;
        }

        public static DocumentType FromClass<T>() where T : class
        {
            var documentType = new DocumentType();

            var type = typeof(T);
            var properties = type.FindProperties();

            var typeOf = type.GetCustomAttribute<TypeOfAttribute>();

            if (typeOf == null)
            {
                documentType.TypeOf = type.Name;
            }
            else
            {
                documentType.TypeOf = typeOf.Name;

                if (String.IsNullOrWhiteSpace(documentType.TypeOf))
                {
                    documentType.TypeOf = type.Name;
                }
            }

            var cacheOf = type.GetCustomAttribute<CacheOfAttribute>();
            if (cacheOf != null)
            {
                documentType.Cache = new DocumentCacheInfo
                {
                    Expire = cacheOf.Expire,
                    Lazy = cacheOf.Lazy
                };
            }
            else
            {
                documentType.Cache = new DocumentCacheInfo
                {
                    Expire = 1000,
                    Lazy = false
                };
            }

            var parseOf = type.GetCustomAttribute<ParseOfAttribute>();
            if (parseOf != null)
            {
                documentType.ParseOf = parseOf.Name;
            }

            foreach (PropertyInfo property in properties)
            {
                var primary = property.GetAttribute<PrimaryAttribute>();
                if (primary != null)
                {
                    documentType.PrimaryOf = property.Name;
                }

                var workOf = property.GetAttribute<WorkOfAttribute>();
                if (workOf != null)
                {
                    documentType.WorkOf = property.Name;
                }

                var userOf = property.GetAttribute<UserOfAttribute>();
                if (userOf != null)
                {
                    documentType.UserOf = property.Name;
                }

                var keyOf = property.GetAttribute<KeyOfAttribute>();
                if (keyOf != null)
                {
                    documentType.KeyOf.Add(property.Name);
                }
            }

            return documentType;
        }
    }
}
