using System;
using System.Collections.Generic;
using System.Text;
using Objectiks.Services;

namespace Objectiks.PostgreSql
{
    public static class PostgreRepository
    {
        public static IDocumentReader<T> Where<T>(this IDocumentReader<T> reader)
        {
            return reader;
        }
    }
}
