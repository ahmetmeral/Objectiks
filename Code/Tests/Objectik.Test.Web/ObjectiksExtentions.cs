using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Objectiks;
using Objectiks.InMemory;
using Objectiks.Json;
using Objectiks.Json.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Objectik.Test.Web
{
    public static class ObjectiksExtentions
    {
        public static void AddObjectiks(this IServiceCollection services)
        {
            var connection = new DocumentConnection();
            connection.BaseDirectory = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", DocumentDefaults.Root);

            var options = new DocumentOptions();
            options.AddDefaultParsers();
            options.UseCacheTypeOf<DocumentInMemory>();
            options.UseEngineTypeOf<JsonEngine>();
            options.UseConnection(connection);

            services.AddSingleton(options);
            services.AddSingleton<ObjectiksOf>();
        }

    }
}
