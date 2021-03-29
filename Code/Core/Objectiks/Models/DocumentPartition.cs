using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Models
{
    public class DocumentPartition
    {
        public int Partition { get; set; }
        public OperationType Operation { get; set; }

        public DocumentPartition(int partition, OperationType operation)
        {
            Partition = partition;
            Operation = operation;
        }

        public DocumentPartition(int partition, bool ifDocExits)
        {
            Partition = partition;
            Operation = ifDocExits ? OperationType.Merge : OperationType.Append;
        }
    }
}
