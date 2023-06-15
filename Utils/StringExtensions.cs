using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils;
public static class StringExtensions
{
    public static bool IsNull(this string me)
    {
        return me is null;
    }

    public static bool IsNullOrEmpty(this string me)
    {
        return string.IsNullOrEmpty(me);
    }

    public static bool IsNullOrWhiteSpace(this string me)
    {
        return string.IsNullOrWhiteSpace(me);
    }
}
