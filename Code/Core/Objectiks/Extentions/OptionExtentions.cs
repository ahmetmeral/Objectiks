using Objectiks.Parsers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Extentions
{
    public static class OptionExtentions
    {
        public static void AddDefaultParsers(this ObjectiksOptions options)
        {
            options.AddParserOf<DocumentDefaultParser>();
            options.AddParserOf<DocumentOneToOneParser>();
            options.AddParserOf<DocumentManyToManyParser>();
            options.AddParserOf<DocumentOneToManyParser>();
            options.AddParserOf<DocumentOneToOneFileParser>();
        }
    }
}
