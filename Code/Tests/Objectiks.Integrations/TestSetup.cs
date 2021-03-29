using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Objectiks.Services;
using Objectiks.InMemory;
using Objectiks.Json;
using Objectiks.Json.Parser;
using Objectiks.Integrations.Models;
using System.IO;

namespace Objectiks.Integrations
{
    public static class TestSetup
    {
        private static Random random = new Random(1);
        private static List<int> idList = new List<int>();

        public static DocumentOptions Options
        {
            get
            {
                var baseDirectory = Path.Combine(Directory.GetCurrentDirectory(),
                    DocumentDefaults.Root);

                var options = new DocumentOptions();
                options.UseCacheTypeOf<DocumentInMemory>();
                options.UseEngineTypeOf<JsonEngine>();
                options.AddDefaultParsers();
                options.UseConnection(new DocumentConnection
                {
                    BaseDirectory = baseDirectory
                });
               

                return options;
            }
        }

        public static int GenerateNewId()
        {
            int? new_id;
            while (true)
            {
                new_id = random.Next(1, 100000000);
                if (!idList.Contains(new_id.Value))
                {
                    idList.Add(new_id.Value);
                    break;
                }
            }
            return new_id.Value;
        }

        public static List<Pages> GeneratePages(int size)
        {
            var rows = new List<Pages>();

            for (int i = 0; i < size; i++)
            {
                rows.Add(new Pages
                {
                    Id = GenerateNewId(),
                    Title = "Home Tr Page Yaptık..1 "
                });
            }

            return rows;
        }

        public static List<JObject> GetPagesJObject(int rowCount = 10)
        {
            var rows = new List<JObject>();

            for (int i = 0; i < rowCount; i++)
            {
                rows.Add(JObject.FromObject(new Pages
                {
                    Id = GenerateNewId(),
                    Title = "Home Tr Page Yaptık..1 ",
                    CategoryRef = 1
                }));
            }

            return rows;
        }

    }
}
