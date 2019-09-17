using System;
using System.Collections.Generic;
using System.Text;

namespace Semverify
{
    internal static class ListExtensions
    {
        public static bool AddIf(this IList<string> list, bool condition, string value)
        {
            if (condition)
            {
                list.Add(value);
            }

            return condition;
        }

        public static bool AddFirstIf(this IList<string> list, IList<(bool condition, string value)> conditions)
        {
            foreach (var (condition, value) in conditions)
            {
                if (condition)
                {
                    list.Add(value);
                    return condition;
                }
            }
            return false;
        }
    }
}
