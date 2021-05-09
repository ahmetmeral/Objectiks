using Objectiks;
using Objectiks.Engine;
using Objectiks.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectik.Test.Web.Providers
{
    public class FileProviderOption : DocumentOption
    {
        public FileProviderOption() : base()
        {
            RegisterDefaults();
            UseDocumentWatcher<DocumentWatcher>();

            throw new Exception("RegisterTypeOf define");
        }
    }
}
