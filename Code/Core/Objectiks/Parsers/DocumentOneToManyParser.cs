﻿using Newtonsoft.Json.Linq;
using Objectiks.Engine;
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

        public void Parse(IDocumentEngine engine, Document document, DocumentRef docRef)
        {
            var query = new DocumentQuery(docRef.TypeOf);
            var option = engine.Option;
            var meta = engine.GetTypeMeta(query.TypeOf);
            JObject source = document.Data;

            if (docRef.KeyOf != null)
            {
                foreach (var sourceKeyOf in docRef.KeyOf.Source)
                {
                    query.AddParameter(new QueryParameter
                    {
                        Type = QueryParameterType.KeyOf,
                        Field = DocumentDefaults.DocumentMetaKeyOfProperty,
                        Value = source[sourceKeyOf]
                    });
                }
            }

            //match keys..
            var queryResult = engine.GetDocumentKeysFromQueryOf(query, meta);
            var documentKeys = queryResult.Keys;
            var property = docRef.GetTargetProperty();

            if (docRef.MapOf != null && !String.IsNullOrEmpty(docRef.MapOf.Target))
            {
                source[property] = new JObject();

                foreach (var key in documentKeys)
                {
                    var queryOfFromKey = new DocumentQuery(meta.TypeOf, key.PrimaryOf);
                    var target = engine.Read<JObject>(queryOfFromKey, meta);

                    var sourcePropertyName = target[docRef.MapOf.Target].ToString();
                    source[property][sourcePropertyName] = target;
                }
            }
            else
            {
                source[property] = engine.ReadList(documentKeys.GetQueryOfFromPrimaryOf(meta.TypeOf), meta);
            }

            document.Data = source;
        }
    }
}
