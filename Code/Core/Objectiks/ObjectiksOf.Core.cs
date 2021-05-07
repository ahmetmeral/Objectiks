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
            private static DocumentProvider Provider;
            private static DocumentEngines Engines = new DocumentEngines();
            private static DocumentOptions Options = new DocumentOptions();

            internal static IDocumentEngine Get()
            {
                return Get(Provider, null);
            }

            internal static IDocumentEngine Get(DocumentProvider documentProvider, DocumentOption option)
            {
                if (documentProvider == null)
                {
                    throw new Exception("DocumentProvider is null");
                }

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
                    throw new Exception("Engine undefined..");
                }

                return (DocumentEngine)Activator.CreateInstance(option.EngineProvider, documentProvider, option); 
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
                Provider = provider;

                if (option == null)
                {
                    option = GetOption(provider);
                }
                else
                {
                    if (option.HasManifest)
                    {
                        option = GetOptionMergeManifest(provider, option);
                    }

                    Map(provider.Key, option);
                }

                var engine = Get(provider, option).Initialize();

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
                    var manifest = DocumentManifest.Get(path);

                    option = manifest;
                    option.RegisterDefaults();

                    if (option.CacheInstance == null)
                    {
                        option.CacheInstance = new DocumentInMemory(option.Name, new DocumentBsonSerializer());
                    }
                }

                if (Options.ContainsKey(key))
                {
                    Options[key] = option;
                }
                else
                {
                    Options.TryAdd(key, option);
                }

                return option;
            }

            private static DocumentOption GetOptionMergeManifest(DocumentProvider provider, DocumentOption option)
            {
                var path = Path.Combine(provider.BaseDirectory, DocumentDefaults.Manifest);
                var manifest = DocumentManifest.Get(path);

                if (manifest == null)
                {
                    throw new Exception("Manifest file not found");
                }

                manifest.CacheInstance = option.CacheInstance;
                manifest.ParserOfTypes = option.ParserOfTypes;
                manifest.DocumentWatcher = option.DocumentWatcher;
                manifest.DocumentLogger = option.DocumentLogger;
                manifest.EngineProvider = option.EngineProvider;
                manifest.HasManifest = true;

                return manifest;
            }
        }
    }
}


