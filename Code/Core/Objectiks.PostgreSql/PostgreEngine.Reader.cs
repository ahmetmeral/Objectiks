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
        public override Document Read(string typeOf, object primaryOf)
        {
            throw new NotImplementedException();
        }

        public override DocumentInfo GetTypeOfDocumentInfo(string typeOf, object primaryOf, Type primaryOfDataType)
        {
            throw new NotImplementedException();
        }

        public override Document Read(DocumentQuery query, DocumentMeta meta = null)
        {
            throw new NotImplementedException();
        }

        public override T Read<T>(DocumentQuery query, DocumentMeta meta = null)
        {
            throw new NotImplementedException();
        }

        public override List<T> ReadList<T>(DocumentQuery query)
        {
            throw new NotImplementedException();
        }

        public override T GetCount<T>(DocumentQuery query, DocumentMeta meta = null)
        {
            throw new NotImplementedException();
        }

        public override List<DocumentMeta> GetTypeMetaAll()
        {
            throw new NotImplementedException();
        }

        public override DocumentMeta GetTypeMeta(string typeOf)
        {
            throw new NotImplementedException();
        }

        public override T ReadAnyCacheOfFromQuery<T>(DocumentQuery query)
        {
            throw new NotImplementedException();
        }

        public override void RemoveAnyCacheOfFromQuery(DocumentQuery query)
        {
            throw new NotImplementedException();
        }

        public override void SetAnyCacheOfDocument<T>(DocumentQuery query, T data)
        {
            throw new NotImplementedException();
        }

        public override T GetCountFromQueryOf<T>(DocumentQuery query, DocumentMeta meta = null)
        {
            throw new NotImplementedException();
        }

        public override QueryResult GetDocumentKeysFromQueryOf(DocumentQuery query, DocumentMeta meta = null)
        {
            throw new NotImplementedException();
        }
    }
}
