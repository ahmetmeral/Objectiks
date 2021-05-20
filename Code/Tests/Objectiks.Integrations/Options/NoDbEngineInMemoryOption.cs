using Objectiks.Caching;
using Objectiks.Caching.Serializer;
using Objectiks.Engine;
using Objectiks.Integrations.Models;
using Objectiks.Models;
using Objectiks.NoDb;
using Objectiks.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Integrations.Option
{
    public class NoDbEngineInMemoryOption : NoDbDocumentOption
    {
        public NoDbEngineInMemoryOption() : base()
        {
            Name = "NoDbEngineProvider";

            RegisterTypeOf<Pages>();
            RegisterTypeOf<Tags>();

            //UseDocumentLogger<DocumentLogger>();
            //UseDocumentWatcher<DocumentWatcher>();
        }
    }

    public class LoggerModel
    {
        public ScopeType ScopeType { get; set; }
        public Exception Exception { get; set; }
        public string Message { get; set; }
    }

    public class DocumentLogger : IDocumentLogger
    {
        public static List<LoggerModel> Logs = new List<LoggerModel>();

        public void Debug(ScopeType debugType, string msg)
        {
            Logs.Add(new LoggerModel
            {
                ScopeType = debugType,
                Message = msg
            });
        }

        public void Debug(ScopeType debugType, bool condition, string msg)
        {
            if (condition)
            {
                Debug(debugType, msg);
            }
        }

        public void Error(Exception exception)
        {
            Logs.Add(new LoggerModel
            {
                Exception = exception
            });
        }

        public void Error(string msg, Exception exception = null)
        {
            Logs.Add(new LoggerModel
            {
                Message = msg,
                Exception = exception
            });
        }

        public void Fatal(string msg, Exception exception = null)
        {
            Logs.Add(new LoggerModel
            {
                Message = msg,
                Exception = exception
            });
        }

        public void Fatal(Exception exception)
        {
            Logs.Add(new LoggerModel
            {
                Exception = exception
            });
        }

        public void Info(string msg)
        {
            Logs.Add(new LoggerModel
            {
                Message = msg
            });
        }
    }
}
