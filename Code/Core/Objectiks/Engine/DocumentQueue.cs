using Objectiks.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Engine
{
    public class DocumentQueue
    {
        public Document Document { get; set; }
        public DocumentPartition PartOf { get; set; }

        public DocumentQueue(Document document, DocumentPartition partOf)
        {
            Document = document;
            PartOf = partOf;
        }
    }
}
