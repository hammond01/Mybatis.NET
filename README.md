# MyBatis.NET

[![NuGet Version](https://img.shields.io/nuget/v/MyBatis.NET.SqlServer.svg)](https://www.nuget.org/packages/MyBatis.NET.SqlServer)
[![NuGet Downloads](https://img.shields.io/nuget/dt/MyBatis.NET.SqlServer.svg)](https://www.nuget.org/packages/MyBatis.NET.SqlServer/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

A lightweight MyBatis port for .NET, providing XML-based SQL mapping, runtime proxy generation, and transaction support.

## 📚 Documentation

- **[QUICK_REFERENCE.md](QUICK_REFERENCE.md)** - Quick Start & Cheat Sheet
- **[USAGE_GUIDE.md](USAGE_GUIDE.md)** - Comprehensive Usage Guide
- **[SQL_LOGGING.md](SQL_LOGGING.md)** - SQL Logging Configuration
- **[Tools/README.md](Tools/README.md)** - Code Generator Tool

## Features

- **XML Mappers**: Define SQL statements in XML files with mandatory `returnSingle` attribute
- **Dynamic SQL**: Support for `<if>`, `<where>`, `<set>`, `<choose>`, `<foreach>`, `<trim>` tags (like MyBatis Java)
- **Runtime Proxy**: Automatically generate mapper implementations using dynamic proxies
- **Optimized Performance**: Compiled Expression Trees with smart caching (2-3x faster for repeated queries)
- **Code Generator**: Auto-generate C# interfaces from XML mappers (keeps them in sync!)
- **SQL Logging**: Built-in SQL query and parameter logging for debugging
- **Transaction Support**: Built-in transaction management
- **Result Mapping**: Automatic mapping of query results to .NET objects with optimized caching
- **ADO.NET Integration**: Uses Microsoft.Data.SqlClient for database connectivity
- **Async Support**: Full asynchronous operations for all database interactions
- **DDD Support**: Load mappers from multiple libraries and embedded resources


## Installation

Install via NuGet:

```bash
dotnet add package MyBatis.NET.SqlServer
```

Or using Package Manager:

```powershell
Install-Package MyBatis.NET.SqlServer
```

## 📖 Demo Project

A complete working demo is included in this repository at **[Demo/MyBatis.TestApp](Demo/MyBatis.TestApp/)**!

**Features demonstrated:**

- ✅ **Dynamic SQL** - Full showcase of `<if>`, `<where>`, `<foreach>`, `<choose>`, `<set>`, and nested conditions
- ✅ **Code Generator Tool** - How to use `mybatis-gen` to auto-generate interfaces
- ✅ **returnSingle attribute** - Proper usage in v1.0.0
- ✅ **Basic CRUD** - Users table with simple operations
- ✅ **Complex Queries** - Products table with multi-filter search, category filtering, and dynamic updates
- ✅ **DDD Architecture** - Domain, Application, Infrastructure, and Presentation layers
- ✅ **ASP.NET Core Web API** - RESTful endpoints with Swagger UI
- ✅ **Async/Await** - Full async support
- ✅ **Transaction Management** - Unit of Work pattern

**Quick start:**

```bash
cd Demo/MyBatis.TestApp
dotnet restore
dotnet run --project Presentation
# Open https://localhost:5001/swagger
```

See **[Demo/MyBatis.TestApp/README.md](Demo/MyBatis.TestApp/README.md)** for detailed setup and API documentation.

## Quick Start

### 1. Define Your Entity

```csharp
public class User
{
    public int Id { get; set; }
    public string UserName { get; set; } = "";
    public string Email { get; set; } = "";
}
```

### 2. Create Mapper Interface

```csharp
public interface IUserMapper
{
    List<User> GetAll();
    User GetById(int id);
    int InsertUser(User user);
    int UpdateUser(int id, string userName, string email);
    int DeleteUser(int id);
}
```

### 3. Create XML Mapper

Create a file `UserMapper.xml` in the `Mappers` directory:

```xml
<mapper namespace="IUserMapper">
  <select id="GetAll" resultType="User" returnSingle="false">
    SELECT Id, UserName, Email FROM Users
  </select>

  <select id="GetById" parameterType="int" resultType="User" returnSingle="true">
    SELECT Id, UserName, Email FROM Users WHERE Id = @Id
  </select>

  <insert id="InsertUser" parameterType="User">
    INSERT INTO Users (UserName, Email) VALUES (@UserName, @Email)
  </insert>

  <update id="UpdateUser">
    UPDATE Users SET UserName = @userName, Email = @email WHERE Id = @id
  </update>

  <delete id="DeleteUser" parameterType="int">
    DELETE FROM Users WHERE Id = @Id
  </delete>
</mapper>
```

> **⚠️ Important (v1.0.0+)**: All `<select>` statements **must** have `returnSingle` attribute:
>
> - `returnSingle="true"` for single object queries (returns `T?`)
> - `returnSingle="false"` for collection queries (returns `List<T>`)

### 4. Use in Your Code

```csharp
using MyBatis.NET.Mapper;
using MyBatis.NET.Core;

// Auto-load all XML mappers from Mappers directory
MapperAutoLoader.AutoLoad("Mappers");

// Create session with connection string
var connStr = "Server=your-server;Database=your-db;User Id=your-user;Password=your-password;";
using var session = new SqlSession(connStr);

// Get mapper instance
var mapper = session.GetMapper<IUserMapper>();

// Use mapper methods
var users = mapper.GetAll();
var user = mapper.GetById(1);
var rowsAffected = mapper.InsertUser(new User { UserName = "John", Email = "john@example.com" });
mapper.UpdateUser(1, "UpdatedName", "updated@example.com");
mapper.DeleteUser(2);
```

## Code Generator Tool

MyBatis.NET provides a **code generator tool** to auto-generate C# interfaces from XML mappers, keeping them in perfect sync!

### Installation

Install the tool globally:

```bash
dotnet tool install -g MyBatis.NET.SqlMapper.Tool
```

Or locally for your project:

```bash
dotnet new tool-manifest  # if not already exists
dotnet tool install MyBatis.NET.SqlMapper.Tool
```

### Usage

Generate interface from a single XML file:

```bash
mybatis-gen generate Mappers/UserMapper.xml
```

Generate all interfaces in a directory:

```bash
mybatis-gen generate-all Mappers
```

With custom namespace:

```bash
mybatis-gen generate Mappers/UserMapper.xml MyApp.Data.Mappers
```

**Features:**

- ✅ Auto-detects parameters from SQL (`@paramName`) and dynamic tags
- ✅ Smart type inference (`id` → `int`, `name` → `string?`, etc.)
- ✅ Correct return types based on `returnSingle` attribute
- ✅ Handles `<foreach>` collections automatically

See **[Tools/README.md](Tools/README.md)** for complete documentation.

## Custom Mapper Folders

For projects with multiple libraries (e.g., DDD architecture), you can load mappers from multiple directories:

```csharp
// Load from multiple directories
MapperAutoLoader.AutoLoad("Mappers", "../Domain/Mappers", "../Infrastructure/Mappers");

// Or load from embedded resources in assemblies (useful for library projects)
MapperAutoLoader.AutoLoadFromAssemblies(typeof(MyClass).Assembly, typeof(OtherClass).Assembly);
```

To embed XML files as resources in your library:

1. Add XML files to your project
2. Set "Build Action" to "Embedded Resource" in file properties
3. Use `AutoLoadFromAssemblies()` to load them

## Async Operations

All database operations support async execution:

```csharp
using var session = new SqlSession(connStr);
var mapper = session.GetMapper<IUserMapper>();

// Async operations
var users = await mapper.GetAllAsync();
var user = await mapper.GetByIdAsync(1);
var rowsAffected = await mapper.InsertUserAsync(new User { UserName = "John", Email = "john@example.com" });
await mapper.UpdateUserAsync(1, "UpdatedName", "updated@example.com");
await mapper.DeleteUserAsync(2);
```

## Transactions

```csharp
using var session = new SqlSession(connStr);
session.BeginTransaction();

try
{
    var mapper = session.GetMapper<IUserMapper>();
    mapper.InsertUser(new User { UserName = "Jane", Email = "jane@example.com" });
    session.Commit();
}
catch
{
    session.Rollback();
    throw;
}
```

## Configuration

### Connection String

MyBatis.NET uses standard ADO.NET connection strings. Ensure your database supports the operations defined in your mappers.

### Mapper Files

- Place XML mapper files in a `Mappers` directory
- Use `MapperAutoLoader.AutoLoad()` to load all mappers automatically
- Or load specific files using `XmlMapperLoader.LoadFromFile(path)`

## Dynamic SQL

MyBatis.NET now supports dynamic SQL tags similar to MyBatis Java, allowing you to build flexible queries based on runtime conditions.

### `<if>` - Conditional SQL

```xml
<select id="FindUsers" resultType="User">
  SELECT * FROM Users
  <where>
    <if test="name != null">
      AND UserName = @name
    </if>
    <if test="email != null">
      AND Email = @email
    </if>
    <if test="age > 0">
      AND Age >= @age
    </if>
  </where>
</select>
```

Usage:

```csharp
// Only search by name
var users = mapper.FindUsers(name: "John", email: null, age: 0);

// Search by name and age
var users = mapper.FindUsers(name: "John", email: null, age: 18);
```

### `<where>` - Smart WHERE clause

The `<where>` tag automatically adds WHERE and removes leading AND/OR:

```xml
<select id="SearchUsers" resultType="User">
  SELECT * FROM Users
  <where>
    <if test="status != null">
      AND Status = @status
    </if>
    <if test="role != null">
      AND Role = @role
    </if>
  </where>
</select>
```

### `<set>` - Smart SET clause for UPDATE

The `<set>` tag automatically adds SET and removes trailing commas:

```xml
<update id="UpdateUser">
  UPDATE Users
  <set>
    <if test="userName != null">
      UserName = @userName,
    </if>
    <if test="email != null">
      Email = @email,
    </if>
    <if test="age > 0">
      Age = @age,
    </if>
  </set>
  WHERE Id = @id
</update>
```

### `<choose>`, `<when>`, `<otherwise>` - Switch/Case

```xml
<select id="FindUsersByType" resultType="User">
  SELECT * FROM Users
  <where>
    <choose>
      <when test="type == 'admin'">
        AND Role = 'Administrator'
      </when>
      <when test="type == 'user'">
        AND Role = 'User'
      </when>
      <otherwise>
        AND Role = 'Guest'
      </otherwise>
    </choose>
  </where>
</select>
```

### `<foreach>` - Loop for IN clauses

```xml
<select id="FindUsersByIds" resultType="User">
  SELECT * FROM Users
  WHERE Id IN
  <foreach collection="ids" item="id" separator="," open="(" close=")">
    @id
  </foreach>
</select>
```

Usage:

```csharp
var ids = new[] { 1, 2, 3, 4, 5 };
var users = mapper.FindUsersByIds(ids);
// Generates: WHERE Id IN (1, 2, 3, 4, 5)
```

### `<trim>` - Custom prefix/suffix handling

```xml
<select id="SearchUsers" resultType="User">
  SELECT * FROM Users
  <trim prefix="WHERE" prefixOverrides="AND |OR ">
    <if test="name != null">
      AND UserName LIKE @name
    </if>
    <if test="email != null">
      OR Email LIKE @email
    </if>
  </trim>
</select>
```

### Expression Syntax

Dynamic SQL conditions support:

- **Null checks**: `name != null`, `email == null`
- **Comparisons**: `age > 18`, `score >= 90`, `level < 5`, `count <= 100`
- **Equality**: `type == 'admin'`, `status != 'inactive'`
- **Logical operators**: `name != null and age > 18`, `status == 'active' or role == 'admin'`
- **Negation**: `!isDeleted`
- **Simple existence**: `name` (true if parameter exists and is not null/empty)

### Complex Example

```xml
<select id="ComplexSearch" resultType="User">
  SELECT * FROM Users
  <where>
    <if test="isActive != null">
      AND IsActive = @isActive
    </if>
    <choose>
      <when test="searchType == 'name'">
        AND UserName LIKE @searchValue
      </when>
      <when test="searchType == 'email'">
        AND Email LIKE @searchValue
      </when>
    </choose>
    <if test="roles != null">
      AND Role IN
      <foreach collection="roles" item="role" separator="," open="(" close=")">
        @role
      </foreach>
    </if>
  </where>
  ORDER BY CreatedDate DESC
</select>
```

## Supported SQL Operations

- `SELECT` (returns List<T> or single T, sync and async)
- `INSERT`, `UPDATE`, `DELETE` (returns affected row count, sync and async)

## SQL Logging

Enable SQL logging to see generated queries and parameters:

```csharp
// Enable SQL logging
SqlSessionConfiguration.EnableSqlLogging = true;

// Enable SQL + Parameter logging
SqlSessionConfiguration.EnableSqlLogging = true;
SqlSessionConfiguration.EnableParameterLogging = true;
```

**Output Example:**

```
═══════════════════════════════════════
[MyBatis.NET SQL] 14:52:07.910
───────────────────────────────────────
SELECT * FROM Users WHERE Role IN (@role_0,@role_1)
───────────────────────────────────────
Parameters:
  @role_0 = 'Admin'
  @role_1 = 'Manager'
═══════════════════════════════════════
```

📖 See [SQL_LOGGING.md](./SQL_LOGGING.md) for complete documentation.

## Requirements

- .NET 8.0 or later
- Microsoft.Data.SqlClient (included as dependency)

## Contributing

Contributions are welcome! Please feel free to submit issues and pull requests.

## License

MIT License - see LICENSE file for details.

## Version History

### Version 1.0.0-preview.1 (Current Preview)

- First public preview for the `MyBatis.NET.SqlServer` package line
- Core runtime and documentation aligned for roadmap-driven releases
- Release quality gate validated (build, unit tests, package generation)

### Version 1.0.0 (Planned Stable)

Initial release line for the SQL Server package `MyBatis.NET.SqlServer`.

- XML-based mappers with mandatory `returnSingle` on `<select>`
- Dynamic SQL tags: `<if>`, `<where>`, `<set>`, `<choose>`, `<foreach>`, `<trim>`
- Runtime mapper proxy and session-based execution
- Sync and async CRUD operations
- SQL logging via `SqlSessionConfiguration`
- Interface generator tool (`mybatis-gen`)
- Result mapping with compiled expression trees and cache statistics APIs

## Author

Hammond
