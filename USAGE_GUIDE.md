# 📖 MyBatis.NET - Complete Usage Guide

A comprehensive guide to using MyBatis.NET with XML-based SQL mapping, dynamic SQL, and code generation.

## 🚀 Quick Start

### 1️⃣ Create XML Mapper

```xml
<?xml version="1.0" encoding="utf-8" ?>
<mapper namespace="IUserMapper">

  <!-- Query returns LIST -->
  <select id="GetAll" resultType="User" returnSingle="false">
    SELECT * FROM Users ORDER BY UserName
  </select>

  <!-- Query returns single OBJECT -->
  <select id="GetById" resultType="User" returnSingle="true">
    SELECT * FROM Users WHERE Id = @id
  </select>

  <!-- Dynamic SQL with IF -->
  <select id="SearchUsers" resultType="User" returnSingle="false">
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

  <!-- ForEach with collection -->
  <select id="FindByRoles" resultType="User" returnSingle="false">
    SELECT * FROM Users
    WHERE Role IN
    <foreach collection="roles" item="role" open="(" separator="," close=")">
      @role
    </foreach>
  </select>

  <!-- INSERT -->
  <insert id="InsertUser">
    INSERT INTO Users (UserName, Email, Age, Role)
    VALUES (@UserName, @Email, @Age, @Role)
  </insert>

  <!-- UPDATE with dynamic SET -->
  <update id="UpdateUser">
    UPDATE Users
    <set>
      <if test="UserName != null">UserName = @UserName,</if>
      <if test="Email != null">Email = @Email,</if>
      <if test="Age != null">Age = @Age,</if>
    </set>
    WHERE Id = @Id
  </update>

  <!-- DELETE -->
  <delete id="DeleteUser">
    DELETE FROM Users WHERE Id = @id
  </delete>

</mapper>
```

### 2️⃣ Create Mapper Interface (Auto-generate or Manual)

#### Option 1: Auto-generate from XML (Recommended ⭐)

```bash
# Generate interface from XML
cd Tools
dotnet run generate ../Mappers/UserMapper.xml MyApp.Mappers

# Or generate all XML files in folder
dotnet run generate-all ../Mappers MyApp.Mappers
```

#### Option 2: Write manually

```csharp
namespace MyApp.Mappers;

public interface IUserMapper
{
    // returnSingle="false" → List<User>
    List<User> GetAll();

    // returnSingle="true" → User? (nullable)
    User? GetById(int id);

    // Dynamic SQL parameters
    List<User> SearchUsers(string? userName, string? role);

    // ForEach collection
    List<User> FindByRoles(List<string> roles);

    // INSERT/UPDATE/DELETE → int (affected rows)
    int InsertUser(User user);
    int UpdateUser(User user);
    int DeleteUser(int id);
}
```

### 3️⃣ Use in Your Code

```csharp
using MyBatis.NET.Core;
using MyBatis.NET.Mapper;
using MyApp.Mappers;

// ========================================
// ENABLE SQL LOGGING (Optional)
// ========================================
SqlSessionConfiguration.EnableSqlLogging = true;
SqlSessionConfiguration.EnableParameterLogging = true;

// ========================================
// LOAD MAPPERS
// ========================================
MapperAutoLoader.AutoLoad("Mappers"); // Auto-load all XML files in folder

// ========================================
// USE MAPPER
// ========================================
var connectionString = "Server=.;Database=MyDB;Integrated Security=true";

using var session = new SqlSession(connectionString);

// Get Mapper Proxy (Type-safe)
var mapper = session.GetMapper<IUserMapper>();

// Simple query
var allUsers = mapper.GetAll();
Console.WriteLine($"Found {allUsers.Count} users");

// Query with parameter
var user = mapper.GetById(1);
Console.WriteLine($"User: {user?.UserName}");

// Dynamic SQL
var results = mapper.SearchUsers("john", "Admin");
Console.WriteLine($"Search results: {results.Count}");

// ForEach collection
var roleUsers = mapper.FindByRoles(new List<string> { "Admin", "Manager" });
Console.WriteLine($"Users in Admin/Manager: {roleUsers.Count}");

// INSERT
var newUser = new User
{
    UserName = "john",
    Email = "john@example.com",
    Age = 25,
    Role = "User"
};
int affected = mapper.InsertUser(newUser);
Console.WriteLine($"Inserted {affected} row(s)");

// UPDATE
user.Email = "newemail@example.com";
mapper.UpdateUser(user);

// DELETE
mapper.DeleteUser(1);
```

