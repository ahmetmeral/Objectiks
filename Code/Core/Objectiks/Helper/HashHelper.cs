﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Helper
{
    public static class HashHelper
    {
        public static string CreateMD5(string input)
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                var encoding = Encoding.ASCII;
                var data = encoding.GetBytes(input);

                Span<byte> hashBytes = stackalloc byte[16];
                md5.TryComputeHash(data, hashBytes, out int written);
                if (written != hashBytes.Length)
                    throw new OverflowException();


                Span<char> stringBuffer = stackalloc char[32];
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    hashBytes[i].TryFormat(stringBuffer.Slice(2 * i), out _, "x2");
                }
                return new string(stringBuffer);
            }
        }



        public static int GetHashCode<T1, T2>(T1 arg1, T2 arg2)
        {
            unchecked
            {
                return 31 * arg1.GetHashCode() + arg2.GetHashCode();
            }
        }

        public static int GetHashCode<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3)
        {
            unchecked
            {
                int hash = arg1.GetHashCode();
                hash = 31 * hash + arg2.GetHashCode();
                return 31 * hash + arg3.GetHashCode();
            }
        }

        public static int GetHashCode<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3,
            T4 arg4)
        {
            unchecked
            {
                int hash = arg1.GetHashCode();
                hash = 31 * hash + arg2.GetHashCode();
                hash = 31 * hash + arg3.GetHashCode();
                return 31 * hash + arg4.GetHashCode();
            }
        }

        public static int GetHashCode<T>(T[] list)
        {
            unchecked
            {
                int hash = 0;
                foreach (var item in list)
                {
                    hash = 31 * hash + item.GetHashCode();
                }
                return hash;
            }
        }

        public static int GetHashCode<T>(IEnumerable<T> list)
        {
            unchecked
            {
                int hash = 0;
                foreach (var item in list)
                {
                    hash = 31 * hash + item.GetHashCode();
                }
                return hash;
            }
        }

        /// <summary>
        /// Gets a hashcode for a collection for that the order of items 
        /// does not matter.
        /// So {1, 2, 3} and {3, 2, 1} will get same hash code.
        /// </summary>
        public static int GetHashCodeForOrderNoMatterCollection<T>(
            IEnumerable<T> list)
        {
            unchecked
            {
                int hash = 0;
                int count = 0;
                foreach (var item in list)
                {
                    hash += item.GetHashCode();
                    count++;
                }
                return 31 * hash + count.GetHashCode();
            }
        }

        /// <summary>
        /// Alternative way to get a hashcode is to use a fluent 
        /// interface like this:<br />
        /// return 0.CombineHashCode(field1).CombineHashCode(field2).
        ///     CombineHashCode(field3);
        /// </summary>
        public static int CombineHashCode<T>(this int hashCode, T arg)
        {
            unchecked
            {
                return 31 * hashCode + arg.GetHashCode();
            }
        }
    }
}
