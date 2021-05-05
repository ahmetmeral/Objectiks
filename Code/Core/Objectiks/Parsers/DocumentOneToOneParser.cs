using Newtonsoft.Json.Linq;
using Objectiks.Engine;
using Objectiks.Engine.Query;
using Objectiks.Extentions;
using Objectiks.Models;
using Objectiks.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Parsers
{
    public class DocumentOneToOneParser : IDocumentRefParser
    {
        public string ParseOf => "1:1";

        public bool IsValidRef(DocumentRef docRef)
        {
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
                    throw new Exception("Not supported..");
                }

                var index = query.ValueOf(sourceValue);
                query.KeyOfStatement($"{DocumentDefaults.DocumentMetaKeyOfProperty}.Contains(@{index})");
            }

            if (docRef.KeyOf.Any)
            {
                query.Any();
            }

            //(KeyOf.Contains(@0) OR KeyOf.Contains(@1)) AND KeyOf.Contains(@2)
            #endregion

            source[property] = engine.Read<JObject>(query, meta);

            document.Data = source;
        }
    }
}
