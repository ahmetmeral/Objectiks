using Objectiks.Models;
using Objectiks.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Objectiks.Engine
{
    public class DocumentWatcher
    {
        private bool IsLocked = false;
        private string DocumentExtention;
        private string[] Extentions;
        private string[] Prefixs;

        public DocumentWatcher(IDocumentEngine engine, bool locked = true)
        {
            if (locked)
            {
                Lock();
            }

            DocumentExtention = engine.Manifest.Documents.Extention.Replace("*", "");
            Extentions = new string[] {
                DocumentExtention,
                ".md",
                ".html",
                ".txt"
            };
            Prefixs = new string[] { "Backup.", "Temp." };

            Watch(engine);
        }

        public void Lock()
        {
            IsLocked = true;
        }

        public void UnLock()
        {
            IsLocked = false;
        }

        private void Watch(IDocumentEngine engine)
        {
            var watcher = new FileSystemWatcher(engine.Connection.BaseDirectory);
            watcher.NotifyFilter = NotifyFilters.Attributes |
                NotifyFilters.CreationTime |
                NotifyFilters.FileName |
                //NotifyFilters.LastAccess |
                NotifyFilters.LastWrite |
                NotifyFilters.Size |
                NotifyFilters.Security;
            watcher.Filter = "*.*";
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;
            watcher.Changed += delegate (object sender, FileSystemEventArgs e)
            {
                if (!IsLocked)
                {
                    if (e.ChangeType == WatcherChangeTypes.Changed)
                    {
                        TryLoadDocumentType(engine, e);
                    }
                }
            };
        }

        private void TryLoadDocumentType(IDocumentEngine engine, FileSystemEventArgs e)
        {
            var file = new FileInfo(e.FullPath);

            if (!IsAcceptExtention(file.Extension) || CheckIgnorePrefix(file.FullName))
            {
                return;
            }

            var types = ObjectiksOf.Core.TypeOf;
            var typeOf = file.Directory.Name;
            if (typeOf == DocumentDefaults.Contents)
            {
                typeOf = file.Directory.Parent.Name;
            }

            if (types.Contains(typeOf.ToLowerInvariant()))
            {
                engine.LoadDocumentType(typeOf);

                foreach (var relationType in types)
                {
                    var meta = engine.GetTypeMeta(relationType);

                    if (meta.Refs != null &&
                        meta.Refs.Count(r => r.TypeOf == typeOf && r.Lazy == false && r.Disabled == false) > 0)
                    {
                        engine.LoadDocumentType(relationType);
                    }
                }
            }
        }

        private bool IsAcceptExtention(string fileExtention)
        {
            var isAccept = false;
            foreach (var extention in Extentions)
            {
                if (fileExtention.Contains(extention))
                {
                    isAccept = true;
                    break;
                }
            }

            return isAccept;
        }

        private bool CheckIgnorePrefix(string fullname)
        {
            bool ignored = false;
            string name = Path.GetFileNameWithoutExtension(fullname);

            foreach (var pref in Prefixs)
            {
                ignored = name.StartsWith(pref);
                if (ignored)
                {
                    break;
                }
            }

            return ignored;
        }
    }
}
