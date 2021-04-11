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
        public string CacheBucket { get; private set; }
        public string BaseDirectory { get; private set; }

        internal IDbConnection Connection { get; private set; }

        public DocumentProvider()
        {
            BaseDirectory = Path.Combine(Directory.GetCurrentDirectory(), DocumentDefaults.Root);
            CacheBucket = HashHelper.CreateMD5(BaseDirectory);
        }

        public DocumentProvider(string baseDirectory)
        {
            if (String.IsNullOrEmpty(baseDirectory))
            {
                baseDirectory = Path.Combine(Directory.GetCurrentDirectory(), DocumentDefaults.Root);
            }

            BaseDirectory = baseDirectory;
            CacheBucket = HashHelper.CreateMD5(baseDirectory);
        }

        internal DocumentProvider(IDbConnection connection)
        {
            BaseDirectory = Path.Combine(Directory.GetCurrentDirectory(), DocumentDefaults.Root);
            CacheBucket = HashHelper.CreateMD5(connection.ConnectionString);
        }

        internal DocumentProvider(IDbConnection connection, string baseDirectory)
        {
            BaseDirectory = baseDirectory;
            CacheBucket = HashHelper.CreateMD5(connection.ConnectionString);
        }
    }
}
