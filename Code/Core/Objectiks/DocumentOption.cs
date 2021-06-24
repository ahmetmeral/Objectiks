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
using Microsoft.Extensions.Logging;

namespace Objectiks
{
    public class DocumentOption
    {
        public string Name { get; set; } = "Objectiks";

        internal IDocumentCache CacheInstance { get; set; }
        internal IDocumentWatcher DocumentWatcher { get; set; }
        internal IDocumentLogger DocumentLogger { get; set; }
        internal List<IDocumentParser> ParserOfTypes { get; set; }
        internal Type EngineProvider { get; set; }
        internal bool HasManifest { get; set; }

        public DocumentTypes TypeOf { get; set; } = new DocumentTypes();
        public DocumentVars Vars { get; set; } = new DocumentVars();

        public string SqlProviderSchema { get; set; }
        public string SqlProviderSchemaSeperator { get; set; } = ".";
        public int SqlProviderDataReaderPageSize { get; set; } = 2;
        public bool SupportSqlDataReaderPaging { get; set; } = true;

        public int SupportPartialStorageSize { get; set; } = 1000;
        public bool SupportPartialStorage { get; set; } = false;
        public bool SupportDocumentParser { get; set; } = false;
        public bool SupportDocumentWatcher { get; set; } = false;

        public int BufferSize { get; set; } = 512;
        public long? MemorySize { get; set; } = null;
        public string Extention { get; set; } = "*.json";


        public DocumentOption()
        {
            ParserOfTypes = new List<IDocumentParser>();
        }

        public virtual void RegisterDefaults()
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
        /// <summary>
        /// Development environment must be active. Creates errors in simultaneous operations in a live environment.
        /// </summary>
        /// <typeparam name="T"></typeparam>
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

        public void RegisterParseOf<T>() where T : IDocumentParser
        {
            ParserOfTypes.Add((IDocumentParser)Activator.CreateInstance(typeof(T)));
        }

        public void RegisterTypeOf<T>() where T : class
        {
            TypeOf.Add(DocumentType.FromClass<T>());
        }

        public void ClearParserOfTypes()
        {
            ParserOfTypes?.Clear();
        }
    }
}
