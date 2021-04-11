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
            private static DocumentEngines Engines = new DocumentEngines();
            private static DocumentOptions Options = new DocumentOptions();

            public static DocumentEngine Get(DocumentProvider documentProvider, DocumentOption option)
            {
                Engines.TryGetValue(documentProvider.CacheBucket, out DocumentEngine engine);

                if (engine == null)
                {
                    if (option == null)
                    {
                        option = GetOption(documentProvider);
                    }

                    if (documentProvider.Connection == null)
                    {
                        engine = new DocumentEngine(documentProvider, option);
                    }
                    else
                    {
                        engine = new DocumentSqlEngine(documentProvider, option);
                    }

                    engine.FirstLoadAllDocumentType();
                    Engines.TryAdd(documentProvider.CacheBucket, engine);
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

            public static void Map(Type type, DocumentOption option)
            {
                var key = type.Name;

                if (Options.ContainsKey(key))
                {
                    Options[key] = option;
                }
                else
                {
                    Options.TryAdd(key, option);
                }
            }

            public static DocumentOption GetOption(DocumentProvider provider)
            {
                var key = provider.Connection != null ? provider.Connection.GetType().Name : typeof(DocumentProvider).Name;

                Options.TryGetValue(key, out DocumentOption setting);

                if (setting == null)
                {
                    var path = Path.Combine(provider.BaseDirectory, DocumentDefaults.Manifest);

                    setting = DocumentManifest.Get(path);

                    if (Options.ContainsKey(key))
                    {
                        Options[key] = setting;
                    }
                    else
                    {
                        Options.TryAdd(key, setting);
                    }
                }

                return setting;
            }
        }
    }
}
