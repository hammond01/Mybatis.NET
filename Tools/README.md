# MyBatis.NET Mapper Interface Generator

**Tự động generate Interface từ XML Mapper** - Đảm bảo Interface và XML luôn đồng bộ!

## 🎯 Vấn đề giải quyết

Khi viết MyBatis mapper, bạn phải maintain 2 files:

1. **UserMapper.xml** - Define SQL statements
2. **IUserMapper.cs** - Define interface methods

❌ **Vấn đề**: Dễ bị sai lệch (mismatch) giữa XML và Interface:

- Thêm method trong XML nhưng quên update Interface
- Đổi tên method/parameter trong XML nhưng không update Interface
- Parameter type không khớp

✅ **Giải pháp**: Tool này tự động generate Interface từ XML!

## 🚀 Cách sử dụng

### 1. Generate từ một file XML

```bash
cd Tools
dotnet run generate <xml-file-path> [output-path] [namespace]
```

**Example:**

```bash
dotnet run generate ../MyBatis.ConsoleTest/Mappers/UserMapper.xml
```

Output: `../MyBatis.ConsoleTest/Mappers/IUserMapper.cs`

### 2. Generate tất cả XML trong folder

```bash
dotnet run generate-all [directory] [namespace]
```

**Example:**

```bash
dotnet run generate-all ../MyBatis.ConsoleTest/Mappers
```

### 3. Custom namespace và output path

```bash
dotnet run generate Mappers/UserMapper.xml Mappers/IUserMapper.cs MyApp.Data.Mappers
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

## 🤖 Tính năng thông minh

### 1. **Auto-detect Parameters**

Tool tự động phân tích:

- `@paramName` trong SQL
- `<if test="...">` conditions
- `<foreach collection="...">` collections
- `parameterType` attribute

### 2. **Type Inference**

Tool đoán type dựa trên tên parameter:

- `id` → `int`
- `userName`, `email`, `role` → `string?`
- `age`, `count` → `int?`
- `date`, `time` → `DateTime?`
- `isActive`, `enabled` → `bool?`

### 3. **Return Type Detection**

- `<select>` → `List<T>` (T từ `resultType`)
- `<insert>`, `<update>`, `<delete>` → `int`

### 4. **Smart Naming**

- `UserMapper.xml` → `IUserMapper.cs`
- `ProductMapper.xml` → `IProductMapper.cs`
- Auto-prefix "I" nếu chưa có

## 📋 Command Reference

| Command                   | Description               | Example                                 |
| ------------------------- | ------------------------- | --------------------------------------- |
| `generate`, `gen`         | Generate from single XML  | `dotnet run gen Mappers/UserMapper.xml` |
| `generate-all`, `gen-all` | Generate all in directory | `dotnet run gen-all Mappers`            |
| `help`, `-h`, `--help`    | Show help                 | `dotnet run help`                       |

## 🔧 Advanced Usage

### 1. CI/CD Integration

Add to your build script:

```bash
# Generate all interfaces before build
cd Tools
dotnet run generate-all ../MyApp/Mappers MyApp.Data.Mappers
```

### 2. Pre-commit Hook

Create `.git/hooks/pre-commit`:

```bash
#!/bin/sh
cd Tools
dotnet run generate-all ../MyApp/Mappers
git add ../MyApp/Mappers/*.cs
```

### 3. Watch Mode (Future)

```bash
# Auto-generate when XML changes (not implemented yet)
dotnet watch run generate-all Mappers
```

## ⚠️ Limitations

1. **Type inference không 100% chính xác** - Review generated code
2. **Complex types** - Chỉ detect basic types (int, string, DateTime, etc.)
3. **Custom collections** - Mặc định `List<string>`, có thể cần adjust
4. **Method overloading** - Không support (XML không support)

## 💡 Best Practices

1. **Review generated code** trước khi sử dụng
2. **Don't edit generated files manually** - Re-generate từ XML
3. **Add to .gitignore** nếu muốn always generate fresh
4. **Run trong CI/CD** để ensure sync
5. **Use meaningful parameter names** trong XML để type inference chính xác hơn

## 📚 Examples

### Example 1: Simple CRUD

**XML:**

```xml
<mapper namespace="IProductMapper">
  <select id="GetAll" resultType="Product">
    SELECT * FROM Products
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
    int Insert(Product product);
}
```

### Example 2: Dynamic SQL

**XML:**

```xml
<mapper namespace="IOrderMapper">
  <select id="Search" resultType="Order">
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
  <select id="FindByIds" resultType="Category">
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
    List<Category> FindByIds(List<string> ids);
}
```

## 🤝 Contributing

Found a bug? Have a suggestion?

- Open an issue
- Submit a PR
- Contact: hammond01

## 📄 License

MIT License - Same as MyBatis.NET
