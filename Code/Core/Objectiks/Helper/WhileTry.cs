using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Helper
{
    public static class WhileTry
    {
        public static bool Try(Func<bool> func, int tryCount, bool throwEx, string message)
        {
            bool result = false;

            while (!result && tryCount > 0)
            {
                result = func();
                tryCount--;
            }

            if (!result)
            {
                throw new Exception(message);
            }

            return result;
        }
    }
}
