# Contributing to MyBatis.NET

First off, thank you for considering contributing to MyBatis.NET! 🎉

It's people like you that make MyBatis.NET such a great tool for the .NET community.

## 📋 Table of Contents

- [Code of Conduct](#code-of-conduct)
- [How Can I Contribute?](#how-can-i-contribute)
- [Development Setup](#development-setup)
- [Pull Request Process](#pull-request-process)
- [Coding Guidelines](#coding-guidelines)
- [Testing Guidelines](#testing-guidelines)
- [Documentation Guidelines](#documentation-guidelines)

## 📜 Code of Conduct

This project and everyone participating in it is governed by our [Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code.

## 🤝 How Can I Contribute?

### Reporting Bugs

Before creating bug reports, please check the [existing issues](https://github.com/hammond01/MyBatis.NET/issues) to avoid duplicates.

When creating a bug report, include:

- **Clear title and description**
- **Steps to reproduce** the issue
- **Expected behavior** vs **actual behavior**
- **Code samples** (XML mapper + C# code)
- **.NET version** and **database** you're using
- **Stack trace** if applicable

**Example:**

```markdown
**Bug**: Dynamic SQL `<if>` tag not evaluating null check correctly

**Environment:**
- MyBatis.NET version: 2.0.0
- .NET version: 8.0
- Database: SQL Server 2022

**Steps to Reproduce:**
1. Create mapper with `<if test="name != null">`
2. Call method with `name = null`
3. SQL still includes the condition

**Expected:** Condition should be skipped
**Actual:** Condition is included in SQL

**Code Sample:**
[XML and C# code here]
```

### Suggesting Enhancements

Enhancement suggestions are tracked as GitHub issues. When creating an enhancement suggestion:

- **Use a clear and descriptive title**
- **Provide detailed description** of the suggested enhancement
- **Explain why this enhancement would be useful**
- **Provide code examples** if applicable

### Your First Code Contribution

Unsure where to begin? Look for issues labeled:

- `good first issue` - Simple issues for beginners
- `help wanted` - Issues that need community help
- `documentation` - Documentation improvements

## 🛠️ Development Setup

### Prerequisites

- .NET 8.0 SDK or later
- SQL Server (for running tests)
- Git
- Your favorite IDE (Visual Studio, Rider, VS Code)

### Setup Steps

1. **Fork the repository**

   ```bash
   # Click "Fork" on GitHub, then clone your fork
   git clone https://github.com/YOUR_USERNAME/MyBatis.NET.git
   cd MyBatis.NET
   ```

2. **Create a branch**

   ```bash
   git checkout -b feature/your-feature-name
   # or
   git checkout -b fix/your-bug-fix
   ```

3. **Restore dependencies**

   ```bash
   dotnet restore
   ```

4. **Build the project**

   ```bash
   dotnet build
   ```

5. **Setup test database**

   ```bash
   # Run the test database script
   sqlcmd -S localhost -i Tests/TestDatabase.sql
   ```

6. **Run tests**

   ```bash
   dotnet test
   ```

### Project Structure

```
MyBatis.NET/
├── Core/                    # Core SQL execution engine
├── Mapper/                  # XML mapper parsing
├── DynamicSql/             # Dynamic SQL engine
├── Tools/                   # Code generator
├── Tests/                   # Unit & integration tests
├── MyBatis.ConsoleTest/    # Demo application
└── docs/                    # Documentation
```

## 🔄 Pull Request Process

### Before Submitting

1. **Ensure all tests pass**

   ```bash
   dotnet test
   ```

2. **Add tests for new features**
   - Unit tests in `Tests/`
   - Integration tests if applicable

3. **Update documentation**
   - Update README.md if needed
   - Add XML doc comments to public APIs
   - Update CHANGELOG.md

4. **Follow coding guidelines** (see below)

5. **Run code formatter**

   ```bash
   dotnet format
   ```

### Submitting the PR

1. **Push to your fork**

   ```bash
   git push origin feature/your-feature-name
   ```

2. **Create Pull Request** on GitHub

3. **Fill in the PR template** with:
   - Description of changes
   - Related issue number (if any)
   - Type of change (bug fix, feature, docs, etc.)
   - Checklist completion

4. **Wait for review**
   - Address review comments
   - Keep PR updated with main branch

### PR Title Format

Use conventional commits format:

```
feat: Add PostgreSQL support
fix: Resolve null reference in DynamicContext
docs: Update USAGE_GUIDE with new examples
test: Add integration tests for <foreach> tag
refactor: Simplify ExpressionEvaluator logic
perf: Optimize SqlNode tree traversal
```

## 📝 Coding Guidelines

### General Principles

- **KISS** - Keep It Simple, Stupid
- **DRY** - Don't Repeat Yourself
- **SOLID** principles
- **Clean Code** - Self-documenting code

### C# Style Guide

1. **Naming Conventions**

   ```csharp
   // PascalCase for classes, methods, properties
   public class SqlSession { }
   public void ExecuteQuery() { }
   public string ConnectionString { get; set; }

   // camelCase for parameters, local variables
   public void Process(string userName, int userId)
   {
       var resultSet = GetData();
   }

   // _camelCase for private fields
   private readonly IDbConnection _conn;

   // UPPER_CASE for constants
   private const string DEFAULT_NAMESPACE = "MyBatis.NET";
   ```

2. **Code Organization**

   ```csharp
   // Order: fields, constructors, properties, methods
   public class Example
   {
       // 1. Private fields
       private readonly string _field;

       // 2. Constructor
       public Example(string field)
       {
           _field = field;
       }

       // 3. Properties
       public string Property { get; set; }

       // 4. Public methods
       public void PublicMethod() { }

       // 5. Private methods
       private void PrivateMethod() { }
   }
   ```

3. **XML Documentation**

   ```csharp
   /// <summary>
   /// Executes a SQL statement and returns the number of affected rows.
   /// </summary>
   /// <param name="id">The statement ID from XML mapper</param>
   /// <param name="parameters">Optional parameters for the statement</param>
   /// <returns>Number of rows affected</returns>
   /// <exception cref="MyBatisException">Thrown when execution fails</exception>
   public int Execute(string id, Dictionary<string, object>? parameters = null)
   {
       // Implementation
   }
   ```

4. **Error Handling**

   ```csharp
   // Use specific exceptions
   throw new MyBatisException($"Statement '{id}' not found in registry");

   // Wrap external exceptions with context
   catch (SqlException ex)
   {
       throw new MyBatisException($"Error executing '{id}': {ex.Message}", ex);
   }
   ```

5. **Async/Await**

   ```csharp
   // Always use async suffix
   public async Task<List<T>> SelectListAsync<T>(string id)
   {
       // Use ConfigureAwait(false) in library code
       await using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
       return ResultMapper.MapToList<T>(reader);
   }
   ```

### XML Mapper Guidelines

```xml
<!-- Use clear, descriptive IDs -->
<select id="GetUserById" resultType="User" returnSingle="true">
  SELECT * FROM Users WHERE Id = @id
</select>

<!-- Format SQL for readability -->
<select id="SearchUsers" resultType="User" returnSingle="false">
  SELECT 
    Id, 
    UserName, 
    Email, 
    Role
  FROM Users
  <where>
    <if test="name != null">
      AND UserName LIKE '%' + @name + '%'
    </if>
    <if test="role != null">
      AND Role = @role
    </if>
  </where>
  ORDER BY UserName
</select>
```

## 🧪 Testing Guidelines

### Test Structure

```csharp
[Fact]
public void MethodName_Scenario_ExpectedBehavior()
{
    // Arrange
    var mapper = session.GetMapper<IUserMapper>();
    var testData = new User { UserName = "test" };

    // Act
    var result = mapper.InsertUser(testData);

    // Assert
    Assert.Equal(1, result);
}
```

### Test Coverage Requirements

- **New features**: 80%+ coverage
- **Bug fixes**: Add test that reproduces the bug
- **Refactoring**: Maintain existing coverage

### Integration Tests

```csharp
// Use IDisposable for cleanup
public class IntegrationTests : IDisposable
{
    private readonly SqlSession _session;

    public IntegrationTests()
    {
        _session = new SqlSession(ConnectionString);
    }

    [Fact]
    public void Test_RealDatabase_Scenario()
    {
        // Test with real database
    }

    public void Dispose()
    {
        _session?.Dispose();
    }
}
```

## 📚 Documentation Guidelines

### README Updates

- Keep examples simple and runnable
- Update version numbers
- Add new features to feature list

### Code Comments

```csharp
// Good: Explain WHY, not WHAT
// Use case-insensitive comparison for SQL Server compatibility
var equals = string.Equals(left, right, StringComparison.OrdinalIgnoreCase);

// Bad: Redundant comment
// Compare strings
var equals = string.Equals(left, right);
```

### Markdown Files

- Use clear headings
- Include code examples
- Add table of contents for long docs
- Use emojis sparingly for visual clarity 😊

## 🎯 Commit Message Guidelines

Follow [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation only
- `test`: Adding tests
- `refactor`: Code refactoring
- `perf`: Performance improvement
- `chore`: Maintenance tasks

**Examples:**

```
feat(dynamic-sql): Add support for <bind> tag

Implement <bind> tag for variable assignment in dynamic SQL,
similar to MyBatis Java implementation.

Closes #123
```

```
fix(expression): Handle null values in comparison operators

Previously, comparing null values would throw NullReferenceException.
Now properly handles null == null and null != value cases.

Fixes #456
```

## 🏆 Recognition

Contributors will be:

- Listed in CONTRIBUTORS.md
- Mentioned in release notes
- Credited in documentation (for major contributions)

## ❓ Questions?

- **General questions**: [GitHub Discussions](https://github.com/hammond01/MyBatis.NET/discussions)
- **Bug reports**: [GitHub Issues](https://github.com/hammond01/MyBatis.NET/issues)
- **Security issues**: See [SECURITY.md](SECURITY.md)

## 📄 License

By contributing, you agree that your contributions will be licensed under the MIT License.

---

**Thank you for contributing to MyBatis.NET!** 🚀

Your efforts help make data access in .NET better for everyone.
