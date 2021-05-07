using Objectiks;
using Objectiks.Engine;
using Objectiks.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectik.Test.Web.Providers
{
    public class FileProviderOption : DocumentOption
    {
        public FileProviderOption() : base()
        {
            var page = new DocumentSchema
            {
                TypeOf = "Pages",
                ParseOf = "Document",
                KeyOf = new DocumentKeyOfNames("Name"),
                PrimaryOf = "Id"
            };

            var category = new DocumentSchema
            {
                TypeOf = "Categories",
                ParseOf = "Document",
                KeyOf = new DocumentKeyOfNames(),
                PrimaryOf = "Id"
            };

            Name = "FileProvider";
            TypeOf = new DocumentTypes("Pages", "Categories");
            Schemes = new DocumentSchemes(page, category);
            BufferSize = 512;

            RegisterDefaults();
            UseDocumentWatcher<DocumentWatcher>();
        }
    }
}
