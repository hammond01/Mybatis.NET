# SQL Logging Configuration

## 📝 Overview

MyBatis.NET provides built-in SQL logging functionality to help you debug and monitor database queries. You can enable/disable SQL logging and parameter logging independently.

**Package Name**: `MyBatis.NET.SqlMapper`  
**Namespaces**: `MyBatis.NET.Core`, `MyBatis.NET.Mapper`

## 📦 Installation

```bash
dotnet add package MyBatis.NET.SqlMapper
```

## 🚀 Quick Start

### Enable SQL Logging

```csharp
using MyBatis.NET.Core;

// Enable SQL logging only
SqlSessionConfiguration.EnableSqlLogging = true;

// Enable SQL + Parameter logging
SqlSessionConfiguration.EnableSqlLogging = true;
SqlSessionConfiguration.EnableParameterLogging = true;

// Use SqlSession as normal - SQL will be logged automatically
using var session = new SqlSession(connectionString);
var users = session.SelectList<User>("IUserMapper.GetAll", null);
```

## ⚙️ Configuration Options

### `SqlSessionConfiguration.EnableSqlLogging`

- **Type**: `bool`
- **Default**: `false`
- **Description**: Enable/disable SQL statement logging to console

```csharp
SqlSessionConfiguration.EnableSqlLogging = true;
```

### `SqlSessionConfiguration.EnableParameterLogging`

- **Type**: `bool`
- **Default**: `false`
- **Description**: Enable/disable parameter value logging (requires `EnableSqlLogging = true`)

```csharp
SqlSessionConfiguration.EnableParameterLogging = true;
```

## 📊 Output Examples

### Example 1: SQL Only

```csharp
SqlSessionConfiguration.EnableSqlLogging = true;
SqlSessionConfiguration.EnableParameterLogging = false;

var user = session.SelectOne<User>("IUserMapper.GetById",
    new Dictionary<string, object> { ["id"] = 1 });
```

**Output:**

```
═══════════════════════════════════════
[MyBatis.NET SQL] 14:52:07.910
───────────────────────────────────────
SELECT * FROM Users WHERE Id = @id
═══════════════════════════════════════
```

### Example 2: SQL + Parameters

```csharp
SqlSessionConfiguration.EnableSqlLogging = true;
SqlSessionConfiguration.EnableParameterLogging = true;

var users = session.SelectList<User>("IUserMapper.FindByRoles",
    new Dictionary<string, object>
    {
        ["roles"] = new List<string> { "Admin", "Manager" }
    });
```

**Output:**

```
═══════════════════════════════════════
[MyBatis.NET SQL] 14:52:07.910
───────────────────────────────────────
SELECT * FROM Users WHERE Role IN (@role_0,@role_1) ORDER BY Role, UserName
───────────────────────────────────────
Parameters:
  @roles = System.Collections.Generic.List`1[System.String]
  @role_0 = 'Admin'
  @role_1 = 'Manager'
═══════════════════════════════════════
```

### Example 3: Dynamic SQL

```csharp
SqlSessionConfiguration.EnableSqlLogging = true;
SqlSessionConfiguration.EnableParameterLogging = true;

var users = session.SelectList<User>("IUserMapper.SearchUsers",
    new Dictionary<string, object>
    {
        ["userName"] = "john",
        ["role"] = "Admin"
    });
```

**Output:**

```
═══════════════════════════════════════
[MyBatis.NET SQL] 14:52:08.123
───────────────────────────────────────
SELECT * FROM Users WHERE UserName LIKE @userName AND Role = @role
───────────────────────────────────────
Parameters:
  @userName = 'john'
  @role = 'Admin'
═══════════════════════════════════════
```

## 🎯 Use Cases

### 1. Development & Debugging

Enable logging during development to see exactly what SQL is being executed:

```csharp
#if DEBUG
    SqlSessionConfiguration.EnableSqlLogging = true;
    SqlSessionConfiguration.EnableParameterLogging = true;
#endif
```

### 2. Performance Monitoring

Log SQL without parameters to track query patterns:

```csharp
SqlSessionConfiguration.EnableSqlLogging = true;
SqlSessionConfiguration.EnableParameterLogging = false; // Skip parameters for cleaner output
```

### 3. Production Troubleshooting

Enable temporarily to debug issues in production:

```csharp
// Enable from configuration
var enableLogging = Configuration.GetValue<bool>("MyBatis:EnableSqlLogging");
SqlSessionConfiguration.EnableSqlLogging = enableLogging;
```

### 4. Unit Testing

Enable in test setup to verify generated SQL:

```csharp
public class MyTests
{
    public MyTests()
    {
        SqlSessionConfiguration.EnableSqlLogging = true;
        SqlSessionConfiguration.EnableParameterLogging = true;
    }

    [Fact]
    public void Test_Query()
    {
        // SQL will be logged to test output
        var result = session.SelectList<User>(...);
    }
}
```

## 🔧 Advanced Configuration

### Custom Logging (Future Enhancement)

Currently logs to `Console.WriteLine()`. Future versions may support:

```csharp
// Future API (not implemented yet)
SqlSessionConfiguration.LogHandler = (sql, parameters) =>
{
    // Log to ILogger, file, or custom sink
    _logger.LogInformation("SQL: {Sql}", sql);
};
```

## ⚠️ Important Notes

1. **Performance Impact**: Logging adds minimal overhead, but disable in production for best performance

2. **Parameter Logging**: When `EnableParameterLogging = true`, all parameter values are converted to strings and logged. Be careful with sensitive data (passwords, etc.)

3. **Thread Safety**: Configuration is static and affects all SqlSession instances globally

4. **Console Output**: Logs are written to `Console.WriteLine()`, which appears in:
   - Visual Studio Output window
   - Test Explorer output
   - Console applications
   - Docker logs

## 📚 Related Documentation

- [Dynamic SQL](./DYNAMIC_SQL_IMPLEMENTATION.md) - Learn about dynamic SQL features
- [Testing Guide](./TESTING_GUIDE.md) - Unit and integration testing
- [Examples](./Examples/) - More code examples

## 🤝 Contributing

Found a bug or have a suggestion? Please open an issue on GitHub!
