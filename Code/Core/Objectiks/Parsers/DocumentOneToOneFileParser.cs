using Newtonsoft.Json.Linq;
using Objectiks.Helper;
using Objectiks.Models;
using Objectiks.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Objectiks.Parsers
{
    public class DocumentOneToOneFileParser : IDocumentRefParser
    {
        public string ParseOf => "1:1F";

        public DocumentOneToOneFileParser()
        {
        }

        public bool IsValidRef(DocumentRef docRef)
        {
            Ensure.NotNullOrEmpty(docRef.TypeOf, $"Parser: {ParseOf} -> TypeOf undefined..");
            Ensure.NotNull(docRef.MapOf, $"ParserOf: {ParseOf} -> MapOf undefined");
            Ensure.NotNullOrEmpty(docRef.MapOf.Source, $"Parser: {ParseOf} -> MapOf Source undefined..");
            Ensure.NotNullOrEmpty(docRef.MapOf.Target, $"Parser: {ParseOf} -> MapOf Target undefined..");

            return true;
        }

        public void Parse(IDocumentEngine engine, Document document, DocumentRef docRef)
        {
            JObject source = document.Data;

            if (!source.ContainsKey(docRef.MapOf.Source))
            {
                throw new ArgumentNullException($"ParserOf: {ParseOf} -> {docRef.MapOf.Source} Property not found..");
            }

            var typeOf = document.TypeOf;
            var fileName = source[docRef.MapOf.Source].ToString();

            if (!String.IsNullOrWhiteSpace(fileName))
            {
                var path = Path.Combine(engine.Connection.BaseDirectory, DocumentDefaults.Documents,
                    typeOf, DocumentDefaults.Contents, fileName);

                if (File.Exists(path))
                {
                    source[docRef.MapOf.Target] = FileHelper.Get(path, engine.Manifest.Documents.BufferSize);
                }
                else
                {
                    source[docRef.MapOf.Target] = string.Empty;
                }
            }

            document.Data = source;
        }
    }
}
