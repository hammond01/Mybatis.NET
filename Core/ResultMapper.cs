using System.Data;
using System.Reflection;

namespace MyBatis.NET.Core;

public static class ResultMapper
{
    public static List<T> MapToList<T>(IDataReader reader) where T : new()
    {
        var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var result = new List<T>();

        while (reader.Read())
        {
            var obj = new T();
            foreach (var prop in props)
            {
                if (!HasColumn(reader, prop.Name)) continue;
                var val = reader[prop.Name];
                if (val is not DBNull) prop.SetValue(obj, val);
            }
            result.Add(obj);
        }
        return result;
    }

    private static bool HasColumn(IDataReader reader, string col)
    {
        for (int i = 0; i < reader.FieldCount; i++)
            if (reader.GetName(i).Equals(col, StringComparison.OrdinalIgnoreCase))
                return true;
        return false;
    }
}