---

## 📋 Core Features

### ✅ 1. Dynamic SQL

Build SQL queries dynamically based on runtime conditions.

```xml
<select id="SearchUsers" resultType="User" returnSingle="false">
  SELECT * FROM Users
  <where>
    <if test="userName != null">
      UserName LIKE '%' + @userName + '%'
    </if>
    <if test="email != null">
      AND Email LIKE '%' + @email + '%'
    </if>
    <if test="isActive != null">
      AND IsActive = @isActive
    </if>
  </where>
</select>
```

**Usage:**

```csharp
// Search by userName only
var users = mapper.SearchUsers("john", null, null);

// Search by userName and email
var users = mapper.SearchUsers("john", "gmail.com", null);

// Search by all conditions
var users = mapper.SearchUsers("john", "gmail.com", true);
```

### ✅ 2. ForEach (Collection Parameters)

Iterate over collections to build IN clauses or batch operations.

```xml
<select id="FindByIds" resultType="User" returnSingle="false">
  SELECT * FROM Users
  WHERE Id IN
  <foreach collection="ids" item="id" open="(" separator="," close=")">
    @id
  </foreach>
</select>
```

**Usage:**

```csharp
var ids = new List<int> { 1, 2, 3, 4, 5 };
var users = mapper.FindByIds(ids);
```

### ✅ 3. CHOOSE/WHEN/OTHERWISE (Switch-Case in SQL)

Implement conditional logic similar to switch-case statements.

```xml
<select id="FindByRole" resultType="User" returnSingle="false">
  SELECT * FROM Users
  <where>
    <choose>
      <when test="roleType == 'admin'">
        Role = 'Admin'
      </when>
      <when test="roleType == 'manager'">
        Role = 'Manager'
      </when>
      <otherwise>
        Role = 'User'
      </otherwise>
    </choose>
  </where>
</select>
```

### ✅ 4. SET (Dynamic UPDATE)

Update only the fields that have values.

```xml
<update id="UpdateUser">
  UPDATE Users
  <set>
    <if test="UserName != null">UserName = @UserName,</if>
    <if test="Email != null">Email = @Email,</if>
    <if test="Age != null">Age = @Age,</if>
    <if test="Role != null">Role = @Role,</if>
  </set>
  WHERE Id = @Id
</update>
```

**Update only specified fields:**

```csharp
var user = new User { Id = 1, Email = "newemail@example.com" };
mapper.UpdateUser(user); // Only updates Email, leaves other fields unchanged
```

### ✅ 5. SQL Logging

Debug your SQL queries with built-in logging.

```csharp
// Enable logging
SqlSessionConfiguration.EnableSqlLogging = true;
SqlSessionConfiguration.EnableParameterLogging = true;

// Execute query
var users = mapper.SearchUsers("john", "Admin");

// Console output:
// ========================================
// [SQL] SearchUsers
// ========================================
// SELECT * FROM Users
// WHERE UserName LIKE '%' + @userName + '%'
//   AND Role = @role
// ----------------------------------------
// Parameters:
//   @userName = john
//   @role = Admin
// ========================================
```

---

## 📝 Important Rules

### ⚠️ 1. `returnSingle` Attribute is REQUIRED

The `returnSingle` attribute is mandatory for all `<select>` statements to explicitly define the return type.

```xml
<!-- ✅ CORRECT: Declares returnSingle -->
<select id="GetAll" resultType="User" returnSingle="false">
  SELECT * FROM Users
</select>

<select id="GetById" resultType="User" returnSingle="true">
  SELECT * FROM Users WHERE Id = @id
</select>

<!-- ❌ WRONG: Missing returnSingle -->
<select id="GetUsers" resultType="User">
  SELECT * FROM Users
</select>
<!-- Error: Missing REQUIRED attribute 'returnSingle' -->
```

**Convention:**

