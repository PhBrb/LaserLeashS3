using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartTest2
{
    public static class Extensions
    {
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
