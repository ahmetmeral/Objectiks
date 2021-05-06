using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Objectiks.Integrations.Models;
using System.IO;
using Objectiks.Caching;
using Objectiks.Engine;

namespace Objectiks.Integrations
{
    public static class TestSetup
    {
        private static Random random = new Random(1);
        private static List<int> idList = new List<int>();
        private static Random randomGroup = new Random(1);
        private static Random randomWorkspace = new Random(1);
        private static Random randomUser = new Random(1);

        public static string[] Tags = new string[] { "Admin", "User", "Manager" };
        public static int[] WorkspaceIDs = new int[] { 1, 2, 3, 4 };
        public static int[] UsersIDs = new int[] { 1, 2, 3, 4 };

        public static DocumentOption Options
        {
            get
            {
                return new DocumentOption();
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

        public static string GetTagName()
        {
            return Tags[randomGroup.Next(1, Tags.Length)];
        }

        public static int GetUserID()
        {
            return UsersIDs[randomUser.Next(1, UsersIDs.Length)];
        }

        public static int GetWorkSpaceID()
        {
            return WorkspaceIDs[randomWorkspace.Next(1, WorkspaceIDs.Length)];
        }

        public static List<Pages> GeneratePages(int size, bool auto_id = true, bool workOf = false)
        {
            var rows = new List<Pages>();

            for (int i = 0; i < size; i++)
            {
                if (workOf)
                {
                    rows.Add(new Pages
                    {
                        Id = auto_id ? GenerateNewId() : 0,
                        WorkSpaceRef = GetWorkSpaceID(),
                        UserRef = GetUserID(),
                        Title = "Home Tr Page Yaptık..1 ",
                        Tag = GetTagName()
                    });
                }
                else
                {
                    rows.Add(new Pages
                    {
                        Id = auto_id ? GenerateNewId() : 0,
                        Title = "Home Tr Page Yaptık..1 ",
                        Tag = GetTagName()
                    });
                }
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
                    Tag = GetTagName()
                }));
            }

            return rows;
        }
    }
}
