using Objectiks.Helper;
using Objectiks.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Objectiks
{
    public class DocumentProvider
    {
        public string Bucket { get; private set; }
        public string BaseDirectory { get; private set; }
        public IDbConnection Connection { get; private set; }

        public DocumentProvider()
        {
            BaseDirectory = Path.Combine(Directory.GetCurrentDirectory(), DocumentDefaults.Root);
            Bucket = HashHelper.CreateMD5(BaseDirectory);
        }

        public DocumentProvider(string baseDirectory)
        {
            if (String.IsNullOrEmpty(baseDirectory))
            {
                baseDirectory = Path.Combine(Directory.GetCurrentDirectory(), DocumentDefaults.Root);
            }

            BaseDirectory = baseDirectory;
            Bucket = HashHelper.CreateMD5(baseDirectory);
        }

        public DocumentProvider(IDbConnection connection)
        {
            BaseDirectory = Path.Combine(Directory.GetCurrentDirectory(), DocumentDefaults.Root);
            Bucket = HashHelper.CreateMD5(connection.ConnectionString);
        }

        public DocumentProvider(IDbConnection connection, string baseDirectory)
        {
            BaseDirectory = baseDirectory;
            Bucket = HashHelper.CreateMD5(connection.ConnectionString);
        }
    }
}
