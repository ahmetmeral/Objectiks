using Objectiks.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Integrations.Models
{
    [TypeOf("Tags"), Serializable]
    public class Tags
    {
        [Primary]
        public int Id { get; set; }

        [Requried]
        public string Name { get; set; }
    }
}
