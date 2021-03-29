using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Services
{
    public interface IDocumentConnection
    {
        int Port { get; }
        string Host { get; set; }
        string ConnectionString { get; set; }
        string BaseDirectory { get; set; }
        string Username { get; set; }
        string Password { get; set; }
    }
}