- `returnSingle="true"` → Interface must return `User?` (nullable)
- `returnSingle="false"` → Interface must return `List<User>`

### ⚠️ 2. Interface and XML Must Be in Sync

**How to ensure sync:**

1. **Write XML first** → **Generate Interface** (Recommended)

   ```bash
   dotnet run generate Mappers/UserMapper.xml MyApp.Mappers
   ```

2. **If writing manually**, ensure:
   - Method name in Interface = `id` in XML
   - Parameters must match `@paramName` in SQL
   - Return type must match `returnSingle` and `resultType`

### ⚠️ 3. Parameter Naming Convention

```xml
<!-- XML uses @paramName -->
<select id="GetById" resultType="User" returnSingle="true">
  SELECT * FROM Users WHERE Id = @id
</select>
```

```csharp
// Interface: parameter name must match (case-insensitive)
User? GetById(int id); // ✅ CORRECT

User? GetById(int userId); // ❌ WRONG - doesn't match @id
```

---

## 🛠️ Code Generation Tool

### Why Use the Generator?

❌ **Problems with manual writing:**

- Interface and XML can get out of sync
- Easy to forget parameters
- Wrong return types
- Time-consuming to maintain

✅ **Solution:**

```bash
# Generate interface from XML
cd Tools
dotnet run generate ../Mappers/UserMapper.xml MyApp.Mappers
```

**Output:** `IUserMapper.cs` is automatically generated with:

- ✅ All methods from XML
- ✅ Parameters detected from SQL
- ✅ Correct return types based on `returnSingle`
- ✅ Smart type inference (id→int, name→string?, etc.)

### Commands:

```bash
# Generate single file
dotnet run generate <xml-file> <namespace>
dotnet run generate Mappers/UserMapper.xml MyApp.Mappers

# Generate all XML files in folder
dotnet run generate-all <folder> <namespace>
dotnet run generate-all Mappers MyApp.Mappers

# Help
dotnet run help
```

---

## 📂 Recommended Project Structure

```
MyProject/
├── Mappers/
│   ├── UserMapper.xml          # XML Mapper
│   ├── ProductMapper.xml
│   └── OrderMapper.xml
├── Interfaces/
│   ├── IUserMapper.cs          # Generated Interface
│   ├── IProductMapper.cs
│   └── IOrderMapper.cs
├── Models/
│   ├── User.cs
│   ├── Product.cs
│   └── Order.cs
└── Program.cs
```

---

## 🎯 Best Practices

### 1. Always Use the Generator

```bash
# After editing XML, re-run generator
dotnet run generate-all Mappers MyApp.Mappers
```

### 2. Enable SQL Logging During Development

```csharp
#if DEBUG
SqlSessionConfiguration.EnableSqlLogging = true;
SqlSessionConfiguration.EnableParameterLogging = true;
#endif
```

### 3. Use Dynamic SQL Instead of Multiple Methods

```csharp
// ❌ Avoid: Creating multiple methods for each case
List<User> FindByName(string name);
List<User> FindByEmail(string email);
List<User> FindByNameAndEmail(string name, string email);

// ✅ Better: Use one method with Dynamic SQL
List<User> SearchUsers(string? name, string? email);
```

### 4. Nullable Parameters for Optional Filters

```csharp
// Parameters with ? = optional
List<User> SearchUsers(string? userName, string? role, int? minAge);

// Call with null if filter not needed
var users = mapper.SearchUsers("john", null, null);
```

### 5. Transaction Handling

```csharp
using var session = new SqlSession(connectionString);
var mapper = session.GetMapper<IUserMapper>();

try
{
    // Multiple operations in one transaction
    var newUser = new User { UserName = "test", Email = "test@mail.com" };
    mapper.InsertUser(newUser);

    mapper.UpdateUser(existingUser);

    mapper.DeleteUser(oldUserId);

    // All successful
}
catch (Exception ex)
{
    Console.WriteLine($"Transaction failed: {ex.Message}");
    // Automatic rollback when session is disposed
}
```

---

## 🔍 Troubleshooting

### ❌ Error: "Missing REQUIRED attribute 'returnSingle'"

**Cause:** Missing `returnSingle` in `<select>`

