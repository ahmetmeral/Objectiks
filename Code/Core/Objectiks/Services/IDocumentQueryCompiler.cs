using Objectiks.Engine.Query;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Services
{
    public interface IDocumentQueryCompiler
    {
        string OrderBy { get; set; }
        int Skip { get; set; }
        int Take { get; set; }
        string TypeOf { get; set; }
        QueryValues ValueBy { get; set; }
        string WhereBy { get; set; }
    }
}
