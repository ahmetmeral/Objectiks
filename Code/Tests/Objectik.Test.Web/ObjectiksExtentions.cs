using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Objectiks;
using Objectiks.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Objectik.Test.Web
{
    public class DocumentLogger : IDocumentLogger
    {
        public void Debug(DebugType debugType, string msg)
        {
            
        }

        public void Debug(DebugType debugType, bool condition, string msg)
        {
            
        }

        public void Error(Exception exception)
        {
            
        }

        public void Error(string msg, Exception exception = null)
        {
            
        }

        public void Fatal(string msg, Exception exception = null)
        {
            
        }

        public void Fatal(Exception exception)
        {
            
        }

        public void Info(string msg)
        {
            
        }
    }

    public static class ObjectiksExtentions
    {
        public static void AddObjectiks(this IServiceCollection services)
        {
            var options = new DocumentOptions(
                Path.Combine(Directory.GetCurrentDirectory(),
                "App_Data",
                DocumentDefaults.Root
                ));
            options.UseDocumentLogger<DocumentLogger>();

            services.AddSingleton(options);
            services.AddSingleton<ObjectiksOf>();
        }

    }
}
