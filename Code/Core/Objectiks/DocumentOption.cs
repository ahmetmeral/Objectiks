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

namespace Objectiks
{
    public class DocumentOption
    {
        public string Name { get; set; } = "Objectiks";
        public string Version { get; set; }
        public string Author { get; set; }
        public string Primary { get; set; } = "Id";
        public string User { get; set; } = "UserRef";
        public string Account { get; set; } = "AccountRef";
        public string ParseOf { get; set; } = "Document";
        public DocumentKeyOfNames KeyOf { get; set; } = new DocumentKeyOfNames();
        public DocumentTypes TypeOf { get; set; }
        public string SqlProviderSchema { get; set; }
        public string SqlProviderSchemaSeperator { get; set; } = ".";
        public int SqlProviderPageLimit { get; set; } = 2;
        public bool SupportSqlDataReaderPaging { get; set; } = true;
        public string Extention { get; set; } = "*.json";
        public int BufferSize { get; set; } = 512;

        public bool SupportPartialStorage { get; set; } = true;
        public int SupportPartialStorageLimit { get; set; } = 1000;
        public bool SupportDocumentParser { get; set; } = false;
        public bool SupportTypeOfRefs { get; set; } = true;
        public bool SupportLoaderInRefs { get; set; } = false;
        public bool SupportFileAppend { get; set; } = true;
        public bool SupportTransaction { get; set; } = true;
        public bool SupportDocumentWatcher { get; set; } = false;
        public bool SupportProperyOverride { get; set; } = true;
        public bool SupportLoaderPaging { get; set; } = false;
        public bool SupportDocumentWriter { get; set; } = true;

        public DocumentCacheInfo CacheInfo { get; set; } = new DocumentCacheInfo { Expire = 10000 };
        public DocumentSchemes Schemes { get; set; } = new DocumentSchemes();
        public DocumentVars Vars { get; set; }

        internal IDocumentCache CacheInstance { get; set; }
        internal IDocumentWatcher DocumentWatcher { get; set; }
        internal IDocumentLogger DocumentLogger { get; set; }

        internal Type EngineProvider { get; private set; }
        internal List<Type> ParserOfTypes { get; private set; }

        public DocumentOption()
        {
            ParserOfTypes = new List<Type>();
        }

        public virtual void RegisterDefaultTypeOrParser()
        {
            if (ParserOfTypes.Count == 0)
            {
                //AddParserTypeOf<DocumentDefaultParser>();
                AddParserTypeOf<DocumentOneToOneParser>();
                AddParserTypeOf<DocumentManyToManyParser>();
                AddParserTypeOf<DocumentOneToManyParser>();
                AddParserTypeOf<DocumentOneToOneFileParser>();
            }

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

        public void AddParserTypeOf<T>() where T : IParser
        {
            ParserOfTypes.Add(typeof(T));
        }

        public void ClearParserOf()
        {
            ParserOfTypes?.Clear();
        }
    }
}
