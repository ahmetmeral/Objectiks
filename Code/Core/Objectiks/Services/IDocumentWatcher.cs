using Objectiks.Services;
using System.IO;

namespace Objectiks.Services
{
    public interface IDocumentWatcher
    {
        void Lock();
        void UnLock();
        void WaitForChanged(DocumentEngine engine);
        void OnChangeDocument(FileSystemEventArgs e);
    }
}