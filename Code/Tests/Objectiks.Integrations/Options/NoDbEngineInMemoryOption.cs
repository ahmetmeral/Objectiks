using Objectiks.Caching;
using Objectiks.Caching.Serializer;
using Objectiks.Engine;
using Objectiks.Models;
using Objectiks.NoDb;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Integrations.Option
{
    public class NoDbEngineInMemoryOption : NoDbDocumentOption
    {
        public NoDbEngineInMemoryOption() : base()
        {
            #region TypeOf
            var pages = new DocumentSchema
            {
                TypeOf = "Pages",
                ParseOf = "Document",
                PrimaryOf = "Id",
                WorkOf = "AccountRef",
                UserOf = "UserRef",
                KeyOf = new DocumentKeyOfNames("Tag")
            };

            var tags = new DocumentSchema
            {
                TypeOf = "Tags",
                ParseOf = "Document",
                PrimaryOf = "Id",
                KeyOf = new DocumentKeyOfNames()
            };
            #endregion

            Name = "NoDbEngineProvider";
            TypeOf = new DocumentTypes("Pages", "Tags");
            Schemes = new DocumentSchemes(pages, tags);
         
            UseDocumentWatcher<DocumentWatcher>();
        }
    }
}
