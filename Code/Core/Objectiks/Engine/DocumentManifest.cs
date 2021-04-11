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
                var manifest = new DocumentSerializer().Get<DocumentManifest>(path);

                if (manifest.TypeOf == null)
                {
                    manifest.TypeOf = new DocumentTypes();
                }

                if (manifest.KeyOf == null)
                {
                    manifest.KeyOf = new DocumentKeyOfNames();
                }

                if (manifest.Cache == null)
                {
                    manifest.Cache = new DocumentCacheInfo
                    {
                        Expire = DocumentDefaults.CacheExpire
                    };
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
