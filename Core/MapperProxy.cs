using System.Reflection;

namespace MyBatis.NET.Core;

public class MapperProxy<T> : DispatchProxy where T : class
{
    private SqlSession? _session;

    public static T Create(SqlSession session)
    {
        var proxyInstance = Create<T, MapperProxy<T>>();
        var proxy = (MapperProxy<T>)(object)proxyInstance!;
        proxy._session = session;
        return proxyInstance;
    }

    protected override object? Invoke(MethodInfo? method, object?[]? args)
    {
        if (method == null)
            throw new ArgumentNullException(nameof(method));

        if (_session == null)
            throw new InvalidOperationException("Session not initialized.");

        var mapperId = $"{typeof(T).Name}.{method.Name}";
        var returnType = method.ReturnType;

        // Chuẩn bị parameters
        var paramNames = method.GetParameters().Select(p => p.Name!).ToArray();
        var dict = new Dictionary<string, object>();
        if (args != null)
            for (int i = 0; i < args.Length; i++)
                dict[paramNames[i]] = args[i] ?? DBNull.Value;

        // Tự động chọn Select/Execute
        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(List<>))
        {
            var itemType = returnType.GetGenericArguments()[0];
            var call = typeof(SqlSession)
                .GetMethod(nameof(SqlSession.SelectList))!
                .MakeGenericMethod(itemType);
            return call.Invoke(_session, new object?[] { mapperId, dict });
        }
        else if (returnType == typeof(void))
        {
            _session.Execute(mapperId, dict);
            return null;
        }
        else if (returnType == typeof(int))
        {
            return _session.Execute(mapperId, dict);
        }
        else
        {
            var call = typeof(SqlSession)
                .GetMethod(nameof(SqlSession.SelectOne))!
                .MakeGenericMethod(returnType);
            return call.Invoke(_session, new object?[] { mapperId, dict });
        }
    }

}
