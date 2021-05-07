using Objectiks.Models;
using Objectiks.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Objectiks.Engine
{
    public class DocumentWatcher : IDocumentWatcher
    {
        private bool IsLocked = false;
        private string DocumentExtention;
        private string[] Extentions;
        private string[] Prefixs;

        private DocumentEngine Engine = null;

        public DocumentWatcher() { }

        public virtual void Lock()
        {
            IsLocked = true;
        }

        public virtual void UnLock()
        {
            IsLocked = false;
        }

        public virtual void WaitForChanged(DocumentEngine engine)
        {
            Engine = engine;
            Prefixs = new string[] { "Backup.", "Temp." };
            DocumentExtention = engine.Option.Extention.Replace("*", "");
            Extentions = new string[] {
                DocumentExtention,
                ".md",
                ".html",
                ".txt"
            };

            engine.Logger?.Debug(ScopeType.Writer, $"Watch Directory : {engine.Provider.BaseDirectory}");

            var watcher = new FileSystemWatcher(engine.Provider.BaseDirectory);
            watcher.NotifyFilter = NotifyFilters.Attributes |
                NotifyFilters.CreationTime |
                NotifyFilters.FileName |
                //NotifyFilters.LastAccess |
                NotifyFilters.LastWrite |
                NotifyFilters.Size |
                NotifyFilters.Security;
            watcher.Filter = engine.Option.Extention;
            watcher.InternalBufferSize = 8192 * 5;
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;
            watcher.Changed += delegate (object sender, FileSystemEventArgs e)
            {
                if (!IsLocked)
                {
                    if (e.ChangeType == WatcherChangeTypes.Changed)
                    {
                        OnChangeDocument(e);
                    }
                }
            };
        }

        public virtual void OnChangeDocument(FileSystemEventArgs e)
        {
            try
            {
                var file = new FileInfo(e.FullPath);

                if (!IsAcceptExtention(file.Extension) || CheckIgnorePrefix(file.FullName))
                {
                    Engine.Logger?.Debug(ScopeType.Watcher, $"Skip File : {file.FullName}");

                    return;
                }

                var types = Engine.Option.TypeOf;
                var typeOf = file.Directory.Name;
                if (typeOf == DocumentDefaults.Contents)
                {
                    typeOf = file.Directory.Parent.Name;

                    Engine.Logger?.Debug(ScopeType.Watcher, $"TypeOf: {typeOf} Contents changes");
                }

                if (types.Contains(typeOf.ToLowerInvariant()))
                {
                    Engine.Logger?.Debug(ScopeType.Watcher, $"OnChangeDocument TypeOf: {typeOf} - File: {file.FullName}");

                    Engine.LoadDocumentType(typeOf);
                }
            }
            catch (Exception ex)
            {
                Engine.Logger?.Error("Document watcher OnChangeDocument:", ex);
            }
        }

        protected virtual bool IsAcceptExtention(string fileExtention)
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

        protected virtual bool CheckIgnorePrefix(string fullname)
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
