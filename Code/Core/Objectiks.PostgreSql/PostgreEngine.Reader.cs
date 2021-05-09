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
        public override DocumentInfo GetTypeOfDocumentInfo(string typeOf, object primary)
        {
            return Cache.GetDocumentInfo(typeOf, primary);
        }

        public override DocumentInfo GetTypeOfDocumentInfo(string typeOf, object primaryOf, Type primaryOfDataType)
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
    }
}
