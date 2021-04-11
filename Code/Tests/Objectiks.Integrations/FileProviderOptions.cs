﻿using Objectiks.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Integrations
{
    public class FileProviderOptions : DocumentOption
    {
        public FileProviderOptions() : base()
        {
            var page = new DocumentSchema
            {
                TypeOf = "Pages",
                ParseOf = "Document",
                KeyOf = new DocumentKeyOfNames("Name"),
                Primary = "Id"
            };

            var category = new DocumentSchema
            {
                TypeOf = "Categories",
                ParseOf = "Document",
                KeyOf = new DocumentKeyOfNames(),
                Primary = "Id"
            };

            Name = "FileProvider";
            TypeOf = new DocumentTypes("Pages", "Categories");
            Schemes = new DocumentSchemes(page, category);
            BufferSize = 512;
        }
    }
}
