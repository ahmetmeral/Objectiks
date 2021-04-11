using Objectiks.Caching;
using Objectiks.Engine;
using Objectiks.Models;
using Objectiks.Parsers;
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
        public string UserOf { get; set; } = "UserRef";
        public string AccountOf { get; set; } = "AccountRef";
        public string ParseOf { get; set; } = "Document";
        public DocumentKeyOfNames KeyOf { get; set; }
        public DocumentTypes TypeOf { get; set; }
        public string DbSchema { get; set; }
        public string DbSchemaSeperator { get; set; } = ".";
        public string Extention { get; set; } = "*.json";
        public int BufferSize { get; set; } = 512;
        public bool SupportPartialStorage { get; set; } = true;
        public bool SupportTypeOfSchema { get; set; } = true;
        public bool SupportTypeOfCache { get; set; } = true;
        public bool SupportTypeOfRefs { get; set; } = true;
        public bool SupportFileAppend { get; set; } = true;
        public bool SupportTransaction { get; set; } = true;
        public int PartialStorageLimit { get; set; } = 1000;
        public bool RefProperyOverride { get; set; } = true;
        public bool SupportDocumentWatcher { get; set; } = false;
        public DocumentCacheInfo Cache { get; set; } = new DocumentCacheInfo { Expire = 10000 };
        public DocumentSchemes Schemes { get; set; } = new DocumentSchemes();

        internal Type CacheType { get; private set; } = typeof(DocumentInMemory);
        internal Type WatcherType { get; private set; } = typeof(DocumentWatcher);
        internal Type LoggerType { get; private set; }
        internal List<Type> ParserOfTypes { get; private set; }

        public DocumentOption()
        {
            ParserOfTypes = new List<Type>();
        }

        public virtual void RegisterDefaultTypeOrParser()
        {
            if (ParserOfTypes.Count == 0)
            {
                AddParserTypeOf<DocumentDefaultParser>();
                AddParserTypeOf<DocumentOneToOneParser>();
                AddParserTypeOf<DocumentManyToManyParser>();
                AddParserTypeOf<DocumentOneToManyParser>();
                AddParserTypeOf<DocumentOneToOneFileParser>();
            }
        }

        public void UseCacheTypeOf<T>() where T : IDocumentCache
        {
            CacheType = typeof(T);
        }

        public void UseWatcher<T>() where T : IDocumentWatcher
        {
            WatcherType = typeof(T);
        }

        public void UseDocumentLogger<T>() where T : IDocumentLogger
        {
            LoggerType = typeof(T);
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
