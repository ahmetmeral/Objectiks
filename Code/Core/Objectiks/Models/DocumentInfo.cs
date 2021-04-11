using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Objectiks.Models
{
    public class DocumentInfo
    {
        public int Partition { get; set; }
        public string TypeOfName { get; set; }
        public string BaseDirectory { get; set; }
        public string DirectoryName { get; set; }
        public string NameWithoutExtension { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public bool Exist { get; set; }

        public DocumentInfo(string typeOf, string baseDirectory, int partition = 0)
        {
            ChangeFileInfo(typeOf, baseDirectory, partition);
        }

        public DocumentInfo(string typeOf, string baseDirectory, FileInfo info)
        {
            TypeOfName = typeOf;
            BaseDirectory = baseDirectory;
            DirectoryName = info.DirectoryName;
            Name = info.Name;
            NameWithoutExtension = Path.GetFileNameWithoutExtension(Name);
            FullName = info.FullName;
            Partition = GetPartitionIndex(NameWithoutExtension);
            Exist = info.Exists;
        }

        public DocumentInfo TypeOf(string typeOf, int partition)
        {
            ChangeFileInfo(typeOf, BaseDirectory, partition);

            return this;
        }

        public DocumentInfo PartOf(int id)
        {
            ChangeFileInfo(TypeOfName, BaseDirectory, id);

            return this;
        }



        public DocumentInfo Delete()
        {
            var info = new FileInfo(FullName);
            if (info.Exists)
            {
                info.Delete();
            }

            return this;
        }

        public FileInfo GetFileInfo()
        {
            return new FileInfo(FullName);
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


        internal void ChangeFileInfo(string typeOf, string baseDirectory, int partition = 0)
        {
            Partition = partition;
            TypeOfName = typeOf;
            BaseDirectory = baseDirectory;
            DirectoryName = Path.Combine(BaseDirectory, DocumentDefaults.Documents, TypeOfName);
            Name = Partition > 0 ? $"{TypeOfName}.{Partition.ToString("0000")}.json" : $"{TypeOfName}.json";
            NameWithoutExtension = Path.GetFileNameWithoutExtension(Name);
            FullName = Path.Combine(DirectoryName, Name);
            Exist = new FileInfo(FullName).Exists;
        }

        internal void ChangeFileInfo(string typeOf, string baseDirectory, string file)
        {
            TypeOfName = typeOf;
            BaseDirectory = baseDirectory;
            DirectoryName = Path.Combine(BaseDirectory, DocumentDefaults.Documents, TypeOfName);
            Name = file;
            NameWithoutExtension = Path.GetFileNameWithoutExtension(Name);
            FullName = Path.Combine(DirectoryName, Name);
            Partition = GetPartitionIndex(NameWithoutExtension);
        }


    }
}
