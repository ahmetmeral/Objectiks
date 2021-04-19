using Objectiks.Helper;
using Objectiks.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;


namespace Objectiks.Engine
{
    public class DocumentInternalTransaction : IDisposable
    {
        static object LockObject = new object();

        private int LockedTryCount = 25;
        private static ConcurrentDictionary<string, bool> Locked = new System.Collections.Concurrent.ConcurrentDictionary<string, bool>();

        private DocumentStorage Document = null;
        private int TryCount = 10;

        public string Tempory { get; private set; }
        public string Target { get; private set; }
        public string Backup { get; private set; }
        public bool IsBackup { get; private set; }
        public OperationType Operation { get; set; }

        public DocumentInternalTransaction(string path, OperationType operation, bool isBackup)
        {
            var document = new DocumentStorage("Stream", String.Empty, new FileInfo(path));
            Initialize(document, operation, isBackup);
        }

        public DocumentInternalTransaction(DocumentStorage document, OperationType operation, bool isBackup = true)
        {
            Initialize(document, operation, isBackup);
        }

        private void Initialize(DocumentStorage document, OperationType operation, bool isBackup = true)
        {
            Document = document;
            Operation = operation;
            IsBackup = isBackup;
            Target = document.Target;

            /*
            Operations : 
              None,
              Create, no effects..
              Append, Backup, Rollback,Commit
              Merge,  Backup,Temp,Rollback,Commit
              Delete  Backup,Temp,Rollback,Commit
            */

            bool isLocked = FileLocked(document.NameWithoutExtension);

            if (isLocked) { throw new Exception("Document is locked.."); }

            if (operation != OperationType.Create)
            {
                string name = document.NameWithoutExtension;
                string ticks = DateTime.Now.Ticks.ToString();
                string operationStr = operation.ToString();

                Tempory = Path.Combine(document.DirectoryName, "Temp", $"Temp.{name}.{operationStr}.{ticks}.json");
                Backup = Path.Combine(document.DirectoryName, "Backup", $"Backup.{name}.{operationStr}.{ticks}.json");

                CheckDirectories();

                if (isBackup)
                {
                    WhileTry.Try(() =>
                    {
                        return CreateTargetBackup();
                    }, TryCount, true, "Failed to create Target file backup");
                }
            }
        }

        private bool FileLocked(string name)
        {
            int check = 0;
            bool locked = true;

            if (!Locked.ContainsKey(name))
            {
                locked = false;
                Locked.TryAdd(name, true);

                return locked;
            }
            else
            {
                lock (LockObject)
                {
                    while (true)
                    {
                        check++;

                        if (Locked.ContainsKey(name))
                        {
                            Thread.Sleep(3000);
                        }
                        else if (FileHelper.IsFileLocked(Target))
                        {
                            //farklı bir uygulama üzerinden erişim varmı kontrol ediyoruz.
                            Thread.Sleep(3000);
                        }
                        else
                        {
                            Thread.Sleep(3000);
                            break;
                        }

                        if (check > LockedTryCount)
                        {
                            break;
                        }
                    }

                    Locked.TryAdd(name, true);

                    locked = false;

                    return locked;
                }
            }
        }

        private bool FileUnlocked(string name)
        {
            Locked.TryRemove(name, out var locked);

            return locked;
        }

        private void CheckDirectories()
        {
            var temp = new FileInfo(Tempory);
            if (!temp.Directory.Exists)
            {
                temp.Directory.Create();
            }

            if (IsBackup)
            {
                var backup = new FileInfo(Backup);
                if (!backup.Directory.Exists)
                {
                    backup.Directory.Create();
                }
            }
            else
            {
                Backup = string.Empty;
            }
        }

        private bool CreateTargetBackup()
        {
            try
            {
                //varsa siliyoruz..
                if (File.Exists(Backup))
                {
                    try
                    {
                        File.Delete(Backup);
                    }
                    catch { }
                }

                File.Copy(Target, Backup);

                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch (IOException)
            {
                // Show a message to the user
                return false;
            }
        }

        private bool Delete(string file)
        {
            try
            {
                if (!String.IsNullOrWhiteSpace(file) && File.Exists(file))
                {
                    File.Delete(file);

                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private bool TempMoveToTarget()
        {
            try
            {
                if (Delete(Target))
                {
                    new FileInfo(Tempory).MoveTo(Target);

                    return true;
                }

                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch (IOException)
            {
                return false;
            }
        }

        private bool BackupMoveToTarget()
        {
            try
            {
                if (Delete(Target))
                {
                    new FileInfo(Backup).MoveTo(Target);

                    return true;
                }

                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch (IOException)
            {
                return false;
            }
        }

        public void Rollback()
        {
            if (IsBackup)
            {
                WhileTry.Try(() =>
                {
                    return BackupMoveToTarget();
                }, TryCount, true, "Rollback : Backup move to target fail..");

                FileUnlocked(Document.NameWithoutExtension);
            }
            else
            {
                FileUnlocked(Document.NameWithoutExtension);

                if (Operation != OperationType.Read)
                    throw new Exception("Backup file not found --> Backup=false");
            }
        }

        public void Commit(bool clearAll = true)
        {
            if (Operation == OperationType.Merge || Operation == OperationType.Delete)
            {
                if (File.Exists(Tempory))
                {
                    WhileTry.Try(() =>
                    {
                        return TempMoveToTarget();
                    }, TryCount, true, "Commit : Temp move to target fail..");
                }
            }

            FileUnlocked(Document.NameWithoutExtension);
        }

        public void ClearBackup()
        {
            if (IsBackup)
            {
                Delete(Backup);
            }
        }

        public void ClearTempory()
        {
            Delete(Tempory);
        }

        public void ClearAll()
        {
            ClearBackup();
            ClearTempory();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
