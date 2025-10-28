using Microsoft.Data.SqlClient;
using MyBatis.NET.Mapper;

namespace MyBatis.NET.Core;

public class SqlSession : IDisposable
{
    private readonly SqlConnection _conn;
    private SqlTransaction? _tran;

    public SqlSession(string connectionString)
    {
        _conn = new SqlConnection(connectionString);
        _conn.Open();
    }

    public void BeginTransaction() => _tran = _conn.BeginTransaction();
    public void Commit() => _tran?.Commit();
    public void Rollback() => _tran?.Rollback();

    public List<T> SelectList<T>(string id, Dictionary<string, object>? parameters = null)
        where T : new()
    {
        var stmt = MapperRegistry.Get(id);
        using var cmd = new SqlCommand(stmt.Sql, _conn, _tran);
        AddParams(cmd, parameters);
        using var reader = cmd.ExecuteReader();
        return ResultMapper.MapToList<T>(reader);
    }

    public T? SelectOne<T>(string id, Dictionary<string, object>? parameters = null)
        where T : new()
        => SelectList<T>(id, parameters).FirstOrDefault();

    public int Execute(string id, Dictionary<string, object>? parameters = null)
    {
        var stmt = MapperRegistry.Get(id);
        using var cmd = new SqlCommand(stmt.Sql, _conn, _tran);
        AddParams(cmd, parameters);
        return cmd.ExecuteNonQuery();
    }

    private static void AddParams(SqlCommand cmd, Dictionary<string, object>? parameters)
    {
        if (parameters == null) return;
        foreach (var p in parameters)
            cmd.Parameters.AddWithValue($"@{p.Key}", p.Value ?? DBNull.Value);
    }

    public void Dispose() => _conn.Close();

    public T GetMapper<T>() where T : class
    {
        return MapperProxy<T>.Create(this);
    }

}
