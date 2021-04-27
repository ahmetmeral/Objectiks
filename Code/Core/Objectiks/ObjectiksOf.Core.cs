using Objectiks.Engine;
using Objectiks.Models;
using Objectiks.Caching.Serializer;
using Objectiks.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Objectiks.Caching;

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

                    if (option.CacheInstance == null)
                    {
                        option.CacheInstance = new DocumentInMemory(option.Name, new DocumentBsonSerializer());
                    }

                    if (documentProvider.Connection == null)
                    {
                        engine = new DocumentEngine(documentProvider, option);
                    }
                    else
                    {
                        if (option.EngineProvider != null)
                        {
                            engine = (DocumentEngine)Activator.CreateInstance(option.EngineProvider, documentProvider, option);
                        }
                        else
                        {
                            throw new Exception("Sql engine type undefined..");
                        }
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
                    setting.RegisterDefaultTypeOrParser();

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
