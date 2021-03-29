using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Objectiks;
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
            var options = new DocumentOptions(
                Path.Combine(Directory.GetCurrentDirectory(),
                "App_Data",
                DocumentDefaults.Root
                ));
            services.AddSingleton(options);
            services.AddSingleton<ObjectiksOf>();
        }

    }
}
