using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Services
{
    public interface IDocumentLogger
    {
        void Debug(DebugType debugType, string msg);
        void Debug(DebugType debugType, bool condition, string msg);
        void Info(string msg);
        void Error(Exception exception);
        void Error(string msg, Exception exception = null);
        void Fatal(string msg, Exception exception = null);
        void Fatal(Exception exception);
    }
}
