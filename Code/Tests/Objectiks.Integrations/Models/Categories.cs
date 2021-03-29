using Objectiks.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Integrations.Models
{
    [TypeOf, Serializable]
    public class Categories
    {
        [Primary]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Language { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
