using Objectiks.Caching;
using Objectiks.Engine;
using Objectiks.Parsers;
using Objectiks.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks
{
    public class DocumentOptions
    {
        internal Type Cache { get; private set; }
        internal Type Engine { get; private set; }
        internal Type Watcher { get; private set; }
        internal Type Logger { get; private set; }
        internal List<Type> ParserOf { get; private set; }

        public DocumentOptions()
        {
            RegisterDefaultTypeOrParser();
        }

        protected virtual void RegisterDefaultTypeOrParser()
        {
            ParserOf = new List<Type>();

            UseCacheTypeOf<DocumentInMemory>();
            UseWatcher<DocumentWatcher>();

            AddParserTypeOf<DocumentDefaultParser>();
            AddParserTypeOf<DocumentOneToOneParser>();
            AddParserTypeOf<DocumentManyToManyParser>();
            AddParserTypeOf<DocumentOneToManyParser>();
            AddParserTypeOf<DocumentOneToOneFileParser>();
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

        public void AddParserTypeOf<T>() where T : IParser
        {
            ParserOf.Add(typeof(T));
        }

        public void ClearParserOf()
        {
            ParserOf?.Clear();
        }
    }
}
