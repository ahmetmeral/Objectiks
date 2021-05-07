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

namespace Objectiks
{
    public abstract partial class DocumentEngine : IDocumentEngine
    {
        public abstract void SubmitChanges(DocumentContext context, DocumentTransaction transaction);
        public abstract void OnChangeDocuments(DocumentMeta meta, DocumentContext context);
        public abstract void BulkCreate(DocumentContext context, DocumentTransaction transaction);
        public abstract void BulkAppend(DocumentContext context, DocumentTransaction transaction);
        public abstract void BulkMerge(DocumentContext context, DocumentTransaction transaction);
        public abstract void BulkDelete(DocumentContext context, DocumentTransaction transaction);
        public abstract void TruncateTypeOf(string typeOf);
        public abstract void TruncateTypeOf(DocumentMeta meta);
        public abstract int Delete<T>(DocumentQuery query);
    }
}
