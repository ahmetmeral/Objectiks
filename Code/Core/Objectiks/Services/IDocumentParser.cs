using Newtonsoft.Json.Linq;
using Objectiks.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Services
{
    public interface IDocumentParser
    {
        string ParseOf { get; }

        bool IsParse(OperationType operation);
        void Parse(IDocumentEngine engine, DocumentMeta meta, Document document, DocumentStorage storage);
    }
}
