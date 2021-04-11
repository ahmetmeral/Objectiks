using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using Objectiks.Engine;
using Objectiks.Helper;
using Objectiks.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Objectiks.Engine
{
    public class DocumentSerializer
    {
        private int BufferSize = 128;

        public DocumentSerializer(int? bufferSize = null)
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

        public static byte[] ToBson<T>(T obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BsonDataWriter writer = new BsonDataWriter(ms))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(writer, obj);

                    return ms.ToArray();
                }
            }
        }

        public static T FromBson<T>(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                using (BsonDataReader reader = new BsonDataReader(ms))
                {
                    JsonSerializer serializer = new JsonSerializer();

                    return serializer.Deserialize<T>(reader);
                }
            }
        }
    }
}
