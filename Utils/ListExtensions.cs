using System.Reflection;

namespace Utils;

public static class ListExtensions
{
    public static string ToCvs<T>(this List<T> me, string separator = ", ")
    {
        PropertyInfo[] properties = typeof(T).GetProperties();
        string header = string.Empty;
        foreach (PropertyInfo property in properties)
        {
            string name = property.Name;
            header += $"{name}{separator}";
        }

        string ret =  header;

        foreach (T rec in me)
        {
            ret += Environment.NewLine + ((rec == null) ? "<null>" : rec.ToCvsLine(separator));
        }

        return ret;
    }
}
