using Xunit;
using MyBatis.NET.Mapper;
using MyBatis.NET.DynamicSql;

namespace MyBatis.NET.Tests.Mapper;

public class SqlStatementTests
{
    [Fact]
    public void BuildSql_StaticSql_ReturnsOriginalSql()
    {
        // Arrange
        var sql = "SELECT * FROM Users";
        var stmt = new SqlStatement
        {
            Id = "GetAll",
            Sql = sql
        };

        // Act
        var result = stmt.BuildSql(null);

        // Assert
        Assert.Equal(sql, result);
        Assert.False(stmt.IsDynamic);
    }

    [Fact]
    public void BuildSql_DynamicSql_ReturnsProcessedSql()
    {
        // Arrange
        var stmt = new SqlStatement
        {
            Id = "GetUsers",
            RootNode = new MixedSqlNode(new List<SqlNode>
            {
                new TextSqlNode("SELECT * FROM Users"),
                new IfSqlNode(new MixedSqlNode(new List<SqlNode>
                {
                    new TextSqlNode(" WHERE Name = @name")
                }), "name != null")
            })
        };

        var parameters = new Dictionary<string, object?> { { "name", "John" } };

        // Act
        var result = stmt.BuildSql(parameters);

        // Assert
        Assert.Equal("SELECT * FROM Users WHERE Name = @name", result);
        Assert.True(stmt.IsDynamic);
    }

    [Fact]
    public void BuildSql_DynamicSql_WithOutParameters_ReturnsModifiedParameters()
    {
        // Arrange
        var stmt = new SqlStatement
        {
            Id = "GetUsers",
            RootNode = new MixedSqlNode(new List<SqlNode>
            {
                new TextSqlNode("SELECT * FROM Users")
            })
        };

        var parameters = new Dictionary<string, object?> { { "name", "John" } };

        // Act
        var result = stmt.BuildSql(parameters, out var modifiedParams);

        // Assert
        Assert.Equal("SELECT * FROM Users", result);
        Assert.Equal(parameters["name"], modifiedParams["name"]);
    }
}
