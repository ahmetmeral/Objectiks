using Objectiks.Engine;
using Objectiks.Extentions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Objectiks.Helper
{
    public static class FileHelper
    {
        public static bool IsFileLocked(string filename)
        {
            return IsFileLocked(new FileInfo(filename));
        }

        public static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException ex)
            {
                return ex.IsLocked();
            }
            finally
            {
                stream?.Dispose();
            }

            //file is not locked
            return false;
        }

        public static bool TryExec(Action action, TimeSpan timeout)
        {
            var timer = DateTime.UtcNow.Add(timeout);

            do
            {
                try
                {
                    action();
                    return true;
                }
                catch (IOException ex)
                {
                    ex.WaitIfLocked(25);
                }
            }
            while (DateTime.UtcNow < timer);

            return false;
        }

        public static string Get(string path, int bufferSize = 128)
        {
            string contents = string.Empty;

            using (var fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite, bufferSize))
            using (var sr = new StreamReader(fs, Encoding.UTF8, false, bufferSize))
            {
                contents = sr.ReadToEnd();
            }

            return contents;
        }
    }
}
