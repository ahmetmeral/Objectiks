﻿using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Objectik.Test.Web.Providers;
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
        public void Debug(ScopeType debugType, string msg)
        {

        }

        public void Debug(ScopeType debugType, bool condition, string msg)
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
            //ObjectiksOf.Core.Map(typeof(DocumentProvider), new FileProviderOption());
            var baseDirectory = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", DocumentDefaults.Root);
            services.AddSingleton(new ObjectiksOf(baseDirectory));
        }

    }
}
