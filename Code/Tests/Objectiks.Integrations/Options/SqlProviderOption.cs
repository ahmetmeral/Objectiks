using Objectiks.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace Objectiks.Integrations.Options
{
    public class SqlProviderOption : DocumentOption
    {
        public SqlProviderOption() : base()
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
                KeyOf = new DocumentKeyOfNames("Name"),
                Primary = "Id"
            };

            Name = "SqlProviderOption";
            DbProviderSchema = "dbo";
            DbProviderSchemaSeperator = ".";
            TypeOf = new DocumentTypes("Pages", "Categories");
            Schemes = new DocumentSchemes(page, category);
            SupportFileAppend = false;
            SupportPartialStorage = false;
            SupportTypeOfCache = true;
            SupportTypeOfRefs = false;
        }
    }
}
