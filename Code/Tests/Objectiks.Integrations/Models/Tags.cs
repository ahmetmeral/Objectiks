using Objectiks.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Integrations.Models
{
    [TypeOf("Tags"), Serializable]
    [CacheOf(1000, true)]
    public class Tags
    {
        [Primary]
        public int Id { get; set; }

        [Requried]
        public string Name { get; set; }
    }
}
