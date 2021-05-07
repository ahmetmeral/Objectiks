using Objectiks.Services;
using System.IO;

namespace Objectiks.Services
{
    public interface IDocumentWatcher
    {
        void Lock();
        void UnLock();
        void WaitForChanged(IDocumentEngine engine);
        void OnChangeDocument(FileSystemEventArgs e);
    }
}