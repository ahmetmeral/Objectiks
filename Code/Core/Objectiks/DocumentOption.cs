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

        public DocumentCacheInfo Cache { get; set; } = new DocumentCacheInfo { Expire = 10000 };
        public DocumentSchemes Schemes { get; set; } = new DocumentSchemes();
        public DocumentVars Vars { get; set; }

        internal IDocumentSerializer Serializer { get; set; }

        internal Type CacheType { get; private set; } = typeof(DocumentInMemory);
        internal Type WatcherType { get; private set; }
        internal Type SqlEngineType { get; private set; }
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
                //AddParserTypeOf<DocumentDefaultParser>();
                AddParserTypeOf<DocumentOneToOneParser>();
                AddParserTypeOf<DocumentManyToManyParser>();
                AddParserTypeOf<DocumentOneToManyParser>();
                AddParserTypeOf<DocumentOneToOneFileParser>();
            }

            if (Serializer == null)
            {
                Serializer = new DocumentBsonSerializer();
            }
        }

        public void UseCacheTypeOf<T>() where T : IDocumentCache
        {
            CacheType = typeof(T);
        }

        public void UseWatcher<T>() where T : IDocumentWatcher
        {
            SupportDocumentWatcher = true;
            WatcherType = typeof(T);
        }

        public void UseLogger<T>() where T : IDocumentLogger
        {
            LoggerType = typeof(T);
        }

        public void UseSerializer<T>() where T : IDocumentSerializer
        {
            Serializer = (IDocumentSerializer)Activator.CreateInstance(typeof(T));
        }

        public void UseSerializer(IDocumentSerializer serializer)
        {
            Serializer = serializer;
        }

        public void AddParserTypeOf<T>() where T : IParser
        {
            ParserOfTypes.Add(typeof(T));
        }

        public void UseSqlEngine<T>() where T : IDocumentEngine
        {
            SqlEngineType = typeof(T);
        }

        public void ClearParserOf()
        {
            ParserOfTypes?.Clear();
        }
    }


}
