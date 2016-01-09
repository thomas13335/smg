using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common
{
    static class Extensions
    {
        public static string ToSeparatorList(this IEnumerable<object> strings, string separator = ", ")
        {
            var sb = new StringBuilder();
            foreach(var s in strings)
            {
                if (sb.Length > 0) sb.Append(separator);
                sb.Append(s);
            }

            return sb.ToString();
        }

        public static string ToSeparatorList(this IEnumerable<int> numbers)
        {
            var sb = new StringBuilder();
            foreach (var s in numbers)
            {
                if (sb.Length > 0) sb.Append(", ");
                sb.Append(s);
            }

            return sb.ToString();
        }
    }
}
