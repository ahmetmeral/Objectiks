using Objectiks.Models;
using Objectiks.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Integrations.Parsers
{
    public class PageDocumentParser : IDocumentParser
    {
        public string ParseOf => "Pages";

        public bool IsParse(OperationType operation)
        {
            return operation == OperationType.Load;
        }

        public void Parse(IDocumentEngine engine, DocumentMeta meta, Document document, DocumentStorage storage)
        {
            
        }
    }
}
