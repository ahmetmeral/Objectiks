using Objectiks.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectik.Test.Web.Models
{
    [Serializable]
    [TypeOf]
    public class Sections
    {
        [Primary]
        public int Id { get; set; }

        [KeyOf]
        public string Name { get; set; }

        [KeyOf]
        public string Language { get; set; }

        public int GroupRef { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public Sections Childs { get; set; }
    }
}
