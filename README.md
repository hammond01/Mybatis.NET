# MyBatis.NET

[![NuGet Version](https://img.shields.io/nuget/v/MyBatis.NET.SqlMapper.svg)](https://www.nuget.org/packages/MyBatis.NET.SqlMapper)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Build](https://img.shields.io/github/actions/workflow/status/hammond01/Mybatis.NET/dotnet.yml?label=build)]()

A lightweight MyBatis port for .NET, providing XML-based SQL mapping, runtime proxy generation, and transaction support.

## Features

- **XML Mappers**: Define SQL statements in XML files
- **Runtime Proxy**: Automatically generate mapper implementations using dynamic proxies
- **Transaction Support**: Built-in transaction management
- **Result Mapping**: Automatic mapping of query results to .NET objects
- **ADO.NET Integration**: Uses Microsoft.Data.SqlClient for database connectivity
- **Async Support**: Full asynchronous operations for all database interactions
- **DDD Support**: Load mappers from multiple libraries and embedded resources

## Installation

Install via NuGet:

```bash
dotnet add package MyBatis.NET.SqlMapper
```

Or using Package Manager:

```powershell
Install-Package MyBatis.NET.SqlMapper
```

## Demo Project

Check out the [MyBatis.Demo](https://github.com/hammond01/MyBatis.Demo) repository for complete working examples including:

- Basic CRUD operations
- Custom mapper configurations
- DDD architecture with multiple libraries
- Async operations
- Transaction management

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
  <select id="GetAll" resultType="User">
    SELECT Id, UserName, Email FROM Users
  </select>

  <select id="GetById" parameterType="int" resultType="User">
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

## Supported SQL Operations

- `SELECT` (returns List<T> or single T, sync and async)
- `INSERT`, `UPDATE`, `DELETE` (returns affected row count, sync and async)

## Requirements

- .NET 8.0 or later
- Microsoft.Data.SqlClient (included as dependency)

## Contributing

Contributions are welcome! Please feel free to submit issues and pull requests.

## License

MIT License - see LICENSE file for details.

## Version 1.5.0

- Cleaned package by removing demo files (User.cs, IUserMapper.cs, Program.cs, and Demo folder) from compilation.
- Added full async support for all database operations.
- Enhanced documentation with complete CRUD examples.

## Author

Hammond
