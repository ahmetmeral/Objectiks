using Objectiks.Caching;
using Objectiks.Engine;
using Objectiks.Models;
using Objectiks.Parsers;
using Objectiks.Caching.Serializer;
using Objectiks.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Objectiks
{
    public class DocumentOption
    {
        public string Name { get; set; } = "Objectiks";
        public string Primary { get; set; } = "Id";
        public string User { get; set; } = "UserOf";
        public string WorkOf { get; set; } = "WorkOf";
        public string ParseOf { get; set; } = "Document";

        public string SqlProviderSchema { get; set; }
        public string SqlProviderSchemaSeperator { get; set; } = ".";
        public int SqlProviderDataReaderPageSize { get; set; } = 2;
        public bool SupportSqlDataReaderPaging { get; set; } = true;

        public int SupportPartialStorageSize { get; set; } = 1000;
        public bool SupportPartialStorage { get; set; } = false;
        public bool SupportDocumentParser { get; set; } = false;
        public bool SupportDocumentWatcher { get; set; } = false;

        public int BufferSize { get; set; } = 512;
        public string Extention { get; set; } = "*.json";

        public DocumentKeyOfNames KeyOf { get; set; } = new DocumentKeyOfNames();
        public DocumentTypes TypeOf { get; set; } = new DocumentTypes();
        public DocumentCacheInfo CacheInfo { get; set; } = new DocumentCacheInfo { Expire = 10000 };
        public DocumentSchemes Schemes { get; set; } = new DocumentSchemes();
        public DocumentVars Vars { get; set; } = new DocumentVars();

        internal IDocumentCache CacheInstance { get; set; }
        internal IDocumentWatcher DocumentWatcher { get; set; }
        internal IDocumentLogger DocumentLogger { get; set; }
        internal List<IDocumentParser> ParserOfTypes { get; set; }
        internal Type EngineProvider { get; set; }
        internal bool HasManifest { get; set; }


        public DocumentOption()
        {
            ParserOfTypes = new List<IDocumentParser>();
        }

        public virtual void RegisterDefaultTypeOrParser()
        {
            if (CacheInstance == null)
            {
                CacheInstance = new DocumentInMemory(Name, new DocumentBsonSerializer());
            }
        }

        public void UseCacheProvider(DocumentCache documentCache)
        {
            CacheInstance = documentCache;
        }

        public void UseEngineProvider<T>() where T : IDocumentEngine
        {
            EngineProvider = typeof(T);
        }

        public void UseDocumentWatcher<T>() where T : IDocumentWatcher
        {
            SupportDocumentWatcher = true;
            DocumentWatcher = (DocumentWatcher)Activator.CreateInstance(typeof(T));
        }

        public void UseDocumentLogger<T>() where T : IDocumentLogger
        {
            DocumentLogger = (IDocumentLogger)Activator.CreateInstance(typeof(T));
        }

        public void UseManifestFile()
        {
            HasManifest = true;
        }

        public void AddParserTypeOf<T>() where T : IDocumentParser
        {
            ParserOfTypes.Add((IDocumentParser)Activator.CreateInstance(typeof(T)));
        }

        public void ClearParserOfTypes()
        {
            ParserOfTypes?.Clear();
        }
    }
}
