using Newtonsoft.Json.Linq;
using Objectiks.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Services
{
    public interface IDocumentRefParser : IParser
    {
        bool IsValidRef(DocumentRef docRef);
        void Parse(IDocumentEngine engine, Document document, DocumentRef docRef);
    }
}
