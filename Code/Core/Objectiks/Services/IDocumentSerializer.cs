using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Services
{
    public interface IDocumentSerializer
    {
        T Deserialize<T>(byte[] serializedObject);
        byte[] Serialize(object item);
    }
}
