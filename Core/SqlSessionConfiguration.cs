namespace MyBatis.NET.Core;

/// <summary>
/// Configuration for SqlSession behavior
/// </summary>
public class SqlSessionConfiguration
{
    /// <summary>
    /// Enable SQL logging to console/debug output
    /// </summary>
    public static bool EnableSqlLogging { get; set; } = false;

    /// <summary>
    /// Enable parameter logging
    /// </summary>
    public static bool EnableParameterLogging { get; set; } = false;

    /// <summary>
    /// Log SQL statement to console
    /// </summary>
    internal static void LogSql(string sql, Dictionary<string, object?>? parameters = null)
    {
        if (!EnableSqlLogging) return;

        Console.WriteLine("═══════════════════════════════════════");
        Console.WriteLine($"[MyBatis.NET SQL] {DateTime.Now:HH:mm:ss.fff}");
        Console.WriteLine("───────────────────────────────────────");
        Console.WriteLine(sql);

        if (EnableParameterLogging && parameters != null && parameters.Count > 0)
        {
            Console.WriteLine("───────────────────────────────────────");
            Console.WriteLine("Parameters:");
            foreach (var param in parameters)
            {
                var value = param.Value?.ToString() ?? "NULL";
                if (param.Value is string)
                    value = $"'{value}'";
                Console.WriteLine($"  @{param.Key} = {value}");
            }
        }
        Console.WriteLine("═══════════════════════════════════════");
    }
}
