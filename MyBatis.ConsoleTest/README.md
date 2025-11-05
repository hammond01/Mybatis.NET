# MyBatis.NET Console Test Project

Test project demonstrating how to use MyBatis.NET library with **Mapper Proxy Pattern**.

## 📁 Project Structure

```
MyBatis.ConsoleTest/
├── Mappers/
│   ├── IUserMapper.cs        # Mapper interface
│   └── UserMapper.xml         # XML mapper với 12 SQL statements
├── Models/
│   └── User.cs                # Entity model
├── Program.cs                 # Console test application
└── MyBatis.ConsoleTest.csproj
```

## 🎯 Key Features Demonstrated

### 1. **Mapper Proxy Pattern** (Đúng cách sử dụng MyBatis)

```csharp
// ✅ ĐÚNG - Sử dụng Mapper Interface
var mapper = session.GetMapper<IUserMapper>();
var users = mapper.GetAll();
var user = mapper.GetById(1);

// ❌ SAI - Gọi trực tiếp bằng string (không type-safe)
var users = session.SelectList<User>("IUserMapper.GetAll", null);
```

### 2. **SQL Logging Configuration**

```csharp
// Bật SQL + Parameter logging
SqlSessionConfiguration.EnableSqlLogging = true;
SqlSessionConfiguration.EnableParameterLogging = true;
```

### 3. **Dynamic SQL với IF conditions**

```csharp
var results = mapper.SearchUsers(
    userName: "john",
    role: null,
    minAge: null,
    maxAge: null,
    isActive: null
);
```

### 4. **ForEach với Collections**

```csharp
var users = mapper.FindByRoles(new List<string> { "Admin", "Manager", "User" });
```

## 🚀 How to Run

```bash
cd f:\LIB\MyBatis.NET\MyBatis.ConsoleTest
dotnet run
```

## 📊 Output Example

```
═══════════════════════════════════════
[MyBatis.NET SQL] 15:23:07.880
───────────────────────────────────────
SELECT * FROM Users WHERE Role IN (@role_0,@role_1,@role_2) ORDER BY Role, UserName
───────────────────────────────────────
Parameters:
  @roles = System.Collections.Generic.List`1[System.String]
  @role_0 = 'Admin'
  @role_1 = 'Manager'
  @role_2 = 'User'
═══════════════════════════════════════
✅ Result: Found 10 users
```

## 🔧 Setup Requirements

1. **Database**: MyBatisTestDB với bảng Users
2. **Connection String**: Update trong Program.cs nếu cần
3. **.NET 8.0**: Required

## 📚 Mapper Interface (IUserMapper.cs)

```csharp
public interface IUserMapper
{
    // Simple queries
    List<User> GetAll();
    User? GetById(int id);

    // Dynamic SQL
    List<User> SearchUsers(string? userName, string? role, int? minAge, int? maxAge, bool? isActive);

    // ForEach
    List<User> FindByRoles(List<string> roles);
    List<User> FindByIds(List<int> ids);

    // CRUD
    int InsertUser(User user);
    int UpdateUser(User user);
    int DeleteUser(int id);
    int SoftDeleteUser(int id);

    // Advanced
    List<User> SearchByType(string searchType, string searchValue);
    List<User> ComplexSearch(...);
    int CountUsers(string? role, bool? isActive);
}
```

## 📝 XML Mapper (UserMapper.xml)

```xml
<mapper namespace="IUserMapper">
  <select id="GetAll" resultType="User">
    SELECT * FROM Users ORDER BY UserName
  </select>

  <select id="GetById" parameterType="int" resultType="User">
    SELECT * FROM Users WHERE Id = @id
  </select>

  <select id="FindByRoles" resultType="User">
    SELECT * FROM Users WHERE Role IN
    <foreach collection="roles" item="role" open="(" separator="," close=")">
      @role
    </foreach>
    ORDER BY Role, UserName
  </select>

  <!-- ... 9 more statements -->
</mapper>
```

## ✅ Tests Included

1. ✅ **GetAll()** - Query without parameters
2. ✅ **GetById(1)** - Query with single parameter
3. ✅ **SearchUsers()** - Dynamic SQL with IF conditions
4. ✅ **FindByRoles()** - ForEach with collection parameter
5. ✅ **Silent Mode** - Query without SQL logging

## 🎓 Learning Points

### Why Mapper Proxy Pattern?

**✅ Advantages:**

- **Type-safe**: Compiler checks at compile-time
- **IntelliSense**: Auto-complete trong IDE
- **Refactoring**: Easy to rename methods
- **Clean code**: `mapper.GetById(1)` vs `session.SelectOne<User>("IUserMapper.GetById", dict)`

**❌ Without Proxy (Old way):**

```csharp
// String literals - error-prone, no IntelliSense
var user = session.SelectOne<User>("IUserMapper.GetById",
    new Dictionary<string, object> { ["id"] = 1 });
```

**✅ With Proxy (MyBatis way):**

```csharp
// Type-safe, clean, maintainable
var mapper = session.GetMapper<IUserMapper>();
var user = mapper.GetById(1);
```

## 🔗 Related Documentation

- [SQL Logging](../SQL_LOGGING.md)
- [Dynamic SQL](../DYNAMIC_SQL_IMPLEMENTATION.md)
- [Main README](../README.md)
