using Objectiks.Caching;
using Objectiks.Caching.Serializer;
using Objectiks.Models;
using Objectiks.PostgreSql;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace Objectiks.Integrations.Options
{
    public class PostgreEngineRedisOption : DocumentOption
    {
        public PostgreEngineRedisOption() : base()
        {
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


            Name = "PostgreEngineProvider";
            SqlProviderSchema = "public";
            SqlProviderSchemaSeperator = ".";
            TypeOf = new DocumentTypes("Pages", "Tags");
            Schemes = new DocumentSchemes(pages, tags);

            SupportFileAppend = false;
            SupportPartialStorage = false;
            SupportTypeOfRefs = false;

            var cacheProvider = new DocumentInMemory(Name, new DocumentBsonSerializer());

            UseCacheProvider(cacheProvider);
            UseEngineProvider<PostgreSqlEngine>();
            RegisterDefaultTypeOrParser();
        }
    }
}
