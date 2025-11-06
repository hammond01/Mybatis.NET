# MyBatis.NET Mapper Interface Generator

[![NuGet Version](https://img.shields.io/nuget/v/MyBatis.NET.SqlMapper.Tool.svg)](https://www.nuget.org/packages/MyBatis.NET.SqlMapper.Tool)

**Auto-generate C# interfaces from XML mappers** - Keep your interface and XML in perfect sync!

## 🎯 Problem Solved

When writing MyBatis mappers, you have to maintain 2 files:

1. **UserMapper.xml** - Define SQL statements
2. **IUserMapper.cs** - Define interface methods

❌ **Problem**: Easy to get out of sync:

- Add method in XML but forget to update interface
- Rename method/parameter in XML but not in interface
- Parameter types don't match

✅ **Solution**: This tool auto-generates interfaces from XML!

## � Installation

### Global Installation (Recommended)

Install the tool globally to use it anywhere:

```bash
dotnet tool install -g MyBatis.NET.SqlMapper.Tool
```

After installation, the `mybatis-gen` command will be available globally.

### Local Installation (Per Project)

Install the tool for a specific project:

```bash
# Create tool manifest if not exists
dotnet new tool-manifest

# Install the tool
dotnet tool install MyBatis.NET.SqlMapper.Tool

# Restore tools
dotnet tool restore
```

Use with `dotnet tool run mybatis-gen` or `dotnet mybatis-gen`.

### Update Tool

```bash
# Global update
dotnet tool update -g MyBatis.NET.SqlMapper.Tool

# Local update
dotnet tool update MyBatis.NET.SqlMapper.Tool
```

## 🚀 Usage

### 1. Generate from a Single XML File

```bash
mybatis-gen generate Mappers/UserMapper.xml
```

Output: `Mappers/IUserMapper.cs`

With custom output path and namespace:

```bash
mybatis-gen generate Mappers/UserMapper.xml Generated/IUserMapper.cs MyApp.Data.Mappers
```

### 2. Generate All XML Files in a Directory

```bash
mybatis-gen generate-all Mappers
```

With custom namespace:

```bash
mybatis-gen generate-all Mappers MyApp.Data.Mappers
```

### 3. Show Help

```bash
mybatis-gen help
```

## 📝 Input Example (UserMapper.xml)

```xml
<mapper namespace="IUserMapper">
  <select id="GetAll" resultType="User">
    SELECT * FROM Users ORDER BY UserName
  </select>

  <select id="GetById" parameterType="int" resultType="User">
    SELECT * FROM Users WHERE Id = @id
  </select>

  <select id="SearchUsers" resultType="User">
    SELECT * FROM Users
    <where>
      <if test="userName != null">
        UserName LIKE '%' + @userName + '%'
      </if>
      <if test="role != null">
        AND Role = @role
      </if>
    </where>
  </select>

  <select id="FindByRoles" resultType="User">
    SELECT * FROM Users WHERE Role IN
    <foreach collection="roles" item="role" open="(" separator="," close=")">
      @role
    </foreach>
  </select>

  <insert id="InsertUser" parameterType="User">
    INSERT INTO Users (UserName, Email) VALUES (@UserName, @Email)
  </insert>

  <delete id="DeleteUser" parameterType="int">
    DELETE FROM Users WHERE Id = @id
  </delete>
</mapper>
```

## ✨ Output Example (IUserMapper.cs)

```csharp
using System;
using System.Collections.Generic;

namespace MyBatis.ConsoleTest.Mappers;

/// <summary>
/// Auto-generated from UserMapper.xml
/// Generated at: 2025-11-05 15:27:14
/// </summary>
public interface IUserMapper
{
    List<User> GetAll();

    List<User> GetById(int id);

    List<User> SearchUsers(string? userName, string? role);

    List<User> FindByRoles(List<string> roles);

    int InsertUser(User user);

    int DeleteUser(int id);
}
```

## 🤖 Smart Features

### 1. **Auto-detect Parameters**

The tool automatically analyzes:

- `@paramName` in SQL statements
- `<if test="...">` conditions
- `<foreach collection="...">` collections
- `parameterType` attribute

### 2. **Type Inference**

Smart type guessing based on parameter names:

- `id`, `count`, `age` → `int?`
- `userName`, `email`, `role`, `name` → `string?`
- `date`, `time`, `createdDate` → `DateTime?`
- `isActive`, `enabled`, `isDeleted` → `bool?`
- `price`, `amount` → `decimal?`

### 3. **Return Type Detection**

Based on `returnSingle` attribute (v2.0.0+):

- `<select returnSingle="false">` → `List<T>` (T from `resultType`)
- `<select returnSingle="true">` → `T?` (nullable single object)
- `<insert>`, `<update>`, `<delete>` → `int` (rows affected)

### 4. **Smart Naming**

- `UserMapper.xml` → `IUserMapper.cs`
- `ProductMapper.xml` → `IProductMapper.cs`
- Auto-prefix "I" if not present

## 📋 Command Reference

| Command                   | Description               | Example                                  |
| ------------------------- | ------------------------- | ---------------------------------------- |
| `generate`, `gen`         | Generate from single XML  | `mybatis-gen gen Mappers/UserMapper.xml` |
| `generate-all`, `gen-all` | Generate all in directory | `mybatis-gen gen-all Mappers`            |
| `help`, `-h`, `--help`    | Show help                 | `mybatis-gen help`                       |

## 🔧 Advanced Usage

### 1. CI/CD Integration

Add to your build script (e.g., `.github/workflows/build.yml`):

```yaml
- name: Generate Mapper Interfaces
  run: |
    dotnet tool restore
    dotnet mybatis-gen generate-all Mappers MyApp.Data.Mappers
```

Or in a shell script:

```bash
# Generate all interfaces before build
mybatis-gen generate-all ./MyApp/Mappers MyApp.Data.Mappers
dotnet build
```

### 2. Pre-commit Hook

Create `.git/hooks/pre-commit`:

```bash
#!/bin/sh
# Auto-generate interfaces before commit
mybatis-gen generate-all ./Mappers
git add ./Mappers/*.cs
```

Make it executable:

```bash
chmod +x .git/hooks/pre-commit
```

### 3. Local Development Workflow

Add npm scripts or make commands for convenience:

**package.json** (if using npm):

```json
{
  "scripts": {
    "gen": "mybatis-gen generate-all Mappers",
    "build": "npm run gen && dotnet build"
  }
}
```

**Makefile**:

```makefile
.PHONY: gen build

gen:
	mybatis-gen generate-all Mappers

build: gen
	dotnet build
```

## ⚠️ Limitations

1. **Type inference is not 100% accurate** - Always review generated code
2. **Complex types** - Only detects basic types (int, string, DateTime, etc.)
3. **Custom collections** - Defaults to `List<T>`, may need manual adjustment
4. **Method overloading** - Not supported (XML doesn't support it either)
5. **Generic types** - Limited support for complex generics

## 💡 Best Practices

1. **Always review generated code** before using in production
2. **Don't edit generated files manually** - Regenerate from XML instead
3. **Version control strategy**:
   - **Option A**: Commit generated `.cs` files (easier for team)
   - **Option B**: Add to `.gitignore` and generate in CI/CD (ensures fresh)
4. **Run in CI/CD** to ensure interface/XML sync before build
5. **Use meaningful parameter names** in XML for better type inference
6. **Keep XML as source of truth** - Update XML, then regenerate interface
7. **Add XML validation** to catch errors early (missing `returnSingle`, etc.)

## 📚 Examples

### Example 1: Simple CRUD

**XML:**

```xml
<mapper namespace="IProductMapper">
  <select id="GetAll" resultType="Product" returnSingle="false">
    SELECT * FROM Products
  </select>

  <select id="GetById" resultType="Product" returnSingle="true">
    SELECT * FROM Products WHERE Id = @id
  </select>

  <insert id="Insert" parameterType="Product">
    INSERT INTO Products (Name, Price) VALUES (@Name, @Price)
  </insert>
</mapper>
```

**Generated:**

```csharp
public interface IProductMapper
{
    List<Product> GetAll();
    Product? GetById(int id);
    int Insert(Product product);
}
```

### Example 2: Dynamic SQL

**XML:**

```xml
<mapper namespace="IOrderMapper">
  <select id="Search" resultType="Order" returnSingle="false">
    SELECT * FROM Orders
    <where>
      <if test="customerId != null">
        CustomerId = @customerId
      </if>
      <if test="status != null">
        AND Status = @status
      </if>
      <if test="minAmount != null">
        AND TotalAmount >= @minAmount
      </if>
    </where>
  </select>
</mapper>
```

**Generated:**

```csharp
public interface IOrderMapper
{
    List<Order> Search(int? customerId, string? status, decimal? minAmount);
}
```

### Example 3: ForEach Collection

**XML:**

```xml
<mapper namespace="ICategoryMapper">
  <select id="FindByIds" resultType="Category" returnSingle="false">
    SELECT * FROM Categories
    WHERE Id IN
    <foreach collection="ids" item="id" open="(" separator="," close=")">
      @id
    </foreach>
  </select>
</mapper>
```

**Generated:**

```csharp
public interface ICategoryMapper
{
    List<Category> FindByIds(List<int> ids);
}
```

> **Note**: Tool detects `ids` collection and generates `List<int>` based on `id` name inference.

## 🔍 Troubleshooting

### "command not found: mybatis-gen"

**Solution**: Make sure the tool is installed and `~/.dotnet/tools` is in your PATH.

```bash
# Check if tool is installed
dotnet tool list -g

# Add to PATH (Linux/Mac - add to ~/.bashrc or ~/.zshrc)
export PATH="$PATH:$HOME/.dotnet/tools"

# Windows - add to PATH environment variable
%USERPROFILE%\.dotnet\tools
```

### "Invalid XML mapper file"

**Solution**: Ensure your XML has:

- Valid XML structure
- `<mapper namespace="...">` root element
- `returnSingle` attribute on all `<select>` statements (v2.0.0+)

### "Type inference incorrect"

**Solution**: The tool uses heuristics. Review and manually adjust types if needed:

```csharp
// Generated (may need adjustment)
int? customerId

// Adjust if needed
Guid? customerId
```

## 🤝 Contributing

Found a bug? Have a suggestion?

- **GitHub**: [https://github.com/hammond01/MyBatis.NET](https://github.com/hammond01/MyBatis.NET)
- **Issues**: [Report bugs or request features](https://github.com/hammond01/MyBatis.NET/issues)
- **Pull Requests**: Contributions welcome!

## 📄 License

MIT License - Same as MyBatis.NET

---

**Related Documentation:**

- [MyBatis.NET Main README](../README.md)
- [Usage Guide](../USAGE_GUIDE.md)
- [Quick Reference](../QUICK_REFERENCE.md)
- [SQL Logging](../SQL_LOGGING.md)
