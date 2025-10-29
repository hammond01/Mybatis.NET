using System.Data;
using Microsoft.Data.SqlClient;
using MyBatis.NET.Mapper;

namespace MyBatis.NET.Core;

public class MyBatisException : Exception
{
    public MyBatisException(string message) : base(message) { }
    public MyBatisException(string message, Exception inner) : base(message, inner) { }
}

public class SqlSession : IDisposable
{
    private readonly IDbConnection _conn;
    private IDbTransaction? _tran;
    private readonly int _commandTimeout;

    public SqlSession(string connectionString, int commandTimeout = 30)
    {
        _commandTimeout = commandTimeout;
        _conn = new SqlConnection(connectionString);
        _conn.Open();
    }

    public void BeginTransaction() => _tran = _conn.BeginTransaction();
    public void Commit() => _tran?.Commit();
    public void Rollback() => _tran?.Rollback();

    public List<T> SelectList<T>(string id, Dictionary<string, object>? parameters = null)
        where T : new()
    {
        try
        {
            var stmt = MapperRegistry.Get(id);
            using var cmd = new SqlCommand(stmt.Sql, (SqlConnection)_conn, (SqlTransaction?)_tran);
            cmd.CommandTimeout = _commandTimeout;
            AddParams(cmd, parameters);
            using var reader = cmd.ExecuteReader();
            return ResultMapper.MapToList<T>(reader);
        }
        catch (Exception ex)
        {
            throw new MyBatisException($"Error executing select statement '{id}': {ex.Message}", ex);
        }
    }

    public T? SelectOne<T>(string id, Dictionary<string, object>? parameters = null)
        where T : new()
        => SelectList<T>(id, parameters).FirstOrDefault();

    public int Execute(string id, Dictionary<string, object>? parameters = null)
    {
        try
        {
            var stmt = MapperRegistry.Get(id);
            using var cmd = new SqlCommand(stmt.Sql, (SqlConnection)_conn, (SqlTransaction?)_tran);
            cmd.CommandTimeout = _commandTimeout;
            AddParams(cmd, parameters);
            return cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            throw new MyBatisException($"Error executing statement '{id}': {ex.Message}", ex);
        }
    }

    private static void AddParams(SqlCommand cmd, Dictionary<string, object>? parameters)
    {
        if (parameters == null) return;
        foreach (var p in parameters)
        {
            if (p.Value is not null && p.Value.GetType().IsClass && p.Value.GetType() != typeof(string))
            {
                // If parameter is an object, extract its properties
                var props = p.Value.GetType().GetProperties();
                foreach (var prop in props)
                {
                    var value = prop.GetValue(p.Value) ?? DBNull.Value;
                    cmd.Parameters.AddWithValue($"@{prop.Name}", value);
                }
            }
            else
            {
                cmd.Parameters.AddWithValue($"@{p.Key}", p.Value ?? DBNull.Value);
            }
        }
    }

    public async Task<List<T>> SelectListAsync<T>(string id, Dictionary<string, object>? parameters = null)
        where T : new()
    {
        try
        {
            var stmt = MapperRegistry.Get(id);
            await using var cmd = new SqlCommand(stmt.Sql, (SqlConnection)_conn, (SqlTransaction?)_tran);
            cmd.CommandTimeout = _commandTimeout;
            AddParams(cmd, parameters);
            await using var reader = await cmd.ExecuteReaderAsync();
            return ResultMapper.MapToList<T>(reader);
        }
        catch (Exception ex)
        {
            throw new MyBatisException($"Error executing async select statement '{id}': {ex.Message}", ex);
        }
    }

    public async Task<T?> SelectOneAsync<T>(string id, Dictionary<string, object>? parameters = null)
        where T : new()
        => (await SelectListAsync<T>(id, parameters)).FirstOrDefault();

    public async Task<int> ExecuteAsync(string id, Dictionary<string, object>? parameters = null)
    {
        try
        {
            var stmt = MapperRegistry.Get(id);
            await using var cmd = new SqlCommand(stmt.Sql, (SqlConnection)_conn, (SqlTransaction?)_tran);
            cmd.CommandTimeout = _commandTimeout;
            AddParams(cmd, parameters);
            return await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            throw new MyBatisException($"Error executing async statement '{id}': {ex.Message}", ex);
        }
    }

    public T GetMapper<T>() where T : class
    {
        return MapperProxy<T>.Create(this);
    }

    public void Dispose()
    {
        _tran?.Dispose();
        _conn?.Dispose();
    }

}
