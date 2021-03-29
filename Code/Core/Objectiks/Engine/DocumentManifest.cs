using Objectiks.Models;
using Objectiks.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Objectiks.Engine
{
    public class DocumentManifest
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Author { get; set; }
        public string Primary { get; set; }
        public int BufferSize { get; set; }
        public DocumentSchemaKeys KeyOf { get; set; }
        public DocumentTypes TypeOf { get; set; }
        public ExtentionNames Extention { get; set; }
        public DocumentCacheInfo Cache { get; set; }
        public DocumentSettings Documents { get; set; }
        public DocumentVars Vars { get; set; }

        public bool Empty { get; set; }

        public DocumentManifest() { }

        public static DocumentManifest Get(IDocumentConnection connection)
        {
            try
            {
                var path = Path.Combine(connection.BaseDirectory, DocumentDefaults.Manifest);

                var manifest = new DocumentSerializer().Get<DocumentManifest>(path);

                if (manifest.Cache == null)
                {
                    manifest.Cache = new DocumentCacheInfo
                    {
                        Expire = DocumentDefaults.CacheExpire
                    };
                }

                if (manifest.KeyOf == null)
                {
                    manifest.KeyOf = new DocumentSchemaKeys();
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
