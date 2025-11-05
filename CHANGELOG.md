# Changelog

All notable changes to MyBatis.NET will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.0] - 2025-11-05

### 🔥 Breaking Changes

- **REQUIRED `returnSingle` attribute**: All `<select>` statements must now explicitly declare:
  - `returnSingle="true"` for queries returning a single object (→ `T?` nullable)
  - `returnSingle="false"` for queries returning a collection (→ `List<T>`)
- **Rationale**: Ensures clear distinction between single vs collection returns, improves type safety, prevents ambiguity
- **Migration**: Add `returnSingle` attribute to all existing `<select>` statements in your XML mappers

**Migration Example:**

```xml
<!-- Before (v1.x) -->
<select id="GetAll" resultType="User">
  SELECT * FROM Users
</select>

<!-- After (v2.0) -->
<select id="GetAll" resultType="User" returnSingle="false">
  SELECT * FROM Users
</select>

<select id="GetById" resultType="User" returnSingle="true">
  SELECT * FROM Users WHERE Id = @id
</select>
```

### ✨ Added

#### Code Generator Tool

- **Auto-generate C# interfaces from XML mappers** - Eliminates manual sync issues
- **Smart parameter detection** - Extracts parameters from:
  - `@paramName` in SQL statements
  - `<if test="...">` conditions
  - `<foreach collection="...">` tags
- **Type inference** - Intelligent type guessing:
  - `id`, `count`, `age` → `int?`
  - `name`, `email`, `userName` → `string?`
  - `isActive`, `isDeleted` → `bool?`
  - `date`, `createdDate` → `DateTime?`
- **Commands**:
  - `dotnet run generate <xml-file> <namespace>` - Generate single interface
  - `dotnet run generate-all <folder> <namespace>` - Batch generate all XML in folder
  - `dotnet run help` - Show usage
- **Features**:
  - Correct return types based on `returnSingle` attribute
  - Detects collections for `<foreach>`
  - Handles complex parameter objects
  - Automatic timestamp in generated code

#### SQL Logging

- **SqlSessionConfiguration.EnableSqlLogging** - Toggle SQL query logging
- **SqlSessionConfiguration.EnableParameterLogging** - Toggle parameter value logging
- **Formatted console output** with:
  - Timestamp
  - Generated SQL query
  - Parameter names and values
  - Clean separator lines
- **ForEach expansion visibility** - Shows `@param_0`, `@param_1`, etc.
- **Useful for debugging** Dynamic SQL generation

#### Documentation

- **USAGE_GUIDE.md** - Comprehensive usage guide with:
  - Quick start (3 steps)
  - Core features explained
  - Important rules and conventions
  - Code generator documentation
  - Best practices
  - Troubleshooting
  - Tips & tricks
  - Common patterns
- **QUICK_REFERENCE.md** - Cheat sheet with:
  - Setup steps
  - Dynamic SQL syntax
  - returnSingle rules
  - Generator commands
  - Common patterns
- **SQL_LOGGING.md** - SQL logging configuration guide
- **Tools/README.md** - Code generator tool documentation

#### Test Suite

- **Comprehensive test console** with 14 test cases:
  - Simple queries (GetAll, GetById)
  - Dynamic SQL with `<if>` (single/multiple conditions)
  - `<foreach>` with collections (int, string)
  - `<choose>/<when>/<otherwise>` (switch-case)
  - Complex nested dynamic SQL
  - COUNT queries (aggregate functions)
  - CRUD operations (INSERT, UPDATE, DELETE)
  - Soft delete pattern
  - SQL logging toggle
- **Test results reporting** - Pass/fail counts, pass rate percentage
- **Feature verification** - Confirms all features working correctly

### 🚀 Improved

- **Dynamic SQL engine** - Full support for all MyBatis tags
- **Expression evaluator** - OGNL-like conditional expressions
- **Smart SQL building** - Automatic WHERE/SET clause management
- **Parameter mapping** - Enhanced parameter detection and binding
- **Error messages** - Clear error messages for missing `returnSingle` attribute
- **Type safety** - Explicit return type declaration prevents runtime surprises

### 📝 Changed

- **returnSingle attribute enforcement** - Generator throws error if attribute missing
- **Interface generation logic** - Determines return type from `returnSingle` instead of heuristics
- **Documentation language** - All docs now in English for global audience

### 🐛 Fixed

- Type ambiguity in generated interfaces (now explicit via `returnSingle`)
- Parameter detection in complex dynamic SQL
- ForEach collection parameter handling

---

## [1.6.0] - 2024

### Added

- Dynamic SQL Support: `<if>`, `<where>`, `<set>`, `<choose>/<when>/<otherwise>`, `<foreach>`, `<trim>` tags
- Expression Evaluator for conditional SQL
- Smart SQL Building with automatic clause management

---

## [1.5.0] - 2024

### Added

- Full async support for all database operations
- Enhanced documentation with complete CRUD examples

### Changed

- Cleaned package by removing demo files (User.cs, IUserMapper.cs, Program.cs, Demo folder) from compilation

---

## [1.0.0] - Initial Release

### Added

- XML-based SQL mapping
- Runtime proxy generation for mapper interfaces
- Transaction support
- Result mapping to .NET objects
- ADO.NET integration with Microsoft.Data.SqlClient
- Basic CRUD operations
- DDD support with multi-library mapper loading

---

## Migration Guide

### From v1.x to v2.0

#### Step 1: Add `returnSingle` to all `<select>` statements

**For queries returning lists:**

```xml
<select id="GetAll" resultType="User" returnSingle="false">
  SELECT * FROM Users
</select>
```

**For queries returning single objects:**

```xml
<select id="GetById" resultType="User" returnSingle="true">
  SELECT * FROM Users WHERE Id = @id
</select>
```

#### Step 2: Update your interfaces to match

**Use the code generator** (recommended):

```bash
cd Tools
dotnet run generate-all ../YourProject/Mappers YourNamespace
```

**Or update manually**:

```csharp
// returnSingle="false" → List<T>
List<User> GetAll();

// returnSingle="true" → T? (nullable)
User? GetById(int id);
```

#### Step 3: Test

Run your application and ensure all queries work correctly. The new attribute provides better type safety!

---

## Upgrade Benefits

### Why upgrade to v2.0?

✅ **Type Safety** - Explicit return types prevent bugs
✅ **Code Generator** - Never write interfaces manually again
✅ **SQL Logging** - Debug queries easily
✅ **Better Docs** - Comprehensive guides and examples
✅ **Test Coverage** - 14 test cases verify everything works
✅ **No Ambiguity** - Clear single vs list distinction

---

[2.0.0]: https://github.com/hammond01/MyBatis.NET/releases/tag/v2.0.0
[1.6.0]: https://github.com/hammond01/MyBatis.NET/releases/tag/v1.6.0
[1.5.0]: https://github.com/hammond01/MyBatis.NET/releases/tag/v1.5.0
[1.0.0]: https://github.com/hammond01/MyBatis.NET/releases/tag/v1.0.0
