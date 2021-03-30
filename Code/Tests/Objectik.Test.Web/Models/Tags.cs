using Objectiks.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectik.Test.Web.Models
{
    [TypeOf, Serializable]
    public class Tags
    {
        [Primary]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Language { get; set; }
    }
}
