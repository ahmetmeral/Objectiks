using Newtonsoft.Json;
using Objectiks.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Integrations.Models
{
    [TypeOf, Serializable]
    public class Pages
    {
        [Primary]
        public int Id { get; set; }

        [WorkOf, Requried]
        public int WorkSpaceRef { get; set; }

        [UserOf, Requried]
        public int UserRef { get; set; }

        [KeyOf, Requried]
        public string Tag { get; set; }

        [Requried]
        public string Title { get; set; }

        public string File { get; set; }

        [Ignore]
        public string Contents { get; set; }
    }
}

