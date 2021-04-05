using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Services
{
    public interface IDocumentLogger
    {
        void Debug(DebugType debugType, string msg);
        void Info(string msg);
        void Error(string msg, Exception exception = null);
        void Fatal(string msg, Exception exception = null);
    }
}
