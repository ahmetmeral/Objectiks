using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Objectiks.Engine;
using Objectiks.Extentions;
using Objectiks.Helper;
using Objectiks.Models;
using Objectiks.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Objectiks.NoDb
{
    public partial class NoDbEngine : DocumentEngine
    {
        public override bool LoadDocumentType(string typeOf, bool isInitialize = false)
        {
            Logger?.Debug(ScopeType.Engine, $"Load TypeOf: {typeOf}");

            var documentType = GetDocumentType(typeOf);
            var meta = new DocumentMeta(typeOf, documentType, Provider, Option);

            if (isInitialize && meta.Cache.Lazy)
            {
                return true;
            }

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
                    var storage = new DocumentStorage(meta.TypeOf, Provider?.BaseDirectory, file);

                    if (storage.Partition > meta.Partitions.Current)
                    {
                        meta.Partitions.Current = storage.Partition;
                    }

                    if (!parts.Contains(storage.Partition))
                    {
                        parts.Add(storage.Partition);
                    }

                    files.Add(storage);
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
                Logger?.Debug(ScopeType.Engine, "LoadDocumentType files.count = 0");

                return false;
            }

            Logger?.Debug(ScopeType.Engine, $"TypeOf:{typeOf} number of files : {files.Count}");

            int bufferSize = Option.BufferSize;
            var serializer = new JsonSerializer();

            foreach (DocumentStorage file in files)
            {
                Logger?.Debug(ScopeType.Engine, $"Read TypeOf: {file.TypeOf} - File : {file.Target}");

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
                                var document = GetDocumentFromSource(ref meta, serializer.Deserialize<JObject>(reader), file.Partition);

                                if (Option.SupportDocumentParser)
                                {
                                    ParseDocumentData(ref meta, ref document, file, OperationType.Load);
                                }

                                meta.SubmitChanges(document, OperationType.Load);

                                Cache.Set(document, meta.Cache.Expire);

                                document.Dispose();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger?.Fatal($"Exception Read TypeOf: {file.TypeOf} File : {file.Target}", ex);
                }
            }

            Cache.Set(meta, meta.Cache.Expire);
            Cache.Set(new DocumentSequence(typeOf, meta.Sequence));

            documentType.Dispose();
            meta.Dispose();

            return true;
        }

        public override void CheckTypeOfSchema(string typeOf)
        {
            Logger?.Debug(ScopeType.Engine, "Check Document Directory");

            var documents = Path.Combine(Provider.BaseDirectory, DocumentDefaults.Documents, typeOf);
           
            if (!Directory.Exists(documents))
            {
                Directory.CreateDirectory(documents);

                Logger?.Debug(ScopeType.Engine, $"TypeOf:{typeOf} directory created.. Directory : {documents}");
            }

            var docFile = Path.Combine(documents, $"{typeOf}.json");
            if (!File.Exists(docFile))
            {
                File.WriteAllText(docFile, "[]", Encoding.UTF8);

                Logger?.Debug(ScopeType.Engine, $"TypeOf:{typeOf} document created.. File: {docFile}");
            }
        }
    }
}
