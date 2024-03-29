﻿using Objectiks.Helper;
using Objectiks.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Objectiks
{
    public class DocumentProvider
    {
        public string Key { get; private set; }
        public string BaseDirectory { get; private set; }

        internal IDbConnection Connection { get; private set; }
        internal IDbTransaction Transaction { get; private set; }
        internal string ConnectionString { get; private set; }


        public DocumentProvider()
        {
            BaseDirectory = Path.Combine(Directory.GetCurrentDirectory(), DocumentDefaults.Root);
            Key = HashHelper.CreateMD5(BaseDirectory);
        }

        public DocumentProvider(string baseDirectory)
        {
            if (String.IsNullOrEmpty(baseDirectory))
            {
                baseDirectory = Path.Combine(Directory.GetCurrentDirectory(), DocumentDefaults.Root);
            }

            BaseDirectory = baseDirectory;
            Key = HashHelper.CreateMD5(baseDirectory);
        }

        internal DocumentProvider(IDbConnection connection)
        {
            Key = HashHelper.CreateMD5(connection.ConnectionString);
            BaseDirectory = Path.Combine(Directory.GetCurrentDirectory(), DocumentDefaults.Root);
            Connection = connection;
            ConnectionString = connection.ConnectionString;
        }

        internal DocumentProvider(IDbConnection connection, string baseDirectory)
        {
            BaseDirectory = baseDirectory;
            Key = HashHelper.CreateMD5(connection.ConnectionString);
            Connection = connection;
            ConnectionString = connection.ConnectionString;
        }

        public IDbConnection GetDbConnection()
        {
            if (Connection.State != ConnectionState.Open)
            {
                Connection.Open();
            }

            return Connection;
        }

        public string GetConnectionString()
        {
            return Connection.ConnectionString;
        }

        internal IDbTransaction BeginTransaction(IsolationLevel li)
        {
            Transaction = Connection.BeginTransaction(li);

            return Transaction;
        }

        internal void EnsureTransaction(IDbTransaction transaction)
        {
            Transaction = transaction;
        }

        internal void CommitTransaction()
        {
            Transaction?.Commit();
        }

        internal void RollbackTransaction()
        {
            Transaction?.Rollback();
        }

    }
}
