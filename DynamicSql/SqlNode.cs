using System.Text;

namespace MyBatis.NET.DynamicSql;

/// <summary>
/// Base class for all SQL nodes in the dynamic SQL tree
/// </summary>
public abstract class SqlNode
{
    /// <summary>
    /// Applies this node to build SQL string
    /// </summary>
    /// <param name="context">The context containing parameters and SQL builder</param>
    /// <returns>True if this node added content to SQL</returns>
    public abstract bool Apply(DynamicContext context);
}

/// <summary>
/// Context for building dynamic SQL
/// </summary>
public class DynamicContext
{
    private readonly StringBuilder _sqlBuilder = new();
    private readonly Dictionary<string, object?> _parameters;

    public DynamicContext(Dictionary<string, object?> parameters)
    {
        _parameters = parameters ?? new Dictionary<string, object?>();
    }

    public void AppendSql(string sql)
    {
        _sqlBuilder.Append(sql);
    }

    public string GetSql() => _sqlBuilder.ToString().Trim();

    public object? GetParameter(string name)
    {
        // Support nested property access like "user.name"
        if (name.Contains('.'))
        {
            var parts = name.Split('.');
            object? current = null;

            if (_parameters.TryGetValue(parts[0], out current))
            {
                for (int i = 1; i < parts.Length && current != null; i++)
                {
                    var prop = current.GetType().GetProperty(parts[i]);
                    current = prop?.GetValue(current);
                }
            }
            return current;
        }

        return _parameters.TryGetValue(name, out var value) ? value : null;
    }

    public bool HasParameter(string name) => _parameters.ContainsKey(name);

    public Dictionary<string, object?> GetParameters() => _parameters;

    public void SetParameter(string name, object? value) => _parameters[name] = value;
}

/// <summary>
/// Simple text node (static SQL)
/// </summary>
public class TextSqlNode : SqlNode
{
    private readonly string _text;

    public TextSqlNode(string text)
    {
        _text = text;
    }

    public override bool Apply(DynamicContext context)
    {
        context.AppendSql(_text);
        return true;
    }
}

/// <summary>
/// Mixed SQL node that can contain static and dynamic parts
/// </summary>
public class MixedSqlNode : SqlNode
{
    private readonly List<SqlNode> _contents;

    public MixedSqlNode(List<SqlNode> contents)
    {
        _contents = contents;
    }

    public override bool Apply(DynamicContext context)
    {
        bool applied = false;
        foreach (var node in _contents)
        {
            applied = node.Apply(context) || applied;
        }
        return applied;
    }
}
