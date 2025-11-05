# Quick Test Guide - MyBatis.NET Dynamic SQL

## ✅ Build Status: SUCCESS

All components built successfully:

- ✅ MyBatis.NET (Main library with Dynamic SQL)
- ✅ MyBatis.NET.Tests (Complete test suite)

## 📦 What We've Built

### Dynamic SQL Implementation (Version 1.6.0)

- ✅ Expression Evaluator (OGNL-like)
- ✅ All Dynamic Tags (`<if>`, `<where>`, `<set>`, `<choose>`, `<foreach>`, `<trim>`)
- ✅ XML Parser with Dynamic SQL support
- ✅ Runtime SQL building

### Complete Test Suite

- **60 Total Tests**
  - 20 Expression Evaluator unit tests
  - 15 SqlNode unit tests
  - 25 Integration tests (real user scenarios)

## 🚀 Running Tests

### 1. Unit Tests Only (No database needed)

```bash
cd Tests
dotnet test --filter "FullyQualifiedName~ExpressionEvaluatorTests|FullyQualifiedName~SqlNodeTests"
```

### 2. All Tests (Requires database)

**Step 1: Setup database**

```bash
# Run TestDatabase.sql in SQL Server Management Studio
sqlcmd -S localhost -i TestDatabase.sql
```

**Step 2: Run all tests**

```bash
cd Tests
dotnet test
```

### 3. Specific Test Categories

**Expression Evaluator:**

```bash
dotnet test --filter "FullyQualifiedName~ExpressionEvaluatorTests"
```

**SQL Nodes:**

```bash
dotnet test --filter "FullyQualifiedName~SqlNodeTests"
```

**Integration (Full scenarios):**

```bash
dotnet test --filter "FullyQualifiedName~DynamicSqlIntegrationTests"
```

### 4. Verbose Output

```bash
dotnet test --logger "console;verbosity=detailed"
```

## 📁 Test Files Created

```
Tests/
├── README.md                             # Detailed test documentation
├── TestDatabase.sql                      # Database setup script
├──  Models/User.cs                        # Test entities
├── Mappers/
│   ├── IUserMapper.cs                    # Mapper interface
│   └── UserMapper.xml                    # ⭐ Complete Dynamic SQL examples
├── DynamicSql/
│   ├── ExpressionEvaluatorTests.cs       # 20 expression tests
│   └── SqlNodeTests.cs                   # 15 SQL node tests
└── Integration/
    └── DynamicSqlIntegrationTests.cs     # 25 real-world scenarios
```

## 🎯 UserMapper.xml - Test Coverage

The `UserMapper.xml` demonstrates ALL Dynamic SQL features:

1. **Basic IF** - Simple conditional SQL
2. **WHERE clause** - Smart WHERE with auto-cleanup
3. **Multiple conditions** - Combined filters
4. **CHOOSE/WHEN/OTHERWISE** - Switch-case logic
5. **SET** - Dynamic UPDATE statements
6. **FOREACH** - IN clauses with collections
7. **TRIM** - Custom prefix/suffix handling
8. **Complex nested** - Real-world complex queries

## 💡 Quick Manual Test

```csharp
using MyBatis.NET.Core;
using MyBatis.NET.Mapper;

// Load the test mapper
MapperAutoLoader.AutoLoad("Mappers");

// Create session (update connection string)
var connStr = "Server=localhost;Database=MyBatisTestDB;Integrated Security=true;TrustServerCertificate=true;";
using var session = new SqlSession(connStr);

// Get mapper
var mapper = session.GetMapper<IUserMapper>();

// Test dynamic SQL - only filters with values are added!
var users = mapper.FindByNameOrEmail(name: "john", email: null);
// Generated SQL: SELECT * FROM Users WHERE UserName LIKE '%john%'

var users2 = mapper.FindByNameOrEmail(name: null, email: "gmail");
// Generated SQL: SELECT * FROM Users WHERE Email LIKE '%gmail%'

var users3 = mapper.FindByNameOrEmail(name: null, email: null);
// Generated SQL: SELECT * FROM Users (no WHERE clause!)

Console.WriteLine($"Dynamic SQL works! Found {users.Count} users");
```

## 🐛 Troubleshooting

### "Cannot open database MyBatisTestDB"

**Solution:** Run `TestDatabase.sql` first to create the database

### "No tests found"

**Solution:** Make sure you're in the Tests directory:

```bash
cd f:\LIB\MyBatis.NET\Tests
```

### Connection issues

**Solution:** Update connection string in `DynamicSqlIntegrationTests.cs` (line 16):

```csharp
private const string ConnectionString = "YOUR_CONNECTION_STRING_HERE";
```

### Integration tests skipped

This is normal if database is not available. Unit tests will still run.

## 📊 Expected Results

### Unit Tests (Always pass, no DB needed)

```
✅ ExpressionEvaluatorTests: 20/20 passed
✅ SqlNodeTests: 15/15 passed
```

### Integration Tests (Requires database)

```
✅ DynamicSqlIntegrationTests: 25/25 passed

Test scenarios include:
- Basic CRUD operations
- Dynamic filtering (IF conditions)
- Smart WHERE clauses
- Switch-case logic (CHOOSE)
- Dynamic UPDATEs (SET)
- Collection queries (FOREACH)
- Complex nested conditions
```

## 🎓 Learning from Tests

The tests are designed to teach you how to use Dynamic SQL:

1. **Read `UserMapper.xml`** - See all dynamic tags in action
2. **Read integration tests** - See how to call from C#
3. **Run tests** - Verify everything works
4. **Modify tests** - Experiment with your own queries

## 🚀 Next Steps

1. ✅ Run unit tests (no setup needed)
2. ✅ Read through `UserMapper.xml` to see all features
3. ✅ Setup database with `TestDatabase.sql`
4. ✅ Run integration tests
5. ✅ Try modifying XML and see SQL change dynamically!

---

**Status**: ✅ **ALL BUILT SUCCESSFULLY**  
**Ready to test**: Run `cd Tests; dotnet test`  
**Date**: November 5, 2025
