using Objectiks.Services;
using System.IO;

namespace Objectiks.Services
{
    public interface IDocumentWatcher
    {
        void On();
        void Off();
        void WaitForChanged(IDocumentEngine engine);
        void OnChangeDocument(FileSystemEventArgs e);
    }
}