﻿using Newtonsoft.Json.Linq;
using Npgsql;
using Objectiks.Engine;
using Objectiks.Extentions;
using Objectiks.PostgreSql.Extentions;
using Objectiks.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Objectiks.PostgreSql.Engine
{
    public class PostgreReader : IDisposable
    {
        public DocumentQuery Query { get; private set; }
        public string TypeOf { get; private set; }
        public IEnumerable<JObject> Rows { get; set; }

        private int CurrentPage = 0;
        private int Limit = 0;
        private int Skip = 0;

        private DocumentProvider Provider;
        private DocumentOption Option;
        private IDocumentLogger Logger;

        private NpgsqlConnection Connection;

        public PostgreReader(string typeOf, DocumentProvider provider, DocumentOption option, IDocumentLogger logger)
        {
            TypeOf = typeOf;
            Provider = provider;
            Option = option;
            Logger = logger;
            Limit = option.SqlProviderDataReaderPageSize;
            Skip = 0;
            CurrentPage = 1;
        }

        public PostgreReader(DocumentProvider provider, DocumentOption option, DocumentQuery query, IDocumentLogger logger)
        {
            Provider = provider;
            Option = option;
            Query = query;
            Logger = logger;
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
                var compiler = new PostgreQueryCompiler(Option, Query);
                var selectQuery = compiler.Select();
                var command = new NpgsqlCommand(selectQuery, GetConnection());

                if (compiler.ValueBy.Count > 0)
                {
                    var index = -1;
                    foreach (var item in compiler.ValueBy)
                    {
                        index++;
                        command.AddNamedParameter($"@{index}", item);
                    }
                }

                var reader = command.ExecuteReader(CommandBehavior.Default);
                var hasRows = reader.HasRows;

                if (hasRows)
                {
                    Rows = reader.ToObjectList();
                }

                return hasRows;
            }
            catch (Exception ex)
            {
                Logger?.Error("PostgreSqlReader", ex);

                return false;
            }
        }

        public void NextPage()
        {
            CurrentPage = CurrentPage + 1;
            Skip = Limit * CurrentPage;
        }

        private string GetSelectSqlStatement()
        {
            if (Option.SupportSqlDataReaderPaging)
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
