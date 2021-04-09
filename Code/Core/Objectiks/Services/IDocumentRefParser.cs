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
        void Parse(IDocumentProvider provider, Document document, DocumentRef docRef);
    }
}
