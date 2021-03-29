using Objectiks.Engine;
using Objectiks.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks
{
    public class DocumentOptions
    {
        public Type Cache { get; private set; }
        public Type Engine { get; private set; }
        public List<Type> ParserOf { get; private set; }
        public IDocumentConnection Connection { get; private set; }

        public DocumentOptions()
        {
            ParserOf = new List<Type>();
        }

        public void UseEngineTypeOf<T>() where T : DocumentEngine
        {
            Engine = typeof(T);
        }

        public void UseCacheTypeOf<T>() where T : IDocumentCache
        {
            Cache = typeof(T);
        }

        public void AddParserTypeOf<T>() where T : IParser
        {
            ParserOf.Add(typeof(T));
        }

        public void UseConnection(IDocumentConnection connection)
        {
            Connection = connection;
        }
    }
}
