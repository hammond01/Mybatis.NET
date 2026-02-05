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
            var convertedParams = ConvertParameters(parameters);
            var sql = stmt.BuildSql(convertedParams, out var modifiedParams);
            SqlSessionConfiguration.LogSql(sql, modifiedParams);
            using var cmd = new SqlCommand(sql, (SqlConnection)_conn, (SqlTransaction?)_tran);
            cmd.CommandTimeout = _commandTimeout;
            // Use modifiedParams instead of original parameters for ForEach expansion
            AddParams(cmd, modifiedParams);
            using var reader = cmd.ExecuteReader();
            return ResultMapper.MapToList<T>(reader);
        }
        catch (SqlException ex) when (ex.Message.Contains("Must declare the scalar variable"))
        {
             throw new MyBatisException($"Error executing '{id}': Missing parameter. {ex.Message}", ex);
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
            var convertedParams = ConvertParameters(parameters);
            var sql = stmt.BuildSql(convertedParams, out var modifiedParams);
            SqlSessionConfiguration.LogSql(sql, modifiedParams);
            using var cmd = new SqlCommand(sql, (SqlConnection)_conn, (SqlTransaction?)_tran);
            cmd.CommandTimeout = _commandTimeout;
            AddParams(cmd, modifiedParams);
            return cmd.ExecuteNonQuery();
        }
        catch (SqlException ex) when (ex.Message.Contains("Must declare the scalar variable"))
        {
             throw new MyBatisException($"Error executing '{id}': Missing parameter. {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new MyBatisException($"Error executing statement '{id}': {ex.Message}", ex);
        }
    }

    private static Dictionary<string, object?> ConvertParameters(Dictionary<string, object>? parameters)
    {
        if (parameters == null)
            return new Dictionary<string, object?>();

        var result = new Dictionary<string, object?>();

        foreach (var kvp in parameters)
        {
            // If value is a complex object (not primitive, string, or collection), extract its properties
            if (kvp.Value != null &&
                kvp.Value.GetType().IsClass &&
                kvp.Value.GetType() != typeof(string) &&
                !(kvp.Value is System.Collections.IEnumerable))
            {
                // Extract all properties from the object
                var props = kvp.Value.GetType().GetProperties();
                foreach (var prop in props)
                {
                    result[prop.Name] = prop.GetValue(kvp.Value);
                }
            }
            else
            {
                result[kvp.Key] = kvp.Value;
            }
        }

        return result;
    }

    private static void AddParams(SqlCommand cmd, Dictionary<string, object?>? parameters)
    {
        if (parameters == null) return;

        var sql = cmd.CommandText;

        foreach (var p in parameters)
        {
            if (p.Value is not null && p.Value.GetType().IsClass && p.Value.GetType() != typeof(string)
                && !(p.Value is System.Collections.IEnumerable))
            {
                // If parameter is an object, extract its properties
                var props = p.Value.GetType().GetProperties();
                foreach (var prop in props)
                {
                    var paramName = $"@{prop.Name}";
                    // Only add parameter if it's referenced in the SQL
                    if (sql.Contains(paramName, StringComparison.OrdinalIgnoreCase))
                    {
                        var value = prop.GetValue(p.Value) ?? DBNull.Value;
                        cmd.Parameters.AddWithValue(paramName, value);
                    }
                }
            }
            else
            {
                var paramName = $"@{p.Key}";
                // Only add parameter if it's referenced in the SQL
                if (sql.Contains(paramName, StringComparison.OrdinalIgnoreCase))
                {
                    cmd.Parameters.AddWithValue(paramName, p.Value ?? DBNull.Value);
                }
            }
        }
    }

    public async Task<List<T>> SelectListAsync<T>(string id, Dictionary<string, object>? parameters = null)
        where T : new()
    {
        try
        {
            var stmt = MapperRegistry.Get(id);
            var convertedParams = ConvertParameters(parameters);
            var sql = stmt.BuildSql(convertedParams, out var modifiedParams);
            SqlSessionConfiguration.LogSql(sql, modifiedParams);
            await using var cmd = new SqlCommand(sql, (SqlConnection)_conn, (SqlTransaction?)_tran);
            cmd.CommandTimeout = _commandTimeout;
            AddParams(cmd, modifiedParams);
            await using var reader = await cmd.ExecuteReaderAsync();
            return ResultMapper.MapToList<T>(reader);
        }
        catch (SqlException ex) when (ex.Message.Contains("Must declare the scalar variable"))
        {
             throw new MyBatisException($"Error executing async select '{id}': Missing parameter. {ex.Message}", ex);
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
            var convertedParams = ConvertParameters(parameters);
            var sql = stmt.BuildSql(convertedParams, out var modifiedParams);
            SqlSessionConfiguration.LogSql(sql, modifiedParams);
            await using var cmd = new SqlCommand(sql, (SqlConnection)_conn, (SqlTransaction?)_tran);
            cmd.CommandTimeout = _commandTimeout;
            AddParams(cmd, modifiedParams);
            return await cmd.ExecuteNonQueryAsync();
        }
        catch (SqlException ex) when (ex.Message.Contains("Must declare the scalar variable"))
        {
             throw new MyBatisException($"Error executing async statement '{id}': Missing parameter. {ex.Message}", ex);
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
