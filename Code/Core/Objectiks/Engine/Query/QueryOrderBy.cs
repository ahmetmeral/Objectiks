using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Engine.Query
{
    public class QueryOrderBy : List<string>
    {
        public OrderByDirection Direction { get; set; } = OrderByDirection.None;
    }
}
