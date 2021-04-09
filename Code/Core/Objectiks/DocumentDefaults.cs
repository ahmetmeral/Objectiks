using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks
{
    public class DocumentDefaults
    {
        public const string Root = "Objectiks";
        public const string Manifest = "Objectiks.json";
        public const string Meta = "Meta";
        public const string Schemes = "Schemes";
        public const string Documents = "Documents";
        public const string Contents = "Contents";
        public const string Partitions = "Partitions";

        public const int CacheExpire = 100000;
        public const int BufferSize = 128;
        public const string ProviderOf = "Default";
        public const string Extention = "*.json";

        //kaldıracağız..buna gerek kalmadı Ref içerisinde file referans verebiliyoruz...
        //yani statik olarak yazması gerekiyor..
        public const string ContentInfoSeparator = "----";
        public const string ContentRegexKeyForId = @"(?<=\[Id:)(.*)(?=\])";
        public const string ContentRegexKeyForLanguage = @"(?<=\[Language:)(.*)(?=\])";

        public const string DocumentPrimaryOf = "Id";
        public const string DocumentTypeOf = "Type";
        public const string DocumentParseOf = "Document";

        public const string DocumentMetaKeyOfProperty = "KeyOf";
        public const string DocumentMetaPrimaryOfProperty = "PrimaryOf";

        public static class Errors
        {

        }
    }
}
