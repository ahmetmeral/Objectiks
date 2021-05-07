using Newtonsoft.Json.Linq;
using Objectiks.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Services
{
    public interface IDocumentParser : IParser
    {
        void Parse(IDocumentEngine engine, DocumentMeta meta, Document document, DocumentStorage storage);
    }
}
