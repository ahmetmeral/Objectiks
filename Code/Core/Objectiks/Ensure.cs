using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks
{
    public static class Ensure
    {
        public static void Try(bool condition, string message)
        {
            if (condition)
                throw new Exception(message);
        }

        public static void NotNull(object obj, string message)
        {
            Try(obj == null, message);
        }

        public static void NotNullOrEmpty(string str, string message)
        {
            Try(String.IsNullOrEmpty(str), message);
        }
    }
}
