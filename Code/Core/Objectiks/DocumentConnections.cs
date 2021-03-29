using Objectiks.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Objectiks
{
    public class DocumentConnection : IDocumentConnection
    {
        public string ConnectionString { get;  set; }
        public string BaseDirectory { get;  set; }
        public int Port { get; set; }
        public string Host { get;  set; }
        public string Username { get;  set; }
        public string Password { get;  set; }

        public DocumentConnection()
        {
            BaseDirectory = Path.Combine(Directory.GetCurrentDirectory(), DocumentDefaults.Root);
        }
    }
}
