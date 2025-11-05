using MyBatis.NET.Core;
using MyBatis.NET.Mapper;
using Xunit.Abstractions;

namespace MyBatis.NET.Tests.Examples;

/// <summary>
/// Tests demonstrating SQL logging configuration
/// </summary>
public class SqlLoggingTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly SqlSession _session;

    public SqlLoggingTests(ITestOutputHelper output)
    {
        _output = output;

        // Load mappers
        MapperAutoLoader.AutoLoad("Mappers");

        var connectionString = "Server=.;Database=MyBatisTestDB;Integrated Security=true;TrustServerCertificate=true";
        _session = new SqlSession(connectionString);
    }

    [Fact]
    public void Test_SqlLogging_Disabled()
    {
        // Arrange
        SqlSessionConfiguration.EnableSqlLogging = false;
        SqlSessionConfiguration.EnableParameterLogging = false;

        _output.WriteLine("=== SQL Logging: DISABLED ===");
        _output.WriteLine("No SQL output will be shown in console");
        _output.WriteLine("");

        // Act
        var users = _session.SelectList<User>("IUserMapper.GetAll", null);

        // Assert
        Assert.NotEmpty(users);
        _output.WriteLine($"Found {users.Count} users (no SQL logged)");
    }

    [Fact]
    public void Test_SqlLogging_OnlySQL()
    {
        // Arrange
        SqlSessionConfiguration.EnableSqlLogging = true;
        SqlSessionConfiguration.EnableParameterLogging = false;

        _output.WriteLine("=== SQL Logging: ENABLED (SQL only) ===");
        _output.WriteLine("SQL will be shown but not parameters");
        _output.WriteLine("");

        // Act
        var user = _session.SelectOne<User>("IUserMapper.GetById",
            new Dictionary<string, object> { ["id"] = 1 });

        // Assert
        Assert.NotNull(user);
        _output.WriteLine($"Found user: {user.UserName}");
    }

    [Fact]
    public void Test_SqlLogging_WithParameters()
    {
        // Arrange
        SqlSessionConfiguration.EnableSqlLogging = true;
        SqlSessionConfiguration.EnableParameterLogging = true;

        _output.WriteLine("=== SQL Logging: ENABLED (SQL + Parameters) ===");
        _output.WriteLine("Both SQL and parameters will be shown");
        _output.WriteLine("");

        // Act
        var users = _session.SelectList<User>("IUserMapper.FindByRoles",
            new Dictionary<string, object>
            {
                ["roles"] = new List<string> { "Admin", "Manager" }
            });

        // Assert
        Assert.NotEmpty(users);
        _output.WriteLine($"Found {users.Count} users with specified roles");
    }

    [Fact]
    public void Test_SqlLogging_DynamicSQL()
    {
        // Arrange
        SqlSessionConfiguration.EnableSqlLogging = true;
        SqlSessionConfiguration.EnableParameterLogging = true;

        _output.WriteLine("=== SQL Logging: Dynamic SQL Example ===");
        _output.WriteLine("Shows how dynamic SQL is generated");
        _output.WriteLine("");

        // Act
        var users = _session.SelectList<User>("IUserMapper.SearchUsers",
            new Dictionary<string, object>
            {
                ["userName"] = "admin",
                ["role"] = "Admin"
            });

        // Assert
        Assert.NotEmpty(users);
        _output.WriteLine($"Found {users.Count} users matching criteria");
    }

    public void Dispose()
    {
        _session?.Dispose();

        // Reset configuration after tests
        SqlSessionConfiguration.EnableSqlLogging = false;
        SqlSessionConfiguration.EnableParameterLogging = false;
    }
}

public class User
{
    public int Id { get; set; }
    public string UserName { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Role { get; set; }
    public DateTime? CreatedAt { get; set; }
}
