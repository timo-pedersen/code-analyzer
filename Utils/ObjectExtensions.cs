using System.Reflection;

namespace Utils
{
    public static class ObjectExtensions
    {
        public static string ToCvsLine(this object me, string separator = ", ")
        {
            PropertyInfo[] properties = me.GetType().GetProperties();
            string ret = string.Empty;
            foreach (PropertyInfo property in properties)
            {
                string? val = property.GetValue(me)?.ToString();
                //ret += $"{(ret.Length > 0 ? separator : "")}{val}";
                ret += $"{val}{separator}";
            }

            return ret;
        }

        public static string ToCvsHeader(this object me, string separator = ", ")
        {
            PropertyInfo[] properties = me.GetType().GetProperties();
            string ret = string.Empty;
            foreach (PropertyInfo property in properties)
            {
                string name = property.Name;
                //ret += $"{(ret.Length > 0 ? separator : "")}{name}";
                ret += $"{name}{separator}";
            }

            return ret;
        }

    }
}
