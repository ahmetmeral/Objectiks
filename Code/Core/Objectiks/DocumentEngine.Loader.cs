using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Objectiks.Engine;
using Objectiks.Engine.Query;
using Objectiks.Extentions;
using Objectiks.Models;
using Objectiks.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Linq.Dynamic.Core;
using Objectiks.Helper;

namespace Objectiks
{
    public abstract partial class DocumentEngine : IDocumentEngine
    {
        public virtual DocumentEngine Initialize()
        {
            Watcher?.Lock();

            foreach (var type in Option.TypeOf)
            {
                using (var trans = Monitor.GetTransaction(this, true, true))
                {
                    trans.EnterTypeOfLock(type.TypeOf);

                    CheckTypeOfSchema(type.TypeOf);
                    LoadDocumentType(type.TypeOf, true);

                    trans.ExitTypeOfLock(type.TypeOf);

                    Monitor.ReleaseTransaction(trans);
                }
            }

            Watcher?.UnLock();

            return this;
        }

        public abstract bool LoadDocumentType(string typeOf, bool isInitialize = false);
        public abstract void CheckTypeOfSchema(string typeOf);

    }
}
