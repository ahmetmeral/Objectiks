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
        public DocumentKeyOfNames KeyOf { get; set; }
        public DocumentTypes TypeOf { get; set; }
        public DocumentProviders ProviderOf { get; set; }
        public DocumentSettings Documents { get; set; }
        public DocumentVars Vars { get; set; }

        public bool Empty { get; set; }

        public DocumentManifest() { }

        public static DocumentManifest Get(string path)
        {
            try
            {
                var manifest = new DocumentSerializer().Get<DocumentManifest>(path);

                if (manifest.Documents == null)
                {
                    manifest.Documents = new DocumentSettings();
                    manifest.Documents.Storage = new DocumentStorageSettings();
                    manifest.Documents.Parser = new DocumentParserSettings();
                }

                if (manifest.Documents.Cache == null)
                {
                    manifest.Documents.Cache = new DocumentCacheInfo
                    {
                        Expire = DocumentDefaults.CacheExpire
                    };
                }

                if (manifest.KeyOf == null)
                {
                    manifest.KeyOf = new DocumentKeyOfNames();
                }

                if (manifest.ProviderOf == null)
                {
                    manifest.ProviderOf = new DocumentProviders();
                    manifest.ProviderOf.Add(DocumentDefaults.ProviderOf, new DocumentOptions
                    {
                        Name = DocumentDefaults.ProviderOf,
                        BaseDirectory = Path.Combine(Directory.GetCurrentDirectory(), DocumentDefaults.Root),
                        BufferSize = DocumentDefaults.BufferSize,
                        Extention = DocumentDefaults.Extention,
                        TypeOf = manifest.TypeOf
                    });
                }
                else if (manifest.ProviderOf.Count == 1 && manifest.ProviderOf.ContainsKey(DocumentDefaults.ProviderOf)
                    && manifest.ProviderOf[DocumentDefaults.ProviderOf].TypeOf == null)
                {
                    manifest.ProviderOf[DocumentDefaults.ProviderOf].TypeOf = manifest.TypeOf;
                }

                foreach (var providerOf in manifest.ProviderOf)
                {
                    if (!String.IsNullOrWhiteSpace(providerOf.Value.Extention))
                    {
                        providerOf.Value.BaseDirectory = Path.Combine(Directory.GetCurrentDirectory(), DocumentDefaults.Root);
                    }
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
