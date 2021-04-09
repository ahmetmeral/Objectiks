using Objectiks.Attributes;
using Objectiks.Engine;
using Objectiks.Engine.Query;
using Objectiks.Extentions;
using Objectiks.Models;
using Objectiks.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Objectiks
{
    public partial class ObjectiksOf
    {
        //load docuemnt ve get meta data yı vs. core taşıyoruz..refleri vs..yüklemek için gerekiyor.
        //her ref in kendine göre bir providerı olabilir.
        public static class Core
        {
            public static DocumentManifest Manifest { get; private set; }
            public static IDocumentLogger Logger { get; private set; }
            public static DocumentCache Cache { get; private set; }
            public static DocumentWatcher Watcher { get; private set; }
            public static DocumentParsers ParseOf { get; private set; }
            public static DocumentTypes TypeOf { get; private set; }
            public static DocumentSchema DefaultSchema { get; private set; }

            private static ConcurrentDictionary<string, DocumentProvider> Providers;
            private static ConcurrentDictionary<string, string> TypeOfProvider;
            private static DocumentTypes SkipTypes = new DocumentTypes();
            private static bool IsInitialize = false;

            internal static void Initialize(ObjectiksOptions options)
            {
                if (!IsInitialize)
                {
                    DefaultSchema = DocumentSchema.Default();
                    Manifest = GetDocumentManifest(options);
                    Logger = GetDocumentLogger(options);
                    Cache = GetDocumentCache(options);
                    Watcher = GetDocumentWatcher(options);
                    Providers = GetProviders(options);
                    ParseOf = GetDocumentParsers(options);
                    TypeOfProvider = GetTypeOfProviderMap();
                    TypeOf = LoadDocumentTypes(Manifest);
                    IsInitialize = true;
                    //TypeOf = Engine.LoadAllDocumentType(Manifest.TypeOf);
                }
            }

            private static DocumentManifest GetDocumentManifest(ObjectiksOptions options)
            {
                var manifest = DocumentManifest.Get(options.ManifestFilePath);

                if (!String.IsNullOrWhiteSpace(options.BaseDirectory))
                {
                    if (manifest.ProviderOf.ContainsKey(DocumentDefaults.ProviderOf))
                    {
                        manifest.ProviderOf[DocumentDefaults.ProviderOf].BaseDirectory = options.BaseDirectory;
                    }
                }

                return manifest;
            }

            private static IDocumentLogger GetDocumentLogger(ObjectiksOptions options)
            {
                if (options.Logger == null)
                {
                    return null;
                }

                return (IDocumentLogger)Activator.CreateInstance(options.Logger);
            }

            private static DocumentCache GetDocumentCache(ObjectiksOptions options)
            {
                return (DocumentCache)Activator.CreateInstance(options.Cache, Manifest);
            }

            private static DocumentWatcher GetDocumentWatcher(ObjectiksOptions options)
            {
                if (options.Watcher == null)
                {
                    return null;
                }

                return (DocumentWatcher)Activator.CreateInstance(options.Watcher);
            }

            private static ConcurrentDictionary<string, DocumentProvider> GetProviders(ObjectiksOptions options)
            {
                ConcurrentDictionary<string, DocumentProvider> providers = new ConcurrentDictionary<string, DocumentProvider>();

                foreach (var provider in options.ProviderOf)
                {
                    if (Manifest.ProviderOf.ContainsKey(provider.Key))
                    {
                        var providerOptions = Manifest.ProviderOf[provider.Key];

                        /*
                        DocumentManifest manifest,
                        IDocumentOptions options,
                        IDocumentCache cache,
                        IDocumentLogger logger,
                        IDocumentWatcher watcher
                         */
                        var instance = (DocumentProvider)Activator
                            .CreateInstance(
                            //type
                            provider.Value,
                            //args
                            Manifest,
                            providerOptions,
                            Cache,
                            Logger,
                            Watcher
                            );

                        providers.TryAdd(provider.Key, instance);
                    }
                    else
                    {
                        throw new Exception($"ProviderOf: {provider.Key} not found..");
                    }
                }

                return providers;
            }

            private static ConcurrentDictionary<string, string> GetTypeOfProviderMap()
            {
                ConcurrentDictionary<string, string> typeOfProvider = new ConcurrentDictionary<string, string>();

                foreach (var provider in Manifest.ProviderOf)
                {
                    if (provider.Value.TypeOf == null)
                    {
                        throw new Exception($"ProviderOf: {provider.Key} - Types undefined..");
                    }

                    foreach (var typeOf in provider.Value.TypeOf)
                    {
                        typeOfProvider.TryAdd(typeOf, provider.Key);
                    }
                }

                return typeOfProvider;
            }

            public static string GetTypeOfName<T>()
            {
                var attr = typeof(T).GetCustomAttribute<TypeOfAttribute>();

                Ensure.NotNull(attr, "TypeOf undefined..");

                if (String.IsNullOrEmpty(attr.Name))
                {
                    attr.Name = typeof(T).Name;
                }

                return attr.Name;
            }

            private static DocumentParsers GetDocumentParsers(ObjectiksOptions options)
            {
                var list = new DocumentParsers();

                foreach (var parseOf in options.ParserOf)
                {
                    list.Add((IParser)Activator.CreateInstance(parseOf));
                }

                return list;
            }

            public static IDocumentParser GetDocumentParser(string typeOf)
            {
                var converter = ParseOf.Where(c => c.ParseOf == typeOf).FirstOrDefault();

                if (converter == null)
                {
                    converter = ParseOf.Where(c => c.ParseOf == DocumentDefaults.DocumentParseOf).FirstOrDefault();
                }

                if (converter == null)
                {
                    return null;
                }

                return (IDocumentParser)converter;
            }

            public static IDocumentRefParser GetReferenceParser(DocumentRef docRef)
            {
                var converter = ParseOf.Where(c => c.ParseOf == docRef.ParseOf).FirstOrDefault();

                Ensure.NotNull(converter, $"Core ReferenceParser Type : {docRef.ParseOf} -> Parser undefined..");

                return (IDocumentRefParser)converter;
            }

            public static string GetBaseDirectory(string typeOf)
            {
                return GetTypeOfProvider(typeOf).Options.BaseDirectory;
            }

            public static DocumentProvider GetTypeOfProvider<T>()
            {
                var typeOf = GetTypeOfName<T>();
                return GetTypeOfProvider(typeOf);
            }

            public static DocumentProvider GetTypeOfProvider(string typeOf)
            {
                if (String.IsNullOrWhiteSpace(typeOf))
                {
                    throw new Exception($"TypeOf name is empty");
                }

                TypeOfProvider.TryGetValue(typeOf, out var providerOf);

                if (String.IsNullOrEmpty(providerOf))
                {
                    throw new Exception($"TypeOf : {typeOf} provider undefined");
                }

                Providers.TryGetValue(providerOf, out DocumentProvider provider);

                if (provider == null)
                {
                    throw new Exception($"TypeOf : {typeOf} provider is null");
                }

                return provider;
            }

            private static DocumentTypes LoadDocumentTypes(DocumentManifest manifest)
            {
                var types = new DocumentTypes();

                foreach (var typeOf in manifest.TypeOf)
                {
                    var provider = GetTypeOfProvider(typeOf);
                    provider.CheckTypeOfSchema(typeOf);
                    provider.LoadDocumentType(typeOf, true);
                    types.Add(typeOf);
                }

                return types;
            }

            public static void LoadDocumentType(string typeOf, IDocumentProvider provider = null)
            {
                if (provider == null)
                {
                    provider = GetTypeOfProvider(typeOf);
                }

                provider.LoadDocumentType(typeOf, false);
            }

            public static Document Read(string typeOf, object primaryOf)
            {
                var document = Cache.GetOrCreate(typeOf, primaryOf, () =>
                {
                    LoadDocumentType(typeOf);

                    return Cache.Get(typeOf, primaryOf);
                });

                return document;
            }

            public static Document Read(QueryOf query, DocumentMeta meta = null)
            {
                if (meta == null)
                {
                    meta = GetTypeMeta(query.TypeOf);
                }

                List<DocumentKey> documentKeys = meta.GetDocumentKeysFromQueryOf(query);

                if (documentKeys.Count > 1 || documentKeys.Count == 0)
                {
                    return null;
                }

                return Read(query.TypeOf, documentKeys[0].PrimaryOf);
            }

            public static DocumentMeta GetTypeMeta(string typeOf)
            {
                var meta = Cache.GetOrCreate(typeOf, () =>
                {
                    LoadDocumentType(typeOf);

                    return Cache.Get(typeOf);
                });

                return meta;
            }

            public static List<DocumentTypeStatus> GetTypeOfStatusAll()
            {
                var list = new List<DocumentTypeStatus>();

                foreach (var typeOf in TypeOf)
                {
                    list.Add(GetTypeOfStatus(typeOf));
                }

                return list;
            }

            public static DocumentTypeStatus GetTypeOfStatus(string typeOf)
            {
                var status = Cache.GetStatus(typeOf);
                status.TypeOf = typeOf;

                if (!status.Loaded)
                {
                    status.Loaded = false;
                    status.Tick = status.Tick + 1;
                    status.CreatedAt = DateTime.UtcNow;
                }

                return status;
            }

            public static void UpdateTypeOfStatus(DocumentTypeStatus status, bool isLoaded)
            {
                status.Loaded = isLoaded;
                status.UpdatedAt = DateTime.UtcNow;
                Cache.SetStatus(status);
            }

            public static List<DocumentMeta> GetTypeMetaAll()
            {
                var list = new List<DocumentMeta>();

                foreach (var typeOf in TypeOf)
                {
                    var meta = GetTypeMeta(typeOf);

                    if (meta != null)
                    {
                        list.Add(meta);
                    }
                }

                return list;
            }
        }
    }
}
