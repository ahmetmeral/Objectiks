using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Objectiks.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Objectiks.Caching.Serializer
{
    public class DocumentBsonSerializer : IDocumentSerializer
    {
        public DocumentBsonSerializer() { }

        public T Deserialize<T>(byte[] serializedObject)
        {
            using (MemoryStream ms = new MemoryStream(serializedObject))
            {
                using (BsonDataReader reader = new BsonDataReader(ms))
                {
                    JsonSerializer serializer = new JsonSerializer();

                    return serializer.Deserialize<T>(reader);
                }
            }
        }

        public byte[] Serialize(object item)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BsonDataWriter writer = new BsonDataWriter(ms))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(writer, item, item.GetType());

                    return ms.ToArray();
                }
            }
        }
    }
}
