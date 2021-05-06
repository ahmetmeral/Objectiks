using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Objectiks.Helper
{
    public static class EnumHelper
    {
        public static IEnumerable<T> GetEnumValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }
}
