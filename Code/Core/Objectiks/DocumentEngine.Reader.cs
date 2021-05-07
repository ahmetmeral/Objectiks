using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Objectiks.Engine;
using Objectiks.Engine.Query;
using Objectiks.Extentions;
using Objectiks.Models;
using Objectiks.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Linq.Dynamic.Core;
using Objectiks.Helper;
using System.Threading;

namespace Objectiks
{
    public abstract partial class DocumentEngine : IDocumentEngine
    {
        public abstract Document Read(string typeOf, object primaryOf);
        public abstract DocumentInfo GetTypeOfDocumentInfo(string typeOf, object primaryOf, Type primaryOfDataType);
        public abstract Document Read(DocumentQuery query, DocumentMeta meta = null);
        public abstract T Read<T>(DocumentQuery query, DocumentMeta meta = null);
        public abstract List<T> ReadList<T>(DocumentQuery query);
        public abstract T GetCount<T>(DocumentQuery query, DocumentMeta meta = null);
        public abstract List<DocumentMeta> GetTypeMetaAll();
        public abstract DocumentMeta GetTypeMeta(string typeOf);
        public abstract T ReadAnyCacheOfFromQuery<T>(DocumentQuery query);
        public abstract void RemoveAnyCacheOfFromQuery(DocumentQuery query);
        public abstract void SetAnyCacheOfDocument<T>(DocumentQuery query, T data);
        public abstract T GetCountFromQueryOf<T>(DocumentQuery query, DocumentMeta meta = null);
        public abstract QueryResult GetDocumentKeysFromQueryOf(DocumentQuery query, DocumentMeta meta = null);
    }
}
