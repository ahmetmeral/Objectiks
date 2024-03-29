﻿using Newtonsoft.Json;
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
    public partial class DocumentEngine : IDocumentEngine
    {
        public DocumentOption Option { get; private set; }
        public DocumentProvider Provider { get; internal set; }
        public IDocumentLogger Logger { get; private set; }
        public IDocumentCache Cache { get; private set; }
        public IDocumentWatcher Watcher { get; private set; }
        public List<IDocumentParser> ParseOf { get; private set; }

        private readonly DocumentMonitor Monitor;

        public DocumentEngine()
        {
            Monitor = new DocumentMonitor();
        }

        public DocumentEngine(DocumentProvider documentProvider, DocumentOption option)
        {
            Option = option;
            Provider = documentProvider;
            Cache = option.CacheInstance;
            Logger = option.DocumentLogger;
            Watcher = option.DocumentWatcher;
            ParseOf = option.ParserOfTypes;
            Monitor = new DocumentMonitor();

            if (Option.SupportDocumentWatcher)
            {
                Watcher?.WaitForChanged(this);
            }
        }

        public virtual DocumentType GetDocumentType(string typeOf)
        {
            DocumentType documentType = null;

            if (Option.TypeOf != null)
            {
                documentType = Option.TypeOf.Where(s => s.TypeOf == typeOf).FirstOrDefault();
            }
            else
            {
                documentType = new DocumentType(typeOf);
            }

            return documentType;
        }

        public virtual Document GetDocumentFromSource(ref DocumentMeta meta, JObject data, int partition)
        {
            var document = new Document();
            document.TypeOf = meta.TypeOf;
            document.PrimaryOf = data[meta.PrimaryOf].AsString();
            document.CacheOf = Cache.CacheOfDoc(meta.TypeOf, document.PrimaryOf);
            document.KeyOf = data.ToKeyOfValues(meta.TypeOf, meta.KeyOfNames, meta.PrimaryOf);
            document.Partition = partition;
            document.CreatedAt = DateTime.UtcNow;

            if (!String.IsNullOrEmpty(meta.WorkOf) && data.ContainsKey(meta.WorkOf))
            {
                document.WorkOf = data[meta.WorkOf].AsString();
            }
            else
            {
                document.WorkOf = "0";
            }

            if (!String.IsNullOrEmpty(meta.UserOf) && data.ContainsKey(meta.UserOf))
            {
                document.UserOf = data[meta.UserOf].AsString();
            }
            else
            {
                document.UserOf = "0";
            }

            document.Data = data;

            return document;
        }

        public virtual IDocumentParser GetDocumentParser(string typeOf, OperationType operation)
        {
            var converter = ParseOf.Where(c => c.ParseOf == typeOf && c.IsParse(operation) == true).FirstOrDefault();

            if (converter == null)
            {
                converter = ParseOf.Where(c => c.ParseOf == DocumentDefaults.DocumentParseOf).FirstOrDefault();
            }

            if (converter == null)
            {
                return null;
            }

            return (IDocumentParser)converter;
        }

        public virtual DocumentTransaction BeginTransaction()
        {
            return GetTransaction(true, false);
        }

        public virtual DocumentTransaction BeginInternalTransaction()
        {
            return GetTransaction(true, true);
        }

        public virtual DocumentTransaction GetThreadTransaction()
        {
            return GetTransaction(false, false);
        }

        public virtual DocumentTransaction GetTransaction(bool create, bool isInternal)
        {
            return Monitor.GetTransaction(this, create, isInternal);
        }

        public virtual void ReleaseTransaction(DocumentTransaction transaction)
        {
            Monitor.ReleaseTransaction(transaction);
        }

        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
