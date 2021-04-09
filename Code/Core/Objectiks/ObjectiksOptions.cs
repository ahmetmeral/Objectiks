using Objectiks.Caching;
using Objectiks.Engine;
using Objectiks.Models;
using Objectiks.Parsers;
using Objectiks.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Objectiks
{
    public class ObjectiksOptions
    {
        internal Type Cache { get; private set; }
        internal Type DefaultProvider { get; private set; }
        internal Type Watcher { get; private set; }
        internal Type Logger { get; private set; }
        internal List<Type> ParserOf { get; private set; }
        internal Dictionary<string, Type> ProviderOf { get; set; }
        internal string ManifestFilePath { get; set; }
        internal string BaseDirectory { get; set; }


        public ObjectiksOptions()
        {
            RegisterDefaultTypeOrParser(string.Empty);
        }

        public ObjectiksOptions(string baseDirectory)
        {
            RegisterDefaultTypeOrParser(baseDirectory);
        }

        private void RegisterDefaultTypeOrParser(string baseDirectory)
        {
            BaseDirectory = baseDirectory;
            ParserOf = new List<Type>();
            ProviderOf = new Dictionary<string, Type>();

            AddManifestFile(Path.Combine(Directory.GetCurrentDirectory(), DocumentDefaults.Root, DocumentDefaults.Manifest));
            AddProviderOf<DocumentProvider>("default");

            UseCacheTypeOf<DocumentInMemory>();
            UseWatcher<DocumentWatcher>();

            AddParserOf<DocumentDefaultParser>();
            AddParserOf<DocumentOneToOneParser>();
            AddParserOf<DocumentManyToManyParser>();
            AddParserOf<DocumentOneToManyParser>();
            AddParserOf<DocumentOneToOneFileParser>();
        }

        public void UseCacheTypeOf<T>() where T : IDocumentCache
        {
            Cache = typeof(T);
        }

        public void UseWatcher<T>() where T : IDocumentWatcher
        {
            Watcher = typeof(T);
        }

        public void UseDocumentLogger<T>() where T : IDocumentLogger
        {
            Logger = typeof(T);
        }

        public void AddProviderOf<T>(string providerOfSettingsName) where T : IDocumentProvider
        {
            ProviderOf.Add(providerOfSettingsName, typeof(T));
        }

        public void AddParserOf<T>() where T : IParser
        {
            ParserOf.Add(typeof(T));
        }

        public void AddManifestFile(string path)
        {
            ManifestFilePath = path;
        }

        public void ClearParserOf()
        {
            ParserOf?.Clear();
        }

        public void ClearProviderOf()
        {
            ProviderOf?.Clear();
        }
    }
}
