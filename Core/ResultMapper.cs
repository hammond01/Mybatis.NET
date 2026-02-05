using System.Data;
using System.Reflection;

namespace MyBatis.NET.Core;

public static class ResultMapper
{
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, Delegate> _cache = new();

    public static List<T> MapToList<T>(IDataReader reader) where T : new()
    {
        var list = new List<T>();
        if (reader == null) return list;

        // Get or compile a mapping function for this specific Reader schema + Type
        // Note: We key by Type only here for simplicity. 
        // Ideally, we should check caching based on column layout, but rebuilding the delegate 
        // once per query execution is fast enough compared to row processing.
        // For max performance, we build the mapper based on the *current* reader columns.
        
        var mapFunction = GetMapFunction<T>(reader);

        while (reader.Read())
        {
            list.Add(mapFunction(reader));
        }

        return list;
    }

    private static Func<IDataReader, T> GetMapFunction<T>(IDataReader reader)
    {
        // 1. Get Column Schema (Name -> Ordinal)
        var columns = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < reader.FieldCount; i++)
        {
            columns[reader.GetName(i)] = i;
        }

        // 2. Build Expression Tree: (IDataReader r) => new T { Prop1 = r[i], Prop2 = r[j] }
        var readerParam = System.Linq.Expressions.Expression.Parameter(typeof(IDataReader), "r");
        var memberBindings = new List<System.Linq.Expressions.MemberBinding>();
        
        var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite);

        foreach (var prop in props)
        {
            // Simple mapping: Property Name == Column Name
            // In future, this is where we check <resultMap> for custom mappings
            if (columns.TryGetValue(prop.Name, out int ordinal))
            {
                // Explicitly get connection methods from IDataRecord (interface inheritance can be tricky with Reflection)
                var getValueMethod = typeof(IDataRecord).GetMethod("GetValue", new[] { typeof(int) }) 
                                     ?? typeof(IDataReader).GetMethod("GetValue", new[] { typeof(int) });

                if (getValueMethod == null)
                    throw new InvalidOperationException("Could not find GetValue method on IDataReader");

                // r.GetValue(ordinal)
                var getValueCall = System.Linq.Expressions.Expression.Call(
                    readerParam, 
                    getValueMethod, 
                    System.Linq.Expressions.Expression.Constant(ordinal)
                );

                // ConvertValue method
                var convertMethod = typeof(ResultMapper).GetMethod(nameof(ConvertValue), BindingFlags.Public | BindingFlags.Static);
                
                if (convertMethod == null)
                     throw new InvalidOperationException("Could not find ConvertValue method on ResultMapper");

                // Convert(val, PropType)
                var valueVar = System.Linq.Expressions.Expression.Convert(
                    System.Linq.Expressions.Expression.Call(null, convertMethod.MakeGenericMethod(prop.PropertyType), getValueCall),
                    prop.PropertyType
                );

                memberBindings.Add(System.Linq.Expressions.Expression.Bind(prop, valueVar));
            }
        }

        var newExpression = System.Linq.Expressions.Expression.New(typeof(T));
        var initExpression = System.Linq.Expressions.Expression.MemberInit(newExpression, memberBindings);
        
        var lambda = System.Linq.Expressions.Expression.Lambda<Func<IDataReader, T>>(initExpression, readerParam);
        return lambda.Compile();
    }

    // Helper to handle DBNull and type conversion safely
    public static TProp ConvertValue<TProp>(object value)
    {
        if (value == null || value is DBNull)
            return default!;
            
        // Handle common type mismatches (e.g. double -> decimal, long -> int)
        var targetType = Nullable.GetUnderlyingType(typeof(TProp)) ?? typeof(TProp);
        
        return (TProp)Convert.ChangeType(value, targetType);
    }
}
