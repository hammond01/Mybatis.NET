using MyBatis.NET.DynamicSql;

namespace MyBatis.NET.Mapper;

public class SqlStatement
{
    public string Id { get; set; } = "";
    public string Sql { get; set; } = "";
    public string ParameterType { get; set; } = "";
    public string ResultType { get; set; } = "";
    public string CommandType { get; set; } = "Text";

    /// <summary>
    /// Root node for dynamic SQL (if SQL contains dynamic tags)
    /// </summary>
    public SqlNode? RootNode { get; set; }

    /// <summary>
    /// Indicates whether this statement contains dynamic SQL
    /// </summary>
    public bool IsDynamic => RootNode != null;

    /// <summary>
    /// Builds the final SQL by applying dynamic SQL nodes
    /// </summary>
    public string BuildSql(Dictionary<string, object?>? parameters)
    {
        if (!IsDynamic)
            return Sql;

        var context = new DynamicContext(parameters ?? new Dictionary<string, object?>());
        RootNode!.Apply(context);
        return context.GetSql();
    }

    /// <summary>
    /// Builds the final SQL and returns modified parameters (for ForEach expansion)
    /// </summary>
    public string BuildSql(Dictionary<string, object?>? parameters, out Dictionary<string, object?> modifiedParameters)
    {
        if (!IsDynamic)
        {
            modifiedParameters = parameters ?? new Dictionary<string, object?>();
            return Sql;
        }

        var context = new DynamicContext(parameters ?? new Dictionary<string, object?>());
        RootNode!.Apply(context);
        modifiedParameters = context.GetParameters();
        return context.GetSql();
    }
}
