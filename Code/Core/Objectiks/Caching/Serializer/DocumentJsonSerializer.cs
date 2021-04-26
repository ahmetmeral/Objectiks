using Newtonsoft.Json;
using Objectiks.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Caching.Serializer
{
    public class DocumentJsonSerializer : IDocumentSerializer
    {
        private static readonly Encoding encoding = Encoding.UTF8;
        private readonly JsonSerializerSettings Settings;

        public DocumentJsonSerializer() : this(null) { }

        public DocumentJsonSerializer(JsonSerializerSettings settings)
        {
           Settings = settings ?? new JsonSerializerSettings();
        }

        public T Deserialize<T>(byte[] serializedObject)
        {
            var jsonString = encoding.GetString(serializedObject);
            return JsonConvert.DeserializeObject<T>(jsonString, Settings);
        }

        public byte[] Serialize(object item)
        {
            var type = item?.GetType();
            var jsonString = JsonConvert.SerializeObject(item, type, Settings);
            return encoding.GetBytes(jsonString);
        }
    }
}
