# MyBatis.NET Tests

Comprehensive test suite for MyBatis.NET Dynamic SQL features.

## Test Structure

```
Tests/
├── DynamicSql/                      # Unit tests for Dynamic SQL components
│   ├── ExpressionEvaluatorTests.cs  # Expression parser tests
│   └── SqlNodeTests.cs               # SqlNode implementations tests
├── Integration/                      # Integration tests
│   └── DynamicSqlIntegrationTests.cs # Real-world usage scenarios
├── Mappers/                          # Test XML mappers
│   ├── IUserMapper.cs                # Mapper interface
│   └── UserMapper.xml                # XML mapper with all dynamic SQL features
├── Models/                           # Test models
│   └── User.cs                       # User entity and DTOs
└── TestDatabase.sql                  # Database setup script
```

## Prerequisites

### 1. Database Setup

Run the database setup script to create test database:

```sql
-- Using SQL Server Management Studio or sqlcmd
sqlcmd -S localhost -i TestDatabase.sql
```

Or execute manually:

- Open SQL Server Management Studio
- Connect to `localhost`
- Open and run `TestDatabase.sql`

This creates:

- Database: `MyBatisTestDB`
- Table: `Users` with 10 test records
- Sample data with various roles, ages, and statuses

### 2. Connection String

Update connection string in `DynamicSqlIntegrationTests.cs` if needed:

```csharp
private const string ConnectionString = "Server=localhost;Database=MyBatisTestDB;Integrated Security=true;TrustServerCertificate=true;";
```

## Running Tests

### Run all tests:

```bash
cd Tests
dotnet test
```

### Run specific test class:

```bash
dotnet test --filter "FullyQualifiedName~ExpressionEvaluatorTests"
dotnet test --filter "FullyQualifiedName~SqlNodeTests"
dotnet test --filter "FullyQualifiedName~DynamicSqlIntegrationTests"
```

### Run with detailed output:

```bash
dotnet test --logger "console;verbosity=detailed"
```

## Test Coverage

### Unit Tests (DynamicSql/)

#### ExpressionEvaluatorTests (20 tests)

- ✅ Null checks (`name != null`, `value == null`)
- ✅ Comparisons (`>`, `<`, `>=`, `<=`, `==`, `!=`)
- ✅ Logical operators (`and`, `or`)
- ✅ Negation (`!condition`)
- ✅ Simple existence checks
- ✅ Numeric comparisons

#### SqlNodeTests (15 tests)

- ✅ TextSqlNode - Static SQL
- ✅ IfSqlNode - Conditional inclusion
- ✅ WhereSqlNode - Smart WHERE clause
- ✅ SetSqlNode - Smart SET for UPDATE
- ✅ ChooseSqlNode - Switch/case logic
- ✅ ForEachSqlNode - Collection loops
- ✅ TrimSqlNode - Custom prefix/suffix
- ✅ MixedSqlNode - Combined nodes

### Integration Tests (25 tests)

#### Basic CRUD

- ✅ Test01: GetAll - Fetch all users
- ✅ Test25: Insert and Delete operations

#### IF Conditions

- ✅ Test02-04: FindByNameOrEmail (single, both, none)
- ✅ Test05-07: FindByAgeRange (min, max, both)
- ✅ Test08: FindByMultipleConditions
- ✅ Test09-10: SearchUsers with filters

#### CHOOSE/WHEN/OTHERWISE

- ✅ Test11-13: FindByRole (admin, manager, unknown)
- ✅ Test14-15: FindByStatus (active, deleted)

#### SET for UPDATE

- ✅ Test16: UpdateUser - Single field
- ✅ Test17: UpdateUser - Multiple fields
- ✅ Test18: UpdateUserSelective - Using DTO

#### FOREACH

- ✅ Test19: FindByIds - Integer collection
- ✅ Test20: FindByRoles - String collection

#### Complex Nested

- ✅ Test21: ComplexSearch - All filters
- ✅ Test22: ComplexSearch - Search type
- ✅ Test23: ComplexSearch - No filters

#### TRIM

- ✅ Test24: SearchWithTrim - Prefix overrides

## XML Mapper Features Tested

### 1. Simple IF Conditions

```xml
<if test="name != null">
  AND UserName = @name
</if>
```

### 2. WHERE Clause

```xml
<where>
  <if test="name != null">...</if>
  <if test="email != null">...</if>
</where>
```

### 3. CHOOSE/WHEN/OTHERWISE

```xml
<choose>
  <when test="type == 'admin'">...</when>
  <when test="type == 'user'">...</when>
  <otherwise>...</otherwise>
</choose>
```

### 4. SET for UPDATE

```xml
<set>
  <if test="userName != null">UserName = @userName,</if>
  <if test="email != null">Email = @email,</if>
</set>
```

### 5. FOREACH

```xml
<foreach collection="ids" item="id" separator="," open="(" close=")">
  @id
</foreach>
```

### 6. Complex Nested

Combines multiple dynamic tags in one query with nested conditions.

### 7. TRIM

```xml
<trim prefix="WHERE" prefixOverrides="AND |OR ">
  <if test="name != null">AND UserName = @name</if>
</trim>
```

## Expected Results

### All Unit Tests: ✅ Pass

- ExpressionEvaluatorTests: 20/20 passed
- SqlNodeTests: 15/15 passed

### All Integration Tests: ✅ Pass (with database)

- DynamicSqlIntegrationTests: 25/25 passed

### Total: 60 tests, all passing

## Troubleshooting

### Database Connection Issues

```
Error: Cannot open database "MyBatisTestDB"
```

**Solution**: Run `TestDatabase.sql` first

### Tests Skip Integration Tests

```
Reason: Database not available
```

**Solution**: Ensure SQL Server is running on localhost

### Authentication Failed

```
Error: Login failed for user
```

**Solution**: Update connection string to use SQL Server authentication:

```csharp
"Server=localhost;Database=MyBatisTestDB;User Id=sa;Password=yourpassword;"
```

## Manual Testing

You can also test manually using the example mappers:

```csharp
using MyBatis.NET.Core;
using MyBatis.NET.Mapper;

// Load mappers
MapperAutoLoader.AutoLoad("Mappers");

// Create session
using var session = new SqlSession(connectionString);
var mapper = session.GetMapper<IUserMapper>();

// Test dynamic SQL
var users = mapper.FindByNameOrEmail(name: "john", email: null);
foreach (var user in users)
{
    Console.WriteLine(user);
}
```

## Contributing

When adding new Dynamic SQL features:

1. Add unit tests in `DynamicSql/`
2. Add XML examples in `Mappers/UserMapper.xml`
3. Add integration tests in `Integration/`
4. Update this README

---

**Test Status**: ✅ All tests passing  
**Coverage**: Expression evaluation, SQL node operations, Real-world scenarios  
**Last Updated**: November 5, 2025
