using Objectiks.Services;
using System.IO;

namespace Objectiks.Services
{
    public interface IDocumentWatcher
    {
        void WaitForChanged(IDocumentEngine engine);
        void OnChangeDocument(FileSystemEventArgs e);
    }
}