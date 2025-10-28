namespace MyBatis.NET.Mapper;

public static class MapperRegistry
{
    private static readonly Dictionary<string, SqlStatement> _map = new();

    public static void Register(SqlStatement stmt) => _map[stmt.Id] = stmt;

    public static void RegisterMany(IEnumerable<SqlStatement> stmts)
    {
        foreach (var s in stmts)
            _map[s.Id] = s;
    }

    public static SqlStatement Get(string id)
    {
        if (!_map.TryGetValue(id, out var stmt))
            throw new InvalidOperationException($"Mapper id '{id}' not found");
        return stmt;
    }
}
