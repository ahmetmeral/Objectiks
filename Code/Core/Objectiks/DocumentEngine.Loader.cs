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
    public partial class DocumentEngine : IDocumentEngine
    {
        public virtual void FirstLoadAllDocumentType()
        {
            Watcher?.Lock();

            foreach (var typeOf in Option.TypeOf)
            {
                CheckDirectoryOrSchema(typeOf);

                if (LoadDocumentType(typeOf))
                {
                    TypeOf.Add(typeOf.ToLowerInvariant());

                }
            }

            Watcher?.UnLock();

            FirstLoaded = true;
        }

        public virtual bool LoadDocumentType(string typeOf)
        {
            Logger?.Debug(DebugType.Engine, $"Load TypeOf: {typeOf}");

            var schema = GetDocumentSchema(typeOf);
            var meta = new DocumentMeta(typeOf, schema, Provider, Option);
            var refs = meta.GetRefs(false);
            var files = new List<DocumentStorage>();

            #region Files
            var directoryInfo = new DirectoryInfo(meta.Directory);
            if (directoryInfo.Exists)
            {
                var extentions = Option.Extention;
                var directoryFiles = directoryInfo.GetFiles(extentions, SearchOption.TopDirectoryOnly);

                meta.HasData = directoryFiles.Length > 0;

                var parts = new List<int>();
                foreach (var file in directoryFiles)
                {
                    var info = new DocumentStorage(meta.TypeOf, Provider?.BaseDirectory, file);

                    if (info.Partition > meta.Partitions.Current)
                    {
                        meta.Partitions.Current = info.Partition;
                    }

                    if (!parts.Contains(info.Partition))
                    {
                        parts.Add(info.Partition);
                    }

                    meta.DiskSize += file.Length;

                    files.Add(info);
                }

                parts = parts.OrderBy(p => p).ToList();

                foreach (var part in parts)
                {
                    meta.Partitions.Add(part, 0);
                }

                meta.Partitions.Next = meta.Partitions.Current + 1;
            }
            #endregion

            if (files.Count == 0)
            {
                Logger?.Debug(DebugType.Engine, "LoadDocumentType files.count = 0");

                return false;
            }

            Logger?.Debug(DebugType.Engine, $"TypeOf:{typeOf} number of files : {files.Count}");

            int bufferSize = Option.BufferSize;
            var serializer = new JsonSerializer();

            foreach (DocumentStorage file in files)
            {
                Logger?.Debug(DebugType.Engine, $"Read TypeOf: {file.TypeOfName} - File : {file.Target}");

                try
                {
                    using (FileStream fs = new FileStream(file.Target, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize))
                    using (StreamReader sr = new StreamReader(fs, Encoding.UTF8, true, bufferSize))
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        reader.SupportMultipleContent = false;

                        while (reader.Read())
                        {
                            if (reader.TokenType == JsonToken.StartObject)
                            {
                                var document = new Document
                                {
                                    TypeOf = typeOf,
                                    Data = serializer.Deserialize<JObject>(reader),
                                    Partition = file.Partition,
                                    CreatedAt = DateTime.UtcNow
                                };

                                UpdateDocumentMeta(ref meta, ref document, file.Partition, OperationType.Read);

                                if (Option.SupportDocumentParser)
                                {
                                    ParseDocumentData(ref meta, ref document, file);
                                }

                                if (Option.SupportLoaderInRefs)
                                {
                                    ParseDocumentRefs(refs, ref document);
                                }

                                Cache.Set(document, meta.Cache.Expire);

                                document.Dispose();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger?.Fatal($"Exception Read TypeOf: {file.TypeOfName} File : {file.Target}", ex);
                }
            }

            Cache.Set(meta, meta.Cache.Expire);

            return true;
        }

        protected virtual void CheckDirectoryOrSchema(string typeOf)
        {
            Logger?.Debug(DebugType.Engine, "Check Document Directory and Schema");

            var documents = Path.Combine(Provider.BaseDirectory, DocumentDefaults.Documents, typeOf);
            if (!Directory.Exists(documents))
            {
                Directory.CreateDirectory(documents);

                Logger?.Debug(DebugType.Engine, $"TypeOf:{typeOf} directory created.. Directory : {documents}");
            }

            var docFile = Path.Combine(documents, $"{typeOf}.json");
            if (!File.Exists(docFile))
            {
                File.WriteAllText(docFile, "[]", Encoding.UTF8);

                Logger?.Debug(DebugType.Engine, $"TypeOf:{typeOf} document created.. File: {docFile}");
            }

            var docSchema = Path.Combine(Provider.BaseDirectory, DocumentDefaults.Schemes, $"{typeOf}.json");

            if (!File.Exists(docSchema))
            {
                var temporySchema = DocumentSchema.Default();
                temporySchema.TypeOf = typeOf;
                temporySchema.ParseOf = "Document";
                temporySchema.KeyOf = Option.KeyOf;
                temporySchema.Primary = Option.Primary;

                var schema = new DocumentSerializer().Serialize(temporySchema);

                File.WriteAllText(docSchema, schema, Encoding.UTF8);

                Logger?.Debug(DebugType.Engine, $"TypeOf:{typeOf} schema created.. File: {docSchema}");
            }
        }

    }
}
