using Objectiks.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.PostgreSql
{
    public class PostgreSqlWriter : IDisposable
    {
        public string TypeOf { get; private set; }
        private DocumentProvider Provider;
        private DocumentOption Option;
        private IDocumentLogger Logger;

        public PostgreSqlWriter(string typeOf, DocumentProvider provider, DocumentOption option, IDocumentLogger logger)
        {
            TypeOf = typeOf;
            Provider = provider;
            Option = option;
            Logger = logger;
        }

        public void BulkCreate(List<Document> docs)
        {
            var conn = new Npgsql.NpgsqlConnection(Provider.GetConnectionString());
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
