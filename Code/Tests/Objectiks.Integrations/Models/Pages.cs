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
        [AccountOf]
        public int AccountRef { get; set; }
        [UserOf]
        public int UserRef { get; set; }
        public int CategoryRef { get; set; }

        [KeyOf]
        public string Name { get; set; }

        public string Language { get; set; }
        [Requried]
        public string Title { get; set; }
        public string File { get; set; }
        [Ignore]
        public string Contents { get; set; }
        [Ignore]
        public Categories Category { get; set; }
        [Ignore]
        public Tags[] Tags { get; set; }
        [Ignore]
        public Categories[] Categories { get; set; }
    }
}
