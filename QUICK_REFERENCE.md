# 🚀 MyBatis.NET - Quick Reference

## Setup (3 Steps)

### 1. Create XML Mapper

```xml
<?xml version="1.0" encoding="utf-8" ?>
<mapper namespace="IUserMapper">
  <select id="GetAll" resultType="User" returnSingle="false">
    SELECT * FROM Users
  </select>

  <select id="GetById" resultType="User" returnSingle="true">
    SELECT * FROM Users WHERE Id = @id
  </select>
</mapper>
```

### 2. Generate Interface

```bash
cd Tools
dotnet run generate ../Mappers/UserMapper.xml MyApp.Mappers
```

### 3. Use It

```csharp
using MyBatis.NET.Core;
using MyBatis.NET.Mapper;

// Load mappers
MapperAutoLoader.AutoLoad("Mappers");

// Use
using var session = new SqlSession(connectionString);
var mapper = session.GetMapper<IUserMapper>();

var users = mapper.GetAll();
var user = mapper.GetById(1);
```

---

## Dynamic SQL Cheat Sheet

### IF Condition

```xml
<where>
  <if test="name != null">
    UserName LIKE '%' + @name + '%'
  </if>
  <if test="age > 0">
    AND Age >= @age
  </if>
</where>
```

### ForEach (IN clause)

```xml
WHERE Role IN
<foreach collection="roles" item="role" open="(" separator="," close=")">
  @role
</foreach>
```

### CHOOSE (Switch-Case)

```xml
<choose>
  <when test="type == 'admin'">
    Role = 'Admin'
  </when>
  <when test="type == 'user'">
    Role = 'User'
  </when>
  <otherwise>
    Role = 'Guest'
  </otherwise>
</choose>
```

### SET (Dynamic Update)

```xml
<update id="UpdateUser">
  UPDATE Users
  <set>
    <if test="UserName != null">UserName = @UserName,</if>
    <if test="Email != null">Email = @Email,</if>
  </set>
  WHERE Id = @Id
</update>
```

---

## returnSingle Rules

```xml
<!-- returnSingle="false" → List<T> -->
<select id="GetAll" resultType="User" returnSingle="false">
  SELECT * FROM Users
</select>
```

```csharp
List<User> GetAll(); // ✅
```

```xml
<!-- returnSingle="true" → T? (nullable) -->
<select id="GetById" resultType="User" returnSingle="true">
  SELECT * FROM Users WHERE Id = @id
</select>
```

```csharp
User? GetById(int id); // ✅
```

**⚠️ returnSingle is REQUIRED!**

---

## SQL Logging

```csharp
// Enable logging
SqlSessionConfiguration.EnableSqlLogging = true;
SqlSessionConfiguration.EnableParameterLogging = true;

// Queries automatically log SQL + parameters to console
var users = mapper.GetAll();

// Disable logging
SqlSessionConfiguration.EnableSqlLogging = false;
```

---

## Generator Commands

```bash
# Generate single file
dotnet run generate <xml-file> <namespace>

# Generate all
dotnet run generate-all <folder> <namespace>

# Examples:
dotnet run generate Mappers/UserMapper.xml MyApp.Mappers
dotnet run generate-all Mappers MyApp.Mappers
```

---

## Common Patterns

### Query with Optional Filters

```csharp
List<User> SearchUsers(string? name, string? email, int? age);

// Usage:
mapper.SearchUsers("john", null, null);  // Search by name only
mapper.SearchUsers(null, "gmail.com", null);  // Search by email only
mapper.SearchUsers("john", "gmail.com", 25);  // Search all
```

### ForEach with List

```csharp
var ids = new List<int> { 1, 2, 3 };
var users = mapper.FindByIds(ids);

var roles = new List<string> { "Admin", "Manager" };
var users = mapper.FindByRoles(roles);
```

### Complex Object Parameter

```csharp
public class SearchCriteria
{
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public int? MinAge { get; set; }
}

List<User> SearchUsers(SearchCriteria criteria);
```

---

## Recommended Workflow

```
1. Write XML Mapper
   ↓
2. Run Generator
   ↓
3. Use Interface
   ↓
4. If logic needs changes:
   - Edit XML
   - Re-run Generator
   - Interface auto-updates
```

**Benefits:**

- ✅ XML and Interface always in sync
- ✅ No parameter mismatches
- ✅ Type-safe
- ✅ Saves time

---

## See Also

- **[USAGE_GUIDE.md](USAGE_GUIDE.md)** - Complete usage guide
- **[SQL_LOGGING.md](SQL_LOGGING.md)** - SQL Logging
- **[Tools/README.md](Tools/README.md)** - Code Generator
- **[MyBatis.ConsoleTest/](MyBatis.ConsoleTest/)** - Demo project
