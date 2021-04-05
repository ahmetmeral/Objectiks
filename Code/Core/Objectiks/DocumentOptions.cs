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
        internal IDocumentConnection Connection { get; private set; }

        public DocumentOptions()
        {
            RegisterDefaultTypeOrParser(string.Empty);
        }

        public DocumentOptions(string baseDirectory)
        {
            RegisterDefaultTypeOrParser(baseDirectory);
        }

        private void RegisterDefaultTypeOrParser(string baseDirectory)
        {
            ParserOf = new List<Type>();
            Connection = new DocumentConnection(baseDirectory);

            UseCacheTypeOf<DocumentInMemory>();
            UseEngineTypeOf<DocumentEngine>();
            UseWatcher<DocumentWatcher>();

            AddParserTypeOf<DocumentDefaultParser>();
            AddParserTypeOf<DocumentOneToOneParser>();
            AddParserTypeOf<DocumentManyToManyParser>();
            AddParserTypeOf<DocumentOneToManyParser>();
            AddParserTypeOf<DocumentOneToOneFileParser>();
        }

        public void UseEngineTypeOf<T>() where T : DocumentEngine
        {
            Engine = typeof(T);
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

        public void UseConnection(IDocumentConnection connection)
        {
            Connection = connection;
        }

        public void ClearParserOf()
        {
            ParserOf?.Clear();
        }

    }
}
