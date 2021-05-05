using Newtonsoft.Json.Linq;
using Objectiks.Engine.Query;
using Objectiks.Extentions;
using Objectiks.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Objectiks;
using Objectiks.Services;
using System.Linq;
using System.Linq.Dynamic.Core;
using Objectiks.Engine;

namespace Objectiks.Parsers
{
    public class DocumentManyToManyParser : IDocumentRefParser
    {
        public string ParseOf => "M:M";

        public DocumentManyToManyParser()
        {
        }

        public bool IsValidRef(DocumentRef documentRef)
        {
            Ensure.NotNullOrEmpty(documentRef.TypeOf, $"Parser: {ParseOf} -> TypeOf undefined..");


            return true;
        }

        public void Parse(IDocumentEngine engine, Document document, DocumentRef docRef)
        {
            JObject source = document.Data;
            var query = new DocumentQuery(docRef.TypeOf);
            var meta = engine.GetTypeMeta(query.TypeOf);
            var property = docRef.GetTargetProperty();

            #region QueryBuilder
            foreach (var sourceKeyOf in docRef.KeyOf.Source)
            {
                var parts = new List<string>();
                var sourceValue = source[sourceKeyOf];

                if (sourceValue == null)
                {
                    continue;
                }

                if (sourceValue.HasArray())
                {
                    foreach (var item in sourceValue)
                    {
                        var index = query.ValueOf(item);
                        parts.Add($"{DocumentDefaults.DocumentMetaKeyOfProperty}.Contains(@{index})");
                    }
                }
                else
                {
                    var index = query.ValueOf(sourceValue);
                    parts.Add($"{DocumentDefaults.DocumentMetaKeyOfProperty}.Contains(@{index})");
                }

                query.KeyOfStatement("(" + string.Join(" OR ", parts) + ")");
            }

            if (docRef.KeyOf.Any)
            {
                query.Any();
            }

            //(KeyOf.Contains(@0) OR KeyOf.Contains(@1)) AND KeyOf.Contains(@2)
            #endregion

            source[property] = engine.ReadList(query, meta); ;

            document.Data = source;
        }
    }
}
