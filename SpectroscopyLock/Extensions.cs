﻿using System.Collections.Generic;

namespace LaserLeash
{
    public static class Extensions
    {
        /// <summary>
        /// Formats elements into [a, b, ...]
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string ToBracketString(this IList<double> list)
        {
            string ret = "[";
            for (int i = 0; i < list.Count; i++)
            {
                ret += list[i];
                if (i < list.Count - 1) ret += ", ";
            }
            ret += "]";
            return ret;
        }
    }
}
