using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Models
{
    internal class DocumentTransactionContext
    {
        public string TypeOf { get; set; }
        public int Partition { get; set; }
        public OperationType Operation { get; set; }
        public DocumentStorage Storage { get; set; }
        public List<Document> Documents { get; set; }
    }
}
