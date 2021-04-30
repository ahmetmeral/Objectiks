using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Objectiks.Engine;
using Objectiks.Models;
using Objectiks.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Objectiks.Helper
{
    public class JSONSerializer
    {
        private int BufferSize = 128;
        private IDocumentLogger Logger = null;

        public JSONSerializer(int? bufferSize = null)
        {
            if (bufferSize.HasValue)
            {
                BufferSize = bufferSize.Value;
            }
            else
            {
                BufferSize = 128;
            }
        }

        public JSONSerializer(IDocumentLogger documentLogger, int? bufferSize = null)
        {
            Logger = documentLogger;

            if (bufferSize.HasValue)
            {
                BufferSize = bufferSize.Value;
            }
        }

        public T Deserialize<T>(string contents) where T : class, new()
        {
            var obj = JsonConvert.DeserializeObject<T>(contents);

            if (obj == null)
            {
                obj = new T();
            }

            return obj;
        }

        public string Serialize<T>(T obj, Formatting formatting = Formatting.None)
        {
            return JsonConvert.SerializeObject(obj, formatting);
        }

        public T Get<T>(string path) where T : class, new()
        {
            var info = new FileInfo(path);

            using (var stream = info.Open(FileMode.Open))
            {
                using (var sr = new StreamReader(stream))
                {
                    var contents = sr.ReadToEnd();
                    return Deserialize<T>(contents);
                }
            }
        }

        //todo: bracket yoksa exception fırlatsın.
        //file sonunda ] karekterini bulur ve pozisyon alır..
        private void SeekJsonArrayEndSquareBrackets(FileStream fs)
        {
            long seed = -1;
            int length = 1;
            long offset = -1;
            var dataAsBytes = new byte[length];
            var dataAsString = string.Empty;

            while (dataAsString != "]")
            {
                fs.Seek(offset, SeekOrigin.End);
                dataAsBytes = new byte[length];
                fs.Read(dataAsBytes, 0, length);
                dataAsString = Encoding.UTF8.GetString(dataAsBytes);
                //next offset
                offset = offset + seed;
            }

            fs.Seek(offset + 1, SeekOrigin.End);
        }

        //Append : Insert işlemleri için 
        public void AppendRows<T>(DocumentStorage storage, List<T> rows, Formatting formatting = Formatting.None)
        {
            try
            {
                using (var fs = new FileStream(storage.Target, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite, BufferSize))
                {
                    SeekJsonArrayEndSquareBrackets(fs);

                    int rCount = rows.Count;

                    if (rCount == 1)
                    {
                        var row = Encoding.UTF8.GetBytes($",{Serialize(rows[0], formatting)}]");
                        fs.Write(row, 0, row.Length);
                    }
                    else
                    {
                        int arrayTagCloseIndex = rCount - 1;

                        for (int i = 0; i < rCount; i++)
                        {
                            if (i == arrayTagCloseIndex)
                            {
                                var row = Encoding.UTF8.GetBytes($",{Serialize(rows[i], formatting)}]");
                                fs.Write(row, 0, row.Length);
                            }
                            else
                            {
                                var row = Encoding.UTF8.GetBytes($",{Serialize(rows[i], formatting)}");
                                fs.Write(row, 0, row.Length);
                            }
                        }
                    }

                    fs.SetLength(fs.Position);
                }

            }
            catch (Exception ex)
            {

                Logger?.Fatal(ex);

                throw ex;
            }
        }

        public void AppendRows(DocumentStorage storage, List<JObject> rows, Formatting formatting = Formatting.None)
        {
            try
            {
                using (var fs = new FileStream(storage.Target, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite, BufferSize))
                {
                    SeekJsonArrayEndSquareBrackets(fs);

                    int rCount = rows.Count;

                    if (rCount == 1)
                    {
                        var row = Encoding.UTF8.GetBytes($",{rows[0].ToString(formatting)}]");
                        fs.Write(row, 0, row.Length);
                    }
                    else
                    {
                        int arrayTagCloseIndex = rCount - 1;

                        for (int i = 0; i < rCount; i++)
                        {
                            if (i == arrayTagCloseIndex)
                            {
                                var row = Encoding.UTF8.GetBytes($",{rows[i].ToString(formatting)}]");
                                fs.Write(row, 0, row.Length);
                            }
                            else
                            {
                                var row = Encoding.UTF8.GetBytes($",{rows[i].ToString(formatting)}");
                                fs.Write(row, 0, row.Length);
                            }
                        }
                    }

                    fs.SetLength(fs.Position);
                }
            }
            catch (Exception ex)
            {
                Logger?.Fatal(ex);

                throw ex;
            }
        }

        public void AppendRows(DocumentStorage storage, params string[] rows)
        {
            try
            {
                using (var fs = new FileStream(storage.Target, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite, BufferSize))
                {
                    SeekJsonArrayEndSquareBrackets(fs);

                    int rCount = rows.Length;

                    if (rCount == 1)
                    {
                        var row = Encoding.UTF8.GetBytes($",{rows[0]}]");
                        fs.Write(row, 0, row.Length);
                        fs.SetLength(fs.Position); //Only needed if new content may be smaller than old
                    }
                    else
                    {
                        int arrayTagCloseIndex = rCount - 1;

                        for (int i = 0; i < rCount; i++)
                        {
                            if (i == arrayTagCloseIndex)
                            {
                                var row = Encoding.UTF8.GetBytes($",{rows[i]}]");
                                fs.Write(row, 0, row.Length);
                            }
                            else
                            {
                                var row = Encoding.UTF8.GetBytes($",{rows[i]}");
                                fs.Write(row, 0, row.Length);
                            }
                        }
                    }

                    fs.SetLength(fs.Position);

                }
            }
            catch (Exception ex)
            {
                Logger?.Fatal(ex);

                throw ex;
            }
        }


        //Update ve Merge işlemleri için
        public void MergeRows(DocumentStorage storage, List<JObject> rows, DocumentMap map, Formatting formatting = Formatting.None)
        {
            try
            {
                using (FileStream fsTemp = new FileStream(storage.Tempory, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Write, BufferSize))
                using (StreamWriter sw = new StreamWriter(fsTemp, Encoding.UTF8, BufferSize))
                using (JsonTextWriter writer = new JsonTextWriter(sw))
                {
                    var match = 0;
                    var count = rows.Count;

                    writer.Formatting = formatting;
                    writer.WriteStartArray();

                    using (FileStream fsTarget = new FileStream(storage.Target, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize))
                    using (StreamReader sr = new StreamReader(fsTarget, Encoding.UTF8, false, BufferSize))
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        while (reader.Read())
                        {
                            if (reader.TokenType == JsonToken.StartObject)
                            {
                                JObject readRow = JObject.Load(reader);

                                if (match < count)
                                {
                                    var readId = readRow[map.Target].ToString();

                                    foreach (JObject row in rows)
                                    {
                                        var id = row[map.Source].ToString();

                                        if (readId == id)
                                        {
                                            readRow.Merge(row);
                                            match++;
                                            break;
                                        }

                                    }
                                }

                                readRow.WriteTo(writer);
                            }
                        }
                    }

                    writer.WriteEndArray();
                    writer.Flush();
                }
            }
            catch (Exception ex)
            {
                Logger?.Fatal(ex);

                throw ex;
            }
        }

        public void MergeRows<T>(DocumentStorage storage, List<T> rows, DocumentMap map, Formatting formatting = Formatting.None)
        {
            try
            {
                using (FileStream fsTemp = new FileStream(storage.Tempory, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Write, BufferSize))
                using (StreamWriter sw = new StreamWriter(fsTemp, Encoding.UTF8, BufferSize))
                using (JsonTextWriter writer = new JsonTextWriter(sw))
                {
                    var match = 0;
                    var count = rows.Count;

                    var settings = new JsonMergeSettings();
                    settings.MergeArrayHandling = MergeArrayHandling.Replace;
                    settings.PropertyNameComparison = StringComparison.OrdinalIgnoreCase;
                    settings.MergeNullValueHandling = MergeNullValueHandling.Merge;

                    writer.Formatting = formatting;
                    writer.WriteStartArray();

                    using (FileStream fsTarget = new FileStream(storage.Target, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize))
                    using (StreamReader sr = new StreamReader(fsTarget, Encoding.UTF8, false, BufferSize))
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        while (reader.Read())
                        {
                            if (reader.TokenType == JsonToken.StartObject)
                            {
                                JObject readRow = JObject.Load(reader);

                                if (match < count)
                                {
                                    var readId = readRow[map.Target].ToString();

                                    foreach (T row in rows)
                                    {
                                        JObject rowObject = JObject.FromObject(row);
                                        var id = rowObject[map.Source].ToString();

                                        if (readId == id)
                                        {
                                            readRow.Merge(rowObject, settings);
                                            match++;
                                            break;
                                        }

                                    }
                                }

                                readRow.WriteTo(writer);
                            }
                        }
                    }

                    writer.WriteEndArray();
                    writer.Flush();
                }

            }
            catch (Exception ex)
            {
                Logger?.Fatal(ex);

                throw ex;
            }
        }

        public void DeleteRows(DocumentStorage storage, List<JObject> rows, DocumentMap map, Formatting formatting = Formatting.None)
        {
            try
            {
                using (FileStream fsTemp = new FileStream(storage.Tempory, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Write, BufferSize))
                using (StreamWriter sw = new StreamWriter(fsTemp, Encoding.UTF8, BufferSize))
                using (JsonTextWriter writer = new JsonTextWriter(sw))
                {
                    var match = 0;
                    var count = rows.Count;

                    writer.Formatting = formatting;
                    writer.WriteStartArray();

                    using (FileStream fsTarget = new FileStream(storage.Target, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize))
                    using (StreamReader sr = new StreamReader(fsTarget, Encoding.UTF8, false, BufferSize))
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        while (reader.Read())
                        {
                            if (reader.TokenType == JsonToken.StartObject)
                            {
                                bool is_write = true;
                                JObject readRow = JObject.Load(reader);

                                if (match < count)
                                {
                                    var readId = readRow[map.Target].ToString();

                                    foreach (JObject row in rows)
                                    {
                                        var id = row[map.Source].ToString();

                                        if (readId == id)
                                        {
                                            is_write = false;
                                            match++;
                                            break;
                                        }

                                    }
                                }

                                if (is_write)
                                {
                                    readRow.WriteTo(writer);
                                }
                            }
                        }
                    }

                    writer.WriteEndArray();
                    writer.Flush();
                }
            }
            catch (Exception ex)
            {
                Logger?.Fatal(ex);

                throw ex;
            }
        }

        public void DeleteRows<T>(DocumentStorage storage, List<T> rows, DocumentMap map, Formatting formatting = Formatting.None)
        {
            try
            {
                using (FileStream fsTemp = new FileStream(storage.Tempory, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Write, BufferSize))
                using (StreamWriter sw = new StreamWriter(fsTemp, Encoding.UTF8, BufferSize))
                using (JsonTextWriter writer = new JsonTextWriter(sw))
                {
                    var match = 0;
                    var count = rows.Count;

                    writer.Formatting = formatting;
                    writer.WriteStartArray();

                    using (FileStream fsTarget = new FileStream(storage.Target, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize))
                    using (StreamReader sr = new StreamReader(fsTarget, Encoding.UTF8, false, BufferSize))
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        while (reader.Read())
                        {
                            if (reader.TokenType == JsonToken.StartObject)
                            {
                                bool is_write = true;
                                JObject readRow = JObject.Load(reader);

                                if (match < count)
                                {
                                    var readId = readRow[map.Target].ToString();

                                    foreach (T row in rows)
                                    {
                                        JObject rowObject = JObject.FromObject(row);
                                        var id = rowObject[map.Source].ToString();

                                        if (readId == id)
                                        {
                                            is_write = false;
                                            match++;
                                            break;
                                        }
                                    }
                                }

                                if (is_write)
                                {
                                    readRow.WriteTo(writer);
                                }
                            }
                        }
                    }

                    writer.WriteEndArray();
                    writer.Flush();
                }

            }
            catch (Exception ex)
            {
                Logger?.Fatal(ex);

                throw ex;
            }
        }



        //Ayrı bir dosya oluşturmak için.
        public void CreateRows<T>(DocumentStorage storage, List<T> rows, Formatting formatting = Formatting.None)
        {
            try
            {
                using (FileStream fsTarget = new FileStream(storage.Target, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write, BufferSize))
                using (StreamWriter sw = new StreamWriter(fsTarget))
                using (JsonTextWriter writer = new JsonTextWriter(sw))
                {
                    writer.Formatting = formatting;
                    writer.WriteStartArray();

                    foreach (T row in rows)
                    {
                        JObject.FromObject(row).WriteTo(writer);
                    }

                    writer.WriteEndArray();
                    writer.Flush();
                }

            }
            catch (Exception ex)
            {
                Logger?.Fatal(ex);

                throw ex;
            }
        }

        public void CreateRows(DocumentStorage storage, List<JObject> rows, Formatting formatting = Formatting.None)
        {
            try
            {
                using (FileStream fsTarget = new FileStream(storage.Target, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write, BufferSize))
                using (StreamWriter sw = new StreamWriter(fsTarget))
                using (JsonTextWriter writer = new JsonTextWriter(sw))
                {
                    writer.Formatting = formatting;
                    writer.WriteStartArray();

                    foreach (JObject row in rows)
                    {
                        row.WriteTo(writer);
                    }

                    writer.WriteEndArray();
                    writer.Flush();
                }

            }
            catch (Exception ex)
            {
                Logger?.Fatal(ex);

                throw ex;
            }
        }
    }
}
