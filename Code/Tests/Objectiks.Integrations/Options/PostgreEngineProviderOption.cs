using Objectiks.Models;
using Objectiks.PostgreSql;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace Objectiks.Integrations.Options
{
    public class PostgreEngineProviderOption : DocumentOption
    {
        public PostgreEngineProviderOption() : base()
        {
            var page = new DocumentSchema
            {
                TypeOf = "Pages",
                ParseOf = "Document",
                KeyOf = new DocumentKeyOfNames("name"),
                Primary = "id"
            };

            Name = "SqlProviderOption";
            SqlProviderSchema = "public";
            SqlProviderSchemaSeperator = ".";
            TypeOf = new DocumentTypes("Pages");
            Schemes = new DocumentSchemes(page);
            SupportFileAppend = false;
            SupportPartialStorage = false;
            SupportTypeOfRefs = false;

            RegisterDefaultTypeOrParser();

            UseEngineProvider<PostgreSqlEngine>();
        }
    }
}
