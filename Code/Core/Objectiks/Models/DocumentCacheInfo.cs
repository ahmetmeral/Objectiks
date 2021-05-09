using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Models
{
    public class DocumentCacheInfo
    {
        public int Expire { get; set; }
        public bool Lazy { get; set; }

        public DocumentCacheInfo() { }


        public DocumentCacheInfo(int expire, bool lazy = false)
        {
            Expire = expire;
            Lazy = lazy;
        }
    }
}
