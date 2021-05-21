using Newtonsoft.Json.Linq;
using Objectiks.Extentions;
using Objectiks.Models;
using Objectiks.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Objectiks.Parsers
{
    public class DocumentDefaultParser : IDocumentParser
    {
        public string ParseOf => "Document";


        public DocumentDefaultParser() { }

        public bool IsParse(OperationType operation)
        {
            return operation != OperationType.Delete;
        }

        public void Parse(IDocumentEngine engine, DocumentMeta meta, Document document, DocumentStorage file)
        {

        }
    }
}
