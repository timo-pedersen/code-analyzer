using System.Reflection;

namespace Utils;

public static class ListExtensions
{
    public static string ToCvs<T>(this List<T> me, string separator = ", ")
    {
        PropertyInfo[] properties = typeof(T).GetProperties();
        string ret = string.Empty;
        string header = string.Empty;
        foreach (PropertyInfo property in properties)
        {
            string name = property.Name;
            header += $"{(header.Length > 0 ? separator : "")}{name}";
        }

        ret =  header;

        foreach (T rec in me)
        {
            ret += Environment.NewLine + rec.ToCvsLine(separator);
        }

        return ret;
    }

}
