using System.Text;
using System.Text.RegularExpressions;

namespace MyBatis.NET.DynamicSql;

/// <summary>
/// &lt;if test="condition"&gt;SQL&lt;/if&gt;
/// </summary>
public class IfSqlNode : SqlNode
{
    private readonly string _test;
    private readonly SqlNode _contents;

    public IfSqlNode(SqlNode contents, string test)
    {
        _contents = contents;
        _test = test;
    }

    public override bool Apply(DynamicContext context)
    {
        if (ExpressionEvaluator.Evaluate(_test, context))
        {
            return _contents.Apply(context);
        }
        return false;
    }
}

/// <summary>
/// &lt;where&gt;...&lt;/where&gt; - Automatically adds WHERE and removes leading AND/OR
/// </summary>
public class WhereSqlNode : SqlNode
{
    private readonly SqlNode _contents;

    public WhereSqlNode(SqlNode contents)
    {
        _contents = contents;
    }

    public override bool Apply(DynamicContext context)
    {
        var prefixContext = new DynamicContext(context.GetParameters());
        bool applied = _contents.Apply(prefixContext);

        if (applied)
        {
            var sql = prefixContext.GetSql();
            sql = RemovePrefixes(sql, "AND ", "OR ");

            if (!string.IsNullOrWhiteSpace(sql))
            {
                context.AppendSql(" WHERE ");
                context.AppendSql(sql);
                return true;
            }
        }
        return false;
    }

    private static string RemovePrefixes(string sql, params string[] prefixes)
    {
        sql = sql.Trim();
        foreach (var prefix in prefixes)
        {
            if (sql.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return sql.Substring(prefix.Length).Trim();
            }
        }
        return sql;
    }
}

/// <summary>
/// &lt;set&gt;...&lt;/set&gt; - Automatically adds SET and removes trailing comma
/// </summary>
public class SetSqlNode : SqlNode
{
    private readonly SqlNode _contents;

    public SetSqlNode(SqlNode contents)
    {
        _contents = contents;
    }

    public override bool Apply(DynamicContext context)
    {
        var prefixContext = new DynamicContext(context.GetParameters());
        bool applied = _contents.Apply(prefixContext);

        if (applied)
        {
            var sql = prefixContext.GetSql().Trim();
            sql = sql.TrimEnd(',', ' ');

            if (!string.IsNullOrWhiteSpace(sql))
            {
                context.AppendSql(" SET ");
                context.AppendSql(sql);
                return true;
            }
        }
        return false;
    }
}

/// <summary>
/// &lt;choose&gt;&lt;when test=""&gt;&lt;/when&gt;&lt;otherwise&gt;&lt;/otherwise&gt;&lt;/choose&gt;
/// </summary>
public class ChooseSqlNode : SqlNode
{
    private readonly List<IfSqlNode> _whenNodes;
    private readonly SqlNode? _otherwiseNode;

    public ChooseSqlNode(List<IfSqlNode> whenNodes, SqlNode? otherwiseNode)
    {
        _whenNodes = whenNodes;
        _otherwiseNode = otherwiseNode;
    }

    public override bool Apply(DynamicContext context)
    {
        foreach (var when in _whenNodes)
        {
            if (when.Apply(context))
            {
                return true;
            }
        }

        if (_otherwiseNode != null)
        {
            return _otherwiseNode.Apply(context);
        }

        return false;
    }
}

/// <summary>
/// &lt;foreach collection="list" item="item" separator="," open="(" close=")"&gt;
/// </summary>
public class ForEachSqlNode : SqlNode
{
    private readonly SqlNode _contents;
    private readonly string _collection;
    private readonly string _item;
    private readonly string _index;
    private readonly string _separator;
    private readonly string _open;
    private readonly string _close;

    public ForEachSqlNode(SqlNode contents, string collection, string item, string index,
        string separator, string open, string close)
    {
        _contents = contents;
        _collection = collection;
        _item = item ?? "item";
        _index = index ?? "index";
        _separator = separator ?? "";
        _open = open ?? "";
        _close = close ?? "";
    }

    public override bool Apply(DynamicContext context)
    {
        var collectionValue = context.GetParameter(_collection);

        if (collectionValue == null)
            return false;

        var items = GetCollectionItems(collectionValue);
        if (items.Count == 0)
            return false;

        context.AppendSql(_open);

        for (int i = 0; i < items.Count; i++)
        {
            var iterContext = new DynamicContext(context.GetParameters())
            {
            };

            // Add item and index to context
            var itemParams = new Dictionary<string, object?>(context.GetParameters())
            {
                [_item] = items[i],
                [_index] = i
            };
            var itemContext = new DynamicContext(itemParams);

            _contents.Apply(itemContext);
            var itemSql = itemContext.GetSql();

            // Replace #{item} with actual parameter AND add to main context
            itemSql = Regex.Replace(itemSql, @"@" + _item + @"\b", $"@{_item}_{i}");
            context.SetParameter($"{_item}_{i}", items[i]);
            context.AppendSql(itemSql);

            if (i < items.Count - 1 && !string.IsNullOrEmpty(_separator))
            {
                context.AppendSql(_separator);
            }
        }

        context.AppendSql(_close);
        return true;
    }

    private static List<object?> GetCollectionItems(object collection)
    {
        var result = new List<object?>();

        if (collection is System.Collections.IEnumerable enumerable)
        {
            foreach (var item in enumerable)
            {
                result.Add(item);
            }
        }
        else
        {
            result.Add(collection);
        }

        return result;
    }
}

/// <summary>
/// &lt;trim prefix="WHERE" prefixOverrides="AND |OR " suffix="" suffixOverrides=","&gt;
/// </summary>
public class TrimSqlNode : SqlNode
{
    private readonly SqlNode _contents;
    private readonly string _prefix;
    private readonly string _suffix;
    private readonly string[] _prefixOverrides;
    private readonly string[] _suffixOverrides;

    public TrimSqlNode(SqlNode contents, string prefix, string suffix,
        string prefixOverrides, string suffixOverrides)
    {
        _contents = contents;
        _prefix = prefix ?? "";
        _suffix = suffix ?? "";
        _prefixOverrides = ParseOverrides(prefixOverrides);
        _suffixOverrides = ParseOverrides(suffixOverrides);
    }

    private static string[] ParseOverrides(string overrides)
    {
        if (string.IsNullOrEmpty(overrides))
            return Array.Empty<string>();

        return overrides.Split('|')
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .ToArray();
    }

    public override bool Apply(DynamicContext context)
    {
        var trimContext = new DynamicContext(context.GetParameters());
        bool applied = _contents.Apply(trimContext);

        if (applied)
        {
            var sql = trimContext.GetSql().Trim();

            // Remove prefix overrides
            foreach (var prefix in _prefixOverrides)
            {
                if (sql.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    sql = sql.Substring(prefix.Length).Trim();
                    break;
                }
            }

            // Remove suffix overrides
            foreach (var suffix in _suffixOverrides)
            {
                if (sql.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    sql = sql.Substring(0, sql.Length - suffix.Length).Trim();
                    break;
                }
            }

            if (!string.IsNullOrWhiteSpace(sql))
            {
                if (!string.IsNullOrEmpty(_prefix))
                    context.AppendSql(_prefix + " ");
                context.AppendSql(sql);
                if (!string.IsNullOrEmpty(_suffix))
                    context.AppendSql(" " + _suffix);
                return true;
            }
        }

        return false;
    }
}
