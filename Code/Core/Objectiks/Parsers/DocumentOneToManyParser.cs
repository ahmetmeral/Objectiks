using Newtonsoft.Json.Linq;
using Objectiks.Engine.Query;
using Objectiks.Extentions;
using Objectiks.Models;
using Objectiks.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;

namespace Objectiks.Parsers
{
    public class DocumentOneToManyParser : IDocumentRefParser
    {
        public string ParseOf => "1:M";

        public DocumentOneToManyParser()
        {
        }

        public bool IsValidRef(DocumentRef docRef)
        {
            Ensure.NotNullOrEmpty(docRef.TypeOf, $"ParserOf: {ParseOf} -> TypeOf undefined..");
            Ensure.NotNull(docRef.MapOf.Source, $"ParserOf: {ParseOf} -> MapOf Source propery undefined..");

            return true;
        }

        public void Parse(IDocumentProvider provider, Document document, DocumentRef docRef)
        {
            var query = new QueryOf(docRef.TypeOf);
            var manifest = provider.Manifest;
            var meta = provider.GetTypeMeta(query.TypeOf);
            JObject source = document.Data;

            if (docRef.KeyOf != null)
            {
                foreach (var sourceKeyOf in docRef.KeyOf.Source)
                {
                    query.ContainsBy(DocumentDefaults.DocumentMetaKeyOfProperty, source[sourceKeyOf]);
                }
            }

            //match keys..
            var keys = meta.GetDocumentKeysFromQueryOf(query);
            var property = docRef.GetTargetProperty();

            if (docRef.MapOf != null && !String.IsNullOrEmpty(docRef.MapOf.Target))
            {
                if (source.ContainsKey(property))
                {
                    if (!manifest.Documents.Parser.PropertyOverride)
                    {
                        throw new ArgumentException($"ParserOf:{ParseOf} - Ref Type : {docRef.TypeOf} - Ref Index: {docRef.Index} --> Document ref already define.. Use MapOf Target Property", docRef.TypeOf);
                    }
                }

                source[property] = new JObject();

                foreach (var key in keys)
                {
                    var queryOfFromKey = new QueryOf(meta.TypeOf, key.PrimaryOf);
                    var target = provider.Read<JObject>(queryOfFromKey, meta);

                    var sourcePropertyName = target[docRef.MapOf.Target].ToString();
                    source[property][sourcePropertyName] = target;
                }
            }
            else
            {
                if (source.ContainsKey(property))
                {
                    if (!manifest.Documents.Parser.PropertyOverride)
                    {
                        throw new ArgumentException($"ParserOf:{ParseOf} - Ref Type : {docRef.TypeOf} - Ref Index: {docRef.Index} --> Document ref already define.. Use MapOf Target Property", docRef.TypeOf);
                    }
                }

                source[property] = provider.ReadList(keys.GetQueryOfFromPrimaryOf(meta.TypeOf), meta);
            }

            document.Data = source;
        }
    }
}
