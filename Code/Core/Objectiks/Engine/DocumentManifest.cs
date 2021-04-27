using Objectiks.Helper;
using Objectiks.Models;
using Objectiks.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Objectiks.Engine
{
    public class DocumentManifest : DocumentOption
    {
        //public string Name { get; set; }
        //public string Version { get; set; }
        //public string Author { get; set; }
        //public string Primary { get; set; }
        //public DocumentKeyOfNames KeyOf { get; set; }
        //public DocumentTypes TypeOf { get; set; }
        //public DocumentSetting Documents { get; set; }


        public bool Empty { get; set; }

        public DocumentManifest() { }

        public static DocumentManifest Get(string path)
        {
            try
            {
                var manifest = new JSONSerializer().Get<DocumentManifest>(path);

                if (manifest.TypeOf == null)
                {
                    manifest.TypeOf = new DocumentTypes();
                }

                if (manifest.KeyOf == null)
                {
                    manifest.KeyOf = new DocumentKeyOfNames();
                }

                if (manifest.CacheInfo == null)
                {
                    manifest.CacheInfo = new DocumentCacheInfo
                    {
                        Expire = DocumentDefaults.CacheExpire
                    };
                }

                if (manifest.SupportDocumentWatcher)
                {
                    manifest.UseDocumentWatcher<DocumentWatcher>();
                }

                return manifest;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
