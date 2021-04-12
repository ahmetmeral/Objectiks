using Newtonsoft.Json.Linq;
using Npgsql;
using Objectiks.Extentions;
using Objectiks.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Objectiks.PostgreSql
{
    public class PostgreSqlReader : IDisposable
    {
        public string TypeOf { get; private set; }
        public IEnumerable<JObject> Rows { get; set; }

        private int CurrentPage = 0;
        private int Limit = 0;
        private int Skip = 0;

        private DocumentProvider Provider;
        private DocumentOption Option;
        private IDocumentLogger Logger;

        private NpgsqlConnection Connection;

        public PostgreSqlReader(string typeOf, DocumentProvider provider, DocumentOption option, IDocumentLogger logger)
        {
            TypeOf = typeOf;
            Provider = provider;
            Option = option;
            Logger = logger;
            Limit = option.SqlProviderPageLimit;
            Skip = 0;
            CurrentPage = 1;
        }

        private NpgsqlConnection GetConnection()
        {
            if (Connection != null)
            {
                if (Connection.State == ConnectionState.Open)
                {
                    Connection.Close();
                }
            }

            Connection = new NpgsqlConnection(Provider.GetConnectionString());
            Connection.Open();
            return Connection;
        }

        private void CloseConnection()
        {
            if (Connection != null && Connection.State == ConnectionState.Open)
            {
                Connection.Close();
                Connection.Dispose();
            }
        }


        public bool Read()
        {
            try
            {
                var command = new NpgsqlCommand(GetSelectSqlStatement(), GetConnection());
                var reader = command.ExecuteReader(CommandBehavior.Default);
                var hasRows = reader.HasRows;

                if (hasRows)
                {
                    Rows = reader.ToObjectList();
                }

                if (Option.SupportSqlReaderPaging)
                {
                    NextPage();
                }

                return hasRows;
            }
            catch (Exception ex)
            {
                Logger?.Error("PostgreSqlReader", ex);

                return false;
            }
        }

        private void NextPage()
        {
            CurrentPage = CurrentPage + 1;
            Skip = Limit * CurrentPage;
        }

        private string GetSelectSqlStatement()
        {
            if (Option.SupportSqlReaderPaging)
            {
                return $"SELECT * FROM {Option.SqlProviderSchema}{Option.SqlProviderSchemaSeperator}{TypeOf} LIMIT {Limit} OFFSET {Skip}";
            }
            return $"SELECT * FROM {Option.SqlProviderSchema}{Option.SqlProviderSchemaSeperator}{TypeOf}";
        }

        public void Dispose()
        {
            CloseConnection();
            GC.SuppressFinalize(this);
        }
    }
}
