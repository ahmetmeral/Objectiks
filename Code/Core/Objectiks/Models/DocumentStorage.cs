using Objectiks.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Objectiks.Models
{
    public class DocumentStorage : IDisposable
    {
        private int TryCount = 10;

        public int Partition { get; set; }
        public string TypeOf { get; set; }
        public string BaseDirectory { get; set; }
        public string DirectoryName { get; set; }
        public string NameWithoutExtension { get; set; }
        public string Name { get; set; }

        public string Target { get; set; }
        public string Backup { get; set; }
        public string Tempory { get; set; }

        public bool Exist { get; set; }

        public DocumentStorage() { }

        public DocumentStorage(string typeOf, string baseDirectory, int partition = 0)
        {
            Partition = partition;
            TypeOf = typeOf;
            BaseDirectory = baseDirectory;
            DirectoryName = Path.Combine(BaseDirectory, DocumentDefaults.Documents, TypeOf);
            Name = Partition > 0 ? $"{TypeOf}.{Partition.ToString("0000")}.json" : $"{TypeOf}.json";
            NameWithoutExtension = Path.GetFileNameWithoutExtension(Name);
            Target = Path.Combine(DirectoryName, Name);
            Exist = new FileInfo(Target).Exists;
        }

        public DocumentStorage(string typeOf, string baseDirectory, FileInfo info)
        {
            TypeOf = typeOf;
            BaseDirectory = baseDirectory;
            DirectoryName = info.DirectoryName;
            Name = info.Name;
            NameWithoutExtension = Path.GetFileNameWithoutExtension(Name);
            Target = info.FullName;
            Partition = GetPartitionIndex(NameWithoutExtension);
            Exist = info.Exists;
        }

        public void BeginOperation()
        {
            string name = NameWithoutExtension;
            string ticks = DateTime.Now.Ticks.ToString();
            string operationStr = "None";

            Tempory = Path.Combine(DirectoryName, "Temp", $"Temp.{name}.{operationStr}.{ticks}.json");
            Backup = Path.Combine(DirectoryName, "Backup", $"Backup.{name}.{operationStr}.{ticks}.json");

            CheckDirectories();

            WhileTry.Try(() =>
            {
                return CreateTargetBackup();
            }, TryCount, true, $"Failed to create Target file backup typeOf: {TypeOf}");
        }

        private void CheckDirectories()
        {
            var temp = new FileInfo(Tempory);
            if (!temp.Directory.Exists)
            {
                temp.Directory.Create();
            }

            var backup = new FileInfo(Backup);
            if (!backup.Directory.Exists)
            {
                backup.Directory.Create();
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

                    File.Copy(Target, Backup);
                }

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

        private bool MoveTemporyToTarget()
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

        private bool MoveBackupToTarget()
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

        internal int GetPartitionIndex(string filenameWithoutExtention)
        {
            if (filenameWithoutExtention.IndexOf(".") != -1)
            {
                //Pages.0001
                var parts = filenameWithoutExtention.Split(".");
                if (parts.Length == 2)
                {
                    if (int.TryParse(parts[1], out var result))
                    {
                        //1
                        return result;
                    }
                    return -1;
                }
            }

            return 0;
        }

        internal void Rollback()
        {
            if (File.Exists(Backup))
            {
                WhileTry.Try(() =>
                {
                    return MoveBackupToTarget();
                }, TryCount, true, "Rollback : Backup move to target fail..");
            }
        }

        internal void Commit()
        {
            if (File.Exists(Tempory))
            {
                WhileTry.Try(() =>
                {
                    return MoveTemporyToTarget();
                }, TryCount, true, "Commit : Temp move to target fail..");
            }
        }

        internal void ClearBackup()
        {
            Delete(Backup);
        }

        internal void ClearTempory()
        {
            Delete(Tempory);
        }

        internal void ClearAll()
        {
            ClearBackup();
            ClearTempory();
        }

        public void Dispose()
        {
            ClearAll();
            GC.SuppressFinalize(this);
        }
    }
}
