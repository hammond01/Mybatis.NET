using Xunit;
using MyBatis.NET.DynamicSql;

namespace MyBatis.NET.Tests.DynamicSql;

public class SqlNodeTests
{
    [Fact]
    public void TestTextSqlNode()
    {
        var node = new TextSqlNode("SELECT * FROM Users");
        var context = new DynamicContext(new Dictionary<string, object?>());

        node.Apply(context);

        Assert.Equal("SELECT * FROM Users", context.GetSql());
    }

    [Fact]
    public void TestIfSqlNode_ConditionTrue()
    {
        var textNode = new TextSqlNode("AND Age > 18");
        var ifNode = new IfSqlNode(textNode, "age != null");
        var parameters = new Dictionary<string, object?> { { "age", 25 } };
        var context = new DynamicContext(parameters);

        bool applied = ifNode.Apply(context);

        Assert.True(applied);
        Assert.Equal("AND Age > 18", context.GetSql());
    }

    [Fact]
    public void TestIfSqlNode_ConditionFalse()
    {
        var textNode = new TextSqlNode("AND Age > 18");
        var ifNode = new IfSqlNode(textNode, "age != null");
        var parameters = new Dictionary<string, object?>();
        var context = new DynamicContext(parameters);

        bool applied = ifNode.Apply(context);

        Assert.False(applied);
        Assert.Equal("", context.GetSql());
    }

    [Fact]
    public void TestWhereSqlNode_WithContent()
    {
        var nodes = new List<SqlNode>
        {
            new TextSqlNode("AND UserName = @name"),
            new TextSqlNode(" AND Email = @email")
        };
        var mixedNode = new MixedSqlNode(nodes);
        var whereNode = new WhereSqlNode(mixedNode);
        var context = new DynamicContext(new Dictionary<string, object?>());

        bool applied = whereNode.Apply(context);

        Assert.True(applied);
        Assert.Contains("WHERE", context.GetSql());
        Assert.DoesNotContain("AND UserName", context.GetSql()); // Leading AND should be removed
        Assert.Contains("UserName = @name", context.GetSql());
    }

    [Fact]
    public void TestWhereSqlNode_NoContent()
    {
        var ifNode = new IfSqlNode(new TextSqlNode("AND Age > 18"), "age != null");
        var whereNode = new WhereSqlNode(ifNode);
        var parameters = new Dictionary<string, object?>(); // age is null
        var context = new DynamicContext(parameters);

        bool applied = whereNode.Apply(context);

        Assert.False(applied);
        Assert.Equal("", context.GetSql());
    }

    [Fact]
    public void TestSetSqlNode_WithContent()
    {
        var nodes = new List<SqlNode>
        {
            new TextSqlNode("UserName = @name,"),
            new TextSqlNode(" Email = @email,")
        };
        var mixedNode = new MixedSqlNode(nodes);
        var setNode = new SetSqlNode(mixedNode);
        var context = new DynamicContext(new Dictionary<string, object?>());

        bool applied = setNode.Apply(context);

        Assert.True(applied);
        Assert.Contains("SET", context.GetSql());
        Assert.DoesNotContain("Email = @email,", context.GetSql()); // Trailing comma should be removed
        Assert.Contains("Email = @email", context.GetSql());
    }

    [Fact]
    public void TestChooseSqlNode_FirstWhenMatches()
    {
        var when1 = new IfSqlNode(new TextSqlNode("Role = 'Admin'"), "type == 'admin'");
        var when2 = new IfSqlNode(new TextSqlNode("Role = 'User'"), "type == 'user'");
        var otherwise = new TextSqlNode("Role = 'Guest'");
        var chooseNode = new ChooseSqlNode(new List<IfSqlNode> { when1, when2 }, otherwise);
        var parameters = new Dictionary<string, object?> { { "type", "admin" } };
        var context = new DynamicContext(parameters);

        bool applied = chooseNode.Apply(context);

        Assert.True(applied);
        Assert.Equal("Role = 'Admin'", context.GetSql());
    }

