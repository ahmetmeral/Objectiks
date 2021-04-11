using Objectiks.Parsers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Extentions
{
    public static class OptionExtentions
    {
        public static void AddDefaultParsers(this DocumentOption options)
        {
            options.AddParserTypeOf<DocumentDefaultParser>();
            options.AddParserTypeOf<DocumentOneToOneParser>();
            options.AddParserTypeOf<DocumentManyToManyParser>();
            options.AddParserTypeOf<DocumentOneToManyParser>();
            options.AddParserTypeOf<DocumentOneToOneFileParser>();
        }
    }
}
