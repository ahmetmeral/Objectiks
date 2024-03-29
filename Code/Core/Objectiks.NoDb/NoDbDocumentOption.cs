﻿using Objectiks.Caching;
using Objectiks.Caching.Serializer;
using Objectiks.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.NoDb
{
    public class NoDbDocumentOption : DocumentOption
    {
        public NoDbDocumentOption() : base()
        {
            this.Name = "NoDbDocuments";
            this.SupportPartialStorage = true;
            this.SupportPartialStorageSize = 1000;
            this.UseEngineProvider<NoDbEngine>();
            this.RegisterDefaults();
        }
    }
}