    [Fact]
    public void TestChooseSqlNode_SecondWhenMatches()
    {
        var when1 = new IfSqlNode(new TextSqlNode("Role = 'Admin'"), "type == 'admin'");
        var when2 = new IfSqlNode(new TextSqlNode("Role = 'User'"), "type == 'user'");
        var otherwise = new TextSqlNode("Role = 'Guest'");
        var chooseNode = new ChooseSqlNode(new List<IfSqlNode> { when1, when2 }, otherwise);
        var parameters = new Dictionary<string, object?> { { "type", "user" } };
        var context = new DynamicContext(parameters);

        bool applied = chooseNode.Apply(context);

        Assert.True(applied);
        Assert.Equal("Role = 'User'", context.GetSql());
    }

    [Fact]
    public void TestChooseSqlNode_OtherwiseMatches()
    {
        var when1 = new IfSqlNode(new TextSqlNode("Role = 'Admin'"), "type == 'admin'");
        var when2 = new IfSqlNode(new TextSqlNode("Role = 'User'"), "type == 'user'");
        var otherwise = new TextSqlNode("Role = 'Guest'");
        var chooseNode = new ChooseSqlNode(new List<IfSqlNode> { when1, when2 }, otherwise);
        var parameters = new Dictionary<string, object?> { { "type", "unknown" } };
        var context = new DynamicContext(parameters);

        bool applied = chooseNode.Apply(context);

        Assert.True(applied);
        Assert.Equal("Role = 'Guest'", context.GetSql());
    }

    [Fact]
    public void TestForEachSqlNode_IntArray()
    {
        var textNode = new TextSqlNode("@id");
        var foreachNode = new ForEachSqlNode(textNode, "ids", "id", "index", ",", "(", ")");
        var parameters = new Dictionary<string, object?>
        {
            { "ids", new[] { 1, 2, 3, 4, 5 } }
        };
        var context = new DynamicContext(parameters);

        bool applied = foreachNode.Apply(context);

        Assert.True(applied);
        var sql = context.GetSql();
        Assert.StartsWith("(", sql);
        Assert.EndsWith(")", sql);
        Assert.Contains(",", sql);
    }

    [Fact]
    public void TestForEachSqlNode_EmptyCollection()
    {
        var textNode = new TextSqlNode("@id");
        var foreachNode = new ForEachSqlNode(textNode, "ids", "id", "index", ",", "(", ")");
        var parameters = new Dictionary<string, object?>
        {
            { "ids", Array.Empty<int>() }
        };
        var context = new DynamicContext(parameters);

        bool applied = foreachNode.Apply(context);

        Assert.False(applied);
    }

    [Fact]
    public void TestTrimSqlNode_RemovePrefixOverride()
    {
        var textNode = new TextSqlNode("AND UserName = @name");
        var trimNode = new TrimSqlNode(textNode, "WHERE", "", "AND |OR ", "");
        var context = new DynamicContext(new Dictionary<string, object?>());

        bool applied = trimNode.Apply(context);

        Assert.True(applied);
        var sql = context.GetSql();
        Assert.StartsWith("WHERE", sql);
        Assert.DoesNotContain("AND UserName", sql);
        Assert.Contains("UserName = @name", sql);
    }

    [Fact]
    public void TestTrimSqlNode_RemoveSuffixOverride()
    {
        var textNode = new TextSqlNode("UserName = @name,");
        var trimNode = new TrimSqlNode(textNode, "SET", "", "", ",");
        var context = new DynamicContext(new Dictionary<string, object?>());

        bool applied = trimNode.Apply(context);

        Assert.True(applied);
        var sql = context.GetSql();
        Assert.StartsWith("SET", sql);
        Assert.DoesNotContain("@name,", sql);
        Assert.Contains("UserName = @name", sql);
    }

    [Fact]
    public void TestMixedSqlNode()
    {
        var nodes = new List<SqlNode>
        {
            new TextSqlNode("SELECT * FROM Users"),
            new IfSqlNode(new TextSqlNode(" WHERE Age > 18"), "includeAge == true")
        };
        var mixedNode = new MixedSqlNode(nodes);
        var parameters = new Dictionary<string, object?> { { "includeAge", true } };
        var context = new DynamicContext(parameters);

        bool applied = mixedNode.Apply(context);

        Assert.True(applied);
        Assert.Equal("SELECT * FROM Users WHERE Age > 18", context.GetSql());
    }
}
