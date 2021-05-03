﻿using Objectiks.Caching;
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
        public string Version { get; set; } = "1.0.0";
        public string Primary { get; set; } = "Id";
        public string User { get; set; } = "UserOf";
        public string WorkOf { get; set; } = "WorkOf";
        public string ParseOf { get; set; } = "Document";
        public DocumentKeyOfNames KeyOf { get; set; } = new DocumentKeyOfNames();
        public DocumentTypes TypeOf { get; set; } = new DocumentTypes();
        public string SqlProviderSchema { get; set; }
        public string SqlProviderSchemaSeperator { get; set; } = ".";
        public int SqlProviderDataReaderPageSize { get; set; } = 2;
        public bool SupportSqlDataReaderPaging { get; set; } = true;

        public int SupportPartialStorageSize { get; set; } = 1000;
        public bool SupportPartialStorage { get; set; } = false;
        public bool SupportDocumentParser { get; set; } = false;
        public bool SupportTypeOfRefs { get; set; } = true;
        public bool SupportTypeOfRefsFirstLoad { get; set; } = false;
        public bool SupportDocumentWatcher { get; set; } = false;

        public string Extention { get; set; } = "*.json";
        public int BufferSize { get; set; } = 512;

        public DocumentCacheInfo CacheInfo { get; set; } = new DocumentCacheInfo { Expire = 10000 };
        public DocumentSchemes Schemes { get; set; } = new DocumentSchemes();
        public DocumentVars Vars { get; set; } = new DocumentVars();

        internal IDocumentCache CacheInstance { get; set; }
        internal IDocumentWatcher DocumentWatcher { get; set; }
        internal IDocumentLogger DocumentLogger { get; set; }
        internal List<IParser> ParserOfTypes { get; set; }
        internal Type EngineProvider { get; set; }
        internal bool HasManifest { get; set; }

        internal bool HasTypeOf
        {
            get
            {
                return TypeOf.Count > 0;
            }
        }

        public DocumentOption()
        {
            ParserOfTypes = new List<IParser>();
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

        public void UseManifestFile()
        {
            HasManifest = true;
        }

        public void AddParserTypeOf<T>() where T : IParser
        {
            ParserOfTypes.Add(GetDocumentParser(typeof(T)));
        }

        public void ClearParserOf()
        {
            ParserOfTypes?.Clear();
        }

        internal IParser GetDocumentParser(Type type)
        {
            return (IParser)Activator.CreateInstance(type);
        }
    }
}
