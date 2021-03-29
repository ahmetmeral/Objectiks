using Objectiks.Engine;
using Objectiks.Models;
using Objectiks.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Objectiks
{
    public partial class ObjectiksOf
    {
        public static class Core
        {
            public static DocumentManifest Manifest { get; private set; }
            public static DocumentEngine Engine { get; private set; }
            public static DocumentCache Cache { get; private set; }
            public static IDocumentConnection Connection { get; private set; }
            public static List<IParser> ParseOf { get; private set; }
            public static List<string> TypeOf { get; private set; }
            public static DocumentSchema DefaultSchema { get; private set; }


            internal static DocumentEngine Initialize(DocumentOptions options)
            {
                Connection = options.Connection;
                DefaultSchema = DocumentSchema.Default();
                Manifest = GetDocumentManifest();
                Cache = GetDocumentCache(options);
                Engine = GetDocumentEngine(options);
                ParseOf = GetDocumentParsers(options);

                TypeOf = Engine.LoadAllDocumentType(Manifest.TypeOf);

                return Engine;
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

            private static DocumentManifest GetDocumentManifest()
            {
                return DocumentManifest.Get(Connection);
            }

            private static DocumentCache GetDocumentCache(DocumentOptions options)
            {
                return (DocumentCache)Activator.CreateInstance(options.Cache, Manifest, options.Connection);
            }

            private static DocumentEngine GetDocumentEngine(DocumentOptions options)
            {
                return (DocumentEngine)Activator.CreateInstance(options.Engine, Manifest, options.Connection, Cache);
            }

            private static List<IParser> GetDocumentParsers(DocumentOptions options)
            {
                var list = new List<IParser>();

                foreach (var parseOf in options.ParserOf)
                {
                    list.Add((IParser)Activator.CreateInstance(parseOf));
                }

                return list;
            }
        }
    }
}
