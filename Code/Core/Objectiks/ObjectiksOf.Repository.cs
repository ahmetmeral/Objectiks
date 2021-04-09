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
        public DocumentProvider Engine { get; private set; }

        public ObjectiksOf()
        {
            Core.Initialize(new ObjectiksOptions());
        }

        public ObjectiksOf(ObjectiksOptions options)
        {
            Core.Initialize(options);
        }

        public ObjectiksOf(string baseDirectory)
        {
            Core.Initialize(new ObjectiksOptions(baseDirectory));
        }

        public DocumentReader<T> TypeOf<T>()
        {
            return new DocumentReader<T>(GetTypeOfName<T>());
        }

        public DocumentReader<dynamic> TypeOf(string typeOf)
        {
            return new DocumentReader<dynamic>(typeOf);
        }

        public DocumentWriter<T> WriterOf<T>()
        {
            return new DocumentWriter<T>(GetTypeOfName<T>());
        }

        public DocumentWriter<T> WriterOf<T>(string typeOf)
        {
            return new DocumentWriter<T>(GetTypeOfName<T>());
        }

        public T First<T>(string typeOf, object primaryOf) where T : class
        {
            return new DocumentReader<T>(typeOf, primaryOf).First();
        }

        public T First<T>(object primaryOf) where T : class
        {
            return new DocumentReader<T>(GetTypeOfName<T>(), primaryOf).First();
        }

        public long Count<T>()
        {
            return Core.GetTypeOfProvider<T>().GetCount<long>(new QueryOf(GetTypeOfName<T>()));
        }

        public long Count(string typeOf)
        {
            return Core.GetTypeOfProvider(typeOf).GetCount<long>(new QueryOf(typeOf));
        }

        public DocumentMeta GetTypeMeta<T>()
        {
            return Core.GetTypeMeta(GetTypeOfName<T>());
        }

        public DocumentMeta GetTypeMeta(string typeOf)
        {
            return Core.GetTypeMeta(typeOf);
        }

        public List<T> ToList<T>(string typeOf)
        {
            return new DocumentReader<T>(typeOf).ToList();
        }

        public List<T> ToList<T>(params object[] keyOf)
        {
            return new DocumentReader<T>(GetTypeOfName<T>(), keyOf).ToList();
        }


        public bool NullReferenceEquals(dynamic obj)
        {
            return Object.ReferenceEquals(null, obj);
        }

        public List<DocumentMeta> GetTypeMetaAll()
        {
            return Core.GetTypeMetaAll();
        }

        public string GetTypeOfName<T>()
        {
            return Core.GetTypeOfName<T>();
        }
    }
}
