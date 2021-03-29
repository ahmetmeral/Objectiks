using Objectiks.Attributes;
using Objectiks.Engine;
using Objectiks.Engine.Query;
using Objectiks.Extentions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks
{
    public partial class ObjectiksOf
    {
        public DocumentEngine Engine { get; private set; }

        public ObjectiksOf(DocumentOptions options)
        {
            Engine = Core.Initialize(options);
        }

        public DocumentReader<T> TypeOf<T>()
        {
            return new DocumentReader<T>(Engine, GetTypeOfName<T>());
        }

        public DocumentReader<dynamic> TypeOf(string typeOf)
        {
            return new DocumentReader<dynamic>(Engine, typeOf);
        }

        public DocumentWriter<T> WriterOf<T>()
        {
            return new DocumentWriter<T>(Engine, GetTypeOfName<T>());
        }

        public DocumentWriter<T> WriterOf<T>(string typeOf)
        {
            return new DocumentWriter<T>(Engine, typeOf);
        }

        public T First<T>(string typeOf, object primaryOf) where T : class
        {
            var query = new QueryOf(typeOf);

            query.PrimaryOf(primaryOf);

            return Engine.Read<T>(query);
        }

        public T First<T>(object primaryOf) where T : class
        {
            var query = new QueryOf(GetTypeOfName<T>());

            query.PrimaryOf(primaryOf);

            return Engine.Read<T>(query);
        }

        public long Count<T>()
        {
            return Engine.GetCount<long>(new QueryOf(GetTypeOfName<T>()));
        }

        public long Count(string typeOf)
        {
            return Engine.GetCount<long>(new QueryOf(typeOf));
        }

        public DocumentMeta GetTypeMeta<T>()
        {
            return Engine.GetTypeMeta(GetTypeOfName<T>());
        }

        public DocumentMeta GetTypeMeta(string typeOf)
        {
            return Engine.GetTypeMeta(typeOf);
        }

        public List<T> ToList<T>(string typeOf)
        {
            return new DocumentReader<T>(Engine, typeOf).ToList();
        }

        public List<T> ToList<T>(params object[] keyOf)
        {
            return new DocumentReader<T>(Engine, GetTypeOfName<T>(), keyOf).ToList();
        }


        public bool NullReferenceEquals(dynamic obj)
        {
            return Object.ReferenceEquals(null, obj);
        }

        public List<DocumentMeta> GetTypeMetaAll()
        {
            return Engine.GetTypeMetaAll();
        }

        private string GetTypeOfName<T>()
        {
            var attr = typeof(T).GetCustomAttribute<TypeOfAttribute>();

            Ensure.NotNull(attr, "TypeOf undefined..");

            if (String.IsNullOrEmpty(attr.Name))
            {
                attr.Name = typeof(T).Name;
            }

            return attr.Name;
        }
    }
}
