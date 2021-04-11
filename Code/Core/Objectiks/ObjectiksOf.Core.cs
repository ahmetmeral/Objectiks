using Objectiks.Engine;
using Objectiks.Models;
using Objectiks.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Objectiks
{
    public partial class ObjectiksOf
    {
        public static class Core
        {
            private static ConcurrentDictionary<string, DocumentEngine> Engines = new ConcurrentDictionary<string, DocumentEngine>();

            public static DocumentEngine Get(DocumentProvider documentProvider, DocumentOptions options)
            {
                if (documentProvider.Connection != null)
                {
                    return GetSql(documentProvider, options);
                }
                return GetDefault(documentProvider, options);
            }

            public static DocumentEngine GetDefault(DocumentProvider fileProvider, DocumentOptions options)
            {
                if (options == null)
                {
                    options = new DocumentOptions();
                }

                Engines.TryGetValue(fileProvider.Bucket, out DocumentEngine engine);

                if (engine == null)
                {
                    engine = new DocumentEngine(fileProvider, options);
                    engine.FirstLoadAllDocumentType();
                    Engines.TryAdd(fileProvider.Bucket, engine);
                }

                return engine;
            }

            public static DocumentEngine GetSql(DocumentProvider sqlProvider, DocumentOptions options)
            {
                if (options == null)
                {
                    options = new DocumentOptions();
                }

                Engines.TryGetValue(sqlProvider.Bucket, out DocumentEngine engine);

                if (engine == null)
                {
                    engine = new DocumentSqlEngine(sqlProvider, options);
                    engine.FirstLoadAllDocumentType();
                    Engines.TryAdd(sqlProvider.Bucket, engine);
                }
                else
                {
                    engine.Provider = sqlProvider;
                }

                return engine;
            }

            public static DocumentEngine Get(DocumentEngine engine)
            {
                if (!engine.FirstLoaded)
                {
                    engine.FirstLoadAllDocumentType();
                }

                return engine;
            }
        }
    }
}