**Fix:**

```xml
<select id="GetUsers" resultType="User" returnSingle="false">
  SELECT * FROM Users
</select>
```

### ❌ Error: "Method not found"

**Cause:** Interface method name doesn't match XML `id`

**Fix:**

```xml
<select id="GetById" ...>  <!-- id must match method name -->
```

```csharp
User? GetById(int id);  // ✅ Must match id="GetById"
```

### ❌ Error: "Parameter not found: @id"

**Cause:** Parameter name in Interface doesn't match `@param` in SQL

**Fix:**

```xml
WHERE Id = @id  <!-- @id in SQL -->
```

```csharp
User? GetById(int id);  // Parameter name must be 'id'
```

### ❌ SQL Logging Not Working

**Cause:** Logging not enabled

**Fix:**

```csharp
SqlSessionConfiguration.EnableSqlLogging = true;
SqlSessionConfiguration.EnableParameterLogging = true;
```

---

## 💡 Tips & Tricks

### 1. Debug SQL Queries

```csharp
// Enable logging to see generated SQL
SqlSessionConfiguration.EnableSqlLogging = true;
SqlSessionConfiguration.EnableParameterLogging = true;

var users = mapper.SearchUsers("test", null, null);
// Check console to debug SQL
```

### 2. Complex Objects

```csharp
// Instead of many parameters, use an object
public class UserSearchCriteria
{
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
}

List<User> SearchUsers(UserSearchCriteria criteria);
```

### 3. Reuse Connection

```csharp
// ✅ Good: Reuse session for multiple queries
using var session = new SqlSession(connectionString);
var userMapper = session.GetMapper<IUserMapper>();
var productMapper = session.GetMapper<IProductMapper>();

var users = userMapper.GetAll();
var products = productMapper.GetAll();

// ❌ Avoid: Creating new session for each query
using var session1 = new SqlSession(connectionString);
var users = session1.GetMapper<IUserMapper>().GetAll();

using var session2 = new SqlSession(connectionString);
var products = session2.GetMapper<IProductMapper>().GetAll();
```

### 4. Async Operations

```csharp
// All operations have async versions
var users = await mapper.GetAllAsync();
var user = await mapper.GetByIdAsync(1);
await mapper.InsertUserAsync(newUser);
```

---

## 📚 Additional Documentation

- **[QUICK_REFERENCE.md](QUICK_REFERENCE.md)** - Quick reference and cheat sheet
- **[SQL_LOGGING.md](SQL_LOGGING.md)** - SQL logging configuration
- **[Tools/README.md](Tools/README.md)** - Code generator documentation
- **[MyBatis.ConsoleTest/](MyBatis.ConsoleTest/)** - Demo project with examples

---

## 🌟 Common Patterns

### Pattern 1: Search with Optional Filters

```csharp
List<User> SearchUsers(string? name, string? email, int? age);

// Usage:
mapper.SearchUsers("john", null, null);  // Search by name only
mapper.SearchUsers(null, "gmail.com", null);  // Search by email only
mapper.SearchUsers("john", "gmail.com", 25);  // Search all
```

### Pattern 2: ForEach with Collections

```csharp
var ids = new List<int> { 1, 2, 3 };
var users = mapper.FindByIds(ids);

var roles = new List<string> { "Admin", "Manager" };
var users = mapper.FindByRoles(roles);
```

### Pattern 3: Partial Updates

```csharp
// Only update non-null fields
var user = new User { Id = 1, Email = "newemail@example.com" };
mapper.UpdateUser(user);  // Only Email is updated
```

### Pattern 4: Pagination

```xml
<select id="GetUsersPage" resultType="User" returnSingle="false">
  SELECT * FROM Users
  ORDER BY Id
  OFFSET @offset ROWS
  FETCH NEXT @pageSize ROWS ONLY
</select>
```

```csharp
// Get page 2 with 10 items per page
var users = mapper.GetUsersPage(offset: 10, pageSize: 10);
```

---

## 🚦 Recommended Workflow

```
1. Write XML Mapper
   ↓
2. Run Generator
   ↓
3. Use Interface
   ↓
4. If logic changes:
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

**Happy Coding! 🚀**
