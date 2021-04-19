using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Models
{
    public class DocumentContext
    {
        public string TypeOf { get; set; }
        public string Primary { get; set; }
        public OperationType Operation { get; set; }
        public DocumentStorage Storage { get; set; }
        public List<Document> Documents { get; set; }
        public int Partition { get; set; }
        public Format Formatting { get; set; } = Format.None;

        public bool HasDocuments
        {
            get { return Documents != null && Documents.Count > 0; }
        }
    }
}
