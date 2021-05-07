using Newtonsoft.Json.Linq;
using Npgsql;
using Npgsql.Json;
using Objectiks.Engine;
using Objectiks.Engine.Query;
using Objectiks.Extentions;
using Objectiks.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace Objectiks.PostgreSql
{
    public partial class PostgreEngine : DocumentEngine
    {
        public override void BulkAppend(DocumentContext context, DocumentTransaction transaction)
        {

        }

        public override void BulkCreate(DocumentContext context, DocumentTransaction transaction)
        {
            //using (var writer = new PostgreSqlWriter(meta.TypeOf, Provider, Option, Logger))
            //{
            //    writer.BulkCreate(docs);
            //}
        }

        public override void BulkMerge(DocumentContext context, DocumentTransaction transaction)
        {

        }

        public override void BulkDelete(DocumentContext context, DocumentTransaction transaction)
        {

        }

        public override void SubmitChanges(DocumentContext context, DocumentTransaction transaction)
        {
            throw new NotImplementedException();
        }

        public override void OnChangeDocuments(DocumentMeta meta, DocumentContext context)
        {
            throw new NotImplementedException();
        }

        public override void TruncateTypeOf(string typeOf)
        {
            throw new NotImplementedException();
        }

        public override void TruncateTypeOf(DocumentMeta meta)
        {
            throw new NotImplementedException();
        }

        public override int Delete<T>(DocumentQuery query)
        {
            throw new NotImplementedException();
        }
    }
}
