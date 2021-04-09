using Newtonsoft.Json.Linq;
using Objectiks.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Services
{
    public interface IDocumentParser : IParser
    {
        void Parse(IDocumentProvider provider, DocumentMeta meta, Document document, DocumentInfo file);
    }
}
