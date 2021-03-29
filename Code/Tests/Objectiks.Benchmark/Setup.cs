using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using Objectiks.Benchmark.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Benchmark
{
    public static class Setup
    {
        private static Random random = new Random(1);
        private static List<int> idList = new List<int>();


        public static DocumentOptions Options
        {
            get
            {
                return new DocumentOptions();
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
                    Title = "Home Tr Page Yaptık..1 ",
                    GroupRef = 10
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
                    GroupRef = 10
                }));
            }

            return rows;
        }

    }
}
