using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Objectiks.Attributes;
using Objectiks.Engine;
using Objectiks.Engine.Query;
using Objectiks.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

namespace Objectiks.Extentions
{
    public static class DocumentExtentions
    {
        public static string[] ToKeyOfValues(this JObject target, string typeOf, DocumentKeyOfNames keyOfNames, string primary)
        {
            var keyOfProperties = keyOfNames;
            var keyOfValues = new List<string>();

            if (!keyOfProperties.Contains(primary))
            {
                keyOfProperties.Add(primary);
            }

            foreach (var key in keyOfProperties)
            {
                if (target.ContainsKey(key))
                {
                    var keyValue = target[key].AsString().ToLowerInvariant();

                    if (!String.IsNullOrEmpty(keyValue))
                    {
                        keyOfValues.Add(keyValue);
                    }
                }
                else
                {
                    throw new ArgumentNullException($"{typeOf} Schema Key {key} undefined..");
                }
            }

            return keyOfValues.ToArray();
        }

        public static bool AsBool(this DocumentVars obj, string key)
        {
            if (obj.ContainsKey(key))
            {
                bool.TryParse(obj[key].ToString(), out var isBool);

                return isBool;
            }
            return false;
        }

        public static string AsHashString(this Document document)
        {
            return ByteArrayToString(new MD5CryptoServiceProvider().ComputeHash(ObjectToByteArray(document)));
        }

        public static byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public static string ByteArrayToString(byte[] arrInput)
        {
            int i;
            StringBuilder sOutput = new StringBuilder(arrInput.Length);
            for (i = 0; i < arrInput.Length - 1; i++)
            {
                sOutput.Append(arrInput[i].ToString("X2"));
            }
            return sOutput.ToString();
        }

        public static string GetTargetProperty(this DocumentRef docRef)
        {
            var property = docRef.TypeOf;

            if (docRef.MapOf != null)
            {
                if (!String.IsNullOrWhiteSpace(docRef.MapOf.Source))
                {
                    property = docRef.MapOf.Source;
                }
            }

            return property;
        }

        public static bool HasArray(this object obj)
        {
            if (obj != null)
            {
                return obj.GetType().Name == "JArray";
            }

            return false;
        }

        public static QueryOf GetQueryOfFromPrimaryOf(this List<DocumentKey> keys, string typeOf)
        {
            var query = new QueryOf(typeOf);

            foreach (var key in keys)
            {
                query.PrimaryOf(key.PrimaryOf);
            }

            return query;
        }

        public static bool IsNull(this DocumentKey? key)
        {
            return key.HasValue;
        }

        public static bool IsNull(this object obj)
        {
            return obj == null;
        }

        public static T ConvertToType<T>(this object value) where T : class, new()
        {
            var serializer = new DocumentSerializer();
            var jsonData = serializer.Serialize(value);

            return serializer.Deserialize<T>(jsonData);
        }

        internal static T As<T>(this object obj) where T : class, new()
        {
            return obj as T;
        }

        internal static bool HasProperty(this ExpandoObject obj, string propertyName)
        {
            if (obj == null)
            {
                return false;
            }
            return ((IDictionary<string, object>)obj).ContainsKey(propertyName);
        }

        internal static bool HasProperty(this object obj, string propertyName)
        {
            if (obj == null)
            {
                return false;
            }
            return obj.GetType().GetProperty(propertyName) != null;
        }

        internal static IDictionary<string, object> ToDictionary(this object obj)
        {
            var expando = new Dictionary<string, object>();
            dynamic dynamic = obj;

            foreach (var dic in dynamic)
            {
                expando[dic.Key] = dic.Value;
            }

            return expando;
        }

        internal static string AsString(this object obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }
            return obj.ToString();
        }

        internal static string ToJoin(this List<string> list, string separator = ",")
        {
            if (list != null)
            {
                return string.Join(separator, list);
            }

            return string.Empty;
        }
    }
}
