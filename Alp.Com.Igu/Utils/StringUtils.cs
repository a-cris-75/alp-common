using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alp.Com.Igu.Utils
{
    public static class StringUtils
    {
        public static string JoinFilter(string separator, IEnumerable<string> strings)
        {
            return string.Join(separator, strings.Where(s => !string.IsNullOrEmpty(s)));
        }
        public static string JoinFilter(string separator, params string[] str)
        {
            return string.Join(separator, str?.Where(s => !string.IsNullOrEmpty(s)));
        }
    }
}
