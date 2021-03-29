using Objectiks.Json.Parser;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Json
{
    public static class JsonExtentions
    {
        public static void AddDefaultParsers(this DocumentOptions options)
        {
            options.AddParserTypeOf<DocumentDefaultParser>();
            options.AddParserTypeOf<DocumentOneToOneParser>();
            options.AddParserTypeOf<DocumentManyToManyParser>();
            options.AddParserTypeOf<DocumentOneToManyParser>();
            options.AddParserTypeOf<DocumentOneToOneFileParser>();
        }
    }
}
