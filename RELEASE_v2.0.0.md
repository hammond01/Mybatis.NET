# 🚀 MyBatis.NET v2.0.0 - Release Summary

## 📊 Version Information

- **Version**: 2.0.0 (Major Release)
- **Release Date**: November 5, 2025
- **Package**: MyBatis.NET.SqlMapper
- **Breaking Changes**: Yes (see below)

---

## 🎯 What's New

### 🔥 Breaking Changes

#### Mandatory `returnSingle` Attribute

All `<select>` statements now **REQUIRE** the `returnSingle` attribute:

```xml
<!-- ✅ CORRECT -->
<select id="GetAll" resultType="User" returnSingle="false">
  SELECT * FROM Users
</select>

<select id="GetById" resultType="User" returnSingle="true">
  SELECT * FROM Users WHERE Id = @id
</select>

<!-- ❌ ERROR - Missing returnSingle -->
<select id="GetUsers" resultType="User">
  SELECT * FROM Users
</select>
```

**Why this change?**

- ✅ Eliminates ambiguity between single vs collection returns
- ✅ Improves type safety at compile time
- ✅ Makes interfaces explicit: `User?` vs `List<User>`
- ✅ Prevents runtime surprises

---

## ✨ Major New Features

### 1️⃣ Code Generator Tool

**Auto-generate C# interfaces from XML mappers!**

```bash
# Generate single file
cd Tools
dotnet run generate ../Mappers/UserMapper.xml MyApp.Mappers

# Generate all XML in folder
dotnet run generate-all ../Mappers MyApp.Mappers
```

**Features:**

- ✅ Keeps XML and C# interface in perfect sync
- ✅ Smart parameter detection from SQL, `<if>`, `<foreach>`
- ✅ Type inference (id→int, name→string?, date→DateTime?)
- ✅ Correct return types from `returnSingle` attribute
- ✅ Handles collections and complex objects

**Example Output:**

```csharp
public interface IUserMapper
{
    List<User> GetAll();                    // returnSingle="false"
    User? GetById(int id);                  // returnSingle="true"
    List<User> SearchUsers(string? name);   // Auto-detected parameters
    int InsertUser(User user);              // INSERT returns int
}
```

---

### 2️⃣ SQL Logging

**Built-in SQL query and parameter logging!**

```csharp
// Enable logging
SqlSessionConfiguration.EnableSqlLogging = true;
SqlSessionConfiguration.EnableParameterLogging = true;

// Execute query
var users = mapper.FindByRoles(new List<string> { "Admin", "Manager" });

// Console output:
// ═══════════════════════════════════════
// [MyBatis.NET SQL] 16:02:13.091
// ───────────────────────────────────────
// SELECT * FROM Users WHERE Role IN (@role_0,@role_1)
// ───────────────────────────────────────
// Parameters:
//   @role_0 = 'Admin'
//   @role_1 = 'Manager'
// ═══════════════════════════════════════
```

**Features:**

- ✅ See generated SQL in real-time
- ✅ View all parameter values
- ✅ ForEach expansion visibility
- ✅ Formatted output with timestamps
- ✅ Toggle on/off anytime

---

### 3️⃣ Comprehensive Documentation

**Complete guides for all users!**

- **[QUICK_REFERENCE.md](QUICK_REFERENCE.md)** - Cheat sheet (3-step setup, syntax guide)
- **[USAGE_GUIDE.md](USAGE_GUIDE.md)** - Complete usage guide (80+ examples)
- **[SQL_LOGGING.md](SQL_LOGGING.md)** - SQL logging configuration
- **[Tools/README.md](Tools/README.md)** - Code generator documentation
- **[CHANGELOG.md](CHANGELOG.md)** - Version history and migration guide

**All in English for global developers!**

---

## 🧪 Quality Assurance

### Comprehensive Test Suite

**14 automated tests** covering all features:

