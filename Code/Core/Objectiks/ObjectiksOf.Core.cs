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
                DocumentEngine engine;

                if (option == null)
                {
                    option = GetOption(documentProvider);
                }

                if (option.CacheInstance == null)
                {
                    option.CacheInstance = new DocumentInMemory(option.Name, new DocumentBsonSerializer());
                }

                if (option.EngineProvider == null)
                {
                    engine = new DocumentEngine(documentProvider, option);
                }
                else
                {
                    engine = (DocumentEngine)Activator.CreateInstance(option.EngineProvider, documentProvider, option);
                }

                return engine;
            }

            public static void Map(Type type, DocumentOption option)
            {
                var key = type.Name;

                Map(key, option);
            }

            private static void Map(string key, DocumentOption option)
            {
                if (Options.ContainsKey(key))
                {
                    Options[key] = option;
                }
                else
                {
                    Options.TryAdd(key, option);
                }
            }

            public static void Initialize(DocumentProvider provider, DocumentOption option = null)
            {
                if (option == null)
                {
                    option = GetOption(provider);
                }
                else
                {
                    Map(provider.Key, option);
                }

                var engine = Get(provider, option).Initialize();

                if (!engine.IsInitialize)
                {
                    throw new Exception("Engine initialize error");
                }
            }

            public static void Initialize(string baseDirectory, DocumentOption option = null)
            {
                Initialize(new DocumentProvider(baseDirectory), option);
            }

            public static void Initialize(IDbConnection connection, DocumentOption option = null)
            {
                Initialize(new DocumentProvider(connection), option);
            }

            public static void Initialize(DocumentOption option)
            {
                Initialize(new DocumentProvider(), option);
            }

            public static DocumentOption GetOption(DocumentProvider provider)
            {
                var key = provider.Key;

                if (!Options.TryGetValue(key, out DocumentOption option))
                {
                    key = provider.Connection != null ? provider.Connection.GetType().Name : typeof(DocumentProvider).Name;
                    Options.TryGetValue(key, out option);
                }

                if (option == null)
                {
                    var path = Path.Combine(provider.BaseDirectory, DocumentDefaults.Manifest);

                    option = DocumentManifest.Get(path);
                    option.RegisterDefaultTypeOrParser();

                    if (option.CacheInstance == null)
                    {
                        option.CacheInstance = new DocumentInMemory(option.Name, new DocumentBsonSerializer());
                    }

                    if (Options.ContainsKey(key))
                    {
                        Options[key] = option;
                    }
                    else
                    {
                        Options.TryAdd(key, option);
                    }
                }

                return option;
            }
        }
    }
}


