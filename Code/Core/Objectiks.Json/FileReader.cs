using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Objectiks.Json
{
    public static class FileReader
    {
        public static string Get(string path, int bufferSize = 128)
        {
            using (var trans = new DocumentTransaction(path, OperationType.Read, false))
            {
                try
                {
                    string contents = string.Empty;

                    using (var fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite, bufferSize))
                    using (var sr = new StreamReader(fs, Encoding.UTF8, false, bufferSize))
                    {
                        contents = sr.ReadToEnd();
                    }

                    trans.Commit();

                    return contents;
                }
                catch (Exception ex)
                {
                    trans.Rollback();

                    throw ex;
                }
            }
        }
    }
}
