using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Engine.Query
{
    public class QueryParameters : List<QueryParameter>
    {

    }

    public class QueryParameter
    {
        public string Field { get; set; }
        public object Value { get; set; }
        public OperationType Operator { get; set; }
        public ParameterType Type { get; set; }
    }
}