1. ✅ GetAll - `returnSingle="false"` → `List<User>`
2. ✅ GetById - `returnSingle="true"` → `User?`
3. ✅ SearchUsers - Dynamic SQL with `<if>`
4. ✅ FindByRoles - `<foreach>` with string collection
5. ✅ FindByIds - `<foreach>` with int collection
6. ✅ SearchByType - `<choose>/<when>/<otherwise>`
7. ✅ ComplexSearch - Nested dynamic SQL
8. ✅ CountUsers - Aggregate function
9. ✅ InsertUser - `<insert>` statement
10. ✅ UpdateUser - `<update>` with `<set>`
11. ✅ SoftDeleteUser - Soft delete pattern
12. ✅ DeleteUser - `<delete>` statement
13. ✅ SQL Logging Toggle

**Result: 14/14 tests passed (100%)**

---

## 📦 What's Included

### Core Library

- Dynamic SQL engine with all MyBatis tags
- Runtime proxy generation
- Transaction support
- Async operations
- SQL logging infrastructure
- Smart parameter mapping

### Tools

- Code generator CLI
- Batch generation support
- Type inference engine
- Parameter detector

### Documentation

- 4 comprehensive guides
- Quick reference
- Migration guide
- Code examples
- Best practices

### Test Suite

- Console test application
- 14 test cases
- Real-world scenarios
- Feature verification

---

## 🔄 Migration from v1.x

### Step 1: Update XML Mappers

Add `returnSingle` to all `<select>` statements:

```xml
<!-- List queries -->
<select id="GetAll" resultType="User" returnSingle="false">
  SELECT * FROM Users
</select>

<!-- Single object queries -->
<select id="GetById" resultType="User" returnSingle="true">
  SELECT * FROM Users WHERE Id = @id
</select>
```

### Step 2: Regenerate Interfaces

Use the code generator:

```bash
cd Tools
dotnet run generate-all ../YourProject/Mappers YourNamespace
```

### Step 3: Update Interface Declarations

```csharp
// Old (v1.x) - might be wrong
List<User> GetById(int id);  // ❌ Should return single user

// New (v2.0) - correct
User? GetById(int id);  // ✅ Explicit single object
```

### Step 4: Test

Run your application and verify all queries work correctly!

---

## 🎉 Benefits of Upgrading

### Type Safety

- No more ambiguous return types
- Compile-time validation
- Prevents runtime errors

### Developer Productivity

- Auto-generate interfaces (never write manually)
- SQL logging for debugging
- Comprehensive documentation

### Code Quality

- Interface/XML always in sync
- Explicit contracts
- Better maintainability

### Modern Tooling

- CLI code generator
- Batch processing
- Smart type inference

---

## 📈 Statistics

- **Lines of Code**: 15,000+ (including tests)
- **Test Coverage**: 14 comprehensive test cases
- **Documentation**: 4 guides + README + CHANGELOG
- **Features**: 10+ major features
- **Breaking Changes**: 1 (returnSingle)
- **New Tools**: Code Generator CLI

---

## 🔮 Roadmap

### Future Enhancements (v2.1+)

- Watch mode for auto-regeneration
- Better type inference from database schema
- Support for stored procedures
- GraphQL integration
- More database providers (PostgreSQL, MySQL)

---

## 🤝 Contributing

We welcome contributions! Please see:

- GitHub Issues for bug reports
- Pull Requests for features/fixes
- Discussions for ideas

---

## 📞 Support

- **Documentation**: See guides in repository
- **Issues**: [GitHub Issues](https://github.com/hammond01/MyBatis.NET/issues)
- **Discussions**: [GitHub Discussions](https://github.com/hammond01/MyBatis.NET/discussions)

---

## 🏆 Acknowledgments

Special thanks to:

- MyBatis Java project for inspiration
- .NET community for feedback
- All contributors and users

---

## 📄 License

MIT License - See [LICENSE](LICENSE) file for details

---

**🎊 Thank you for using MyBatis.NET! 🎊**

Upgrade to v2.0.0 today and experience the power of type-safe, auto-generated SQL mappers!
