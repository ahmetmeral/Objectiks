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
        /// <summary>
        /// Sabit alan cache için kullanılacak
        /// </summary>
        public string TypeOf { get; set; }
        /// <summary>
        /// Sabit alan cache için kullanılacak
        /// </summary>
        public string ParseOf { get; set; }
        /// <summary>
        /// Identity Property
        /// </summary>
        public string PrimaryOf { get; set; }

        public string WorkOf { get; set; }

        public string UserOf { get; set; }
        /// <summary>
        /// Unique Keys..
        /// </summary>
        public DocumentKeyOfNames KeyOf { get; set; }
        /// <summary>
        /// Nesnenin ilişkilerini saklar
        /// </summary>
        public DocumentRefs Refs { get; set; }
        /// <summary>
        /// Nesnenin cache de nasıl saklanacağını belirler..
        /// Çok sık erişilen nesneleri JObject olarak saklamak daha mantıklı gibi.
        /// </summary>
        public DocumentCacheInfo Cache { get; set; }
        /// <summary>
        /// custom variables..
        /// </summary>
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
