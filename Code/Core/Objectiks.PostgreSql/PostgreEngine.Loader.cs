using Newtonsoft.Json.Linq;
using Npgsql;
using Npgsql.Json;
using Objectiks.Engine;
using Objectiks.Engine.Query;
using Objectiks.Extentions;
using Objectiks.Models;
using Objectiks.PostgreSql.Engine;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace Objectiks.PostgreSql
{
    public partial class PostgreEngine : DocumentEngine
    {
        public override bool LoadDocumentType(string typeOf, bool isInitialize = false)
        {
            var schema = GetDocumentType(typeOf);
            var meta = new DocumentMeta(typeOf, schema, Provider, Option);

            if (isInitialize && meta.Cache.Lazy)
            {
                return false;
            }

            meta.Partitions.Add(0, 0);

            var query = new DocumentQuery(typeOf);
            query.Skip = 0;
            query.Take = Option.SqlProviderDataReaderPageSize;

            using (var reader = new PostgreReader(Provider, Option, query, Logger))
            {
                while (reader.Read())
                {
                    foreach (var row in reader.Rows)
                    {
                        var document = GetDocumentFromSource(ref meta, row, 0);

                        if (Option.SupportDocumentParser)
                        {
                            ParseDocumentData(ref meta, ref document, new DocumentStorage(), OperationType.Load);
                        }

                        meta.SubmitChanges(document, OperationType.Load);

                        Cache.Set(document, meta.Cache.Expire);

                        document.Dispose();
                    }

                    reader.NextPage();
                }

                Cache.Set(meta, meta.Cache.Expire);
            }

            return true;
        }

        public override void CheckTypeOfSchema(string typeOf)
        {
            
        }
    }
}
