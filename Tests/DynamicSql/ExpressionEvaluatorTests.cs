using Xunit;
using MyBatis.NET.DynamicSql;

namespace MyBatis.NET.Tests.DynamicSql;

public class ExpressionEvaluatorTests
{
    [Fact]
    public void TestNullCheck_NotNull()
    {
        var parameters = new Dictionary<string, object?> { { "name", "John" } };
        var context = new DynamicContext(parameters);

        bool result = ExpressionEvaluator.Evaluate("name != null", context);

        Assert.True(result);
    }

    [Fact]
    public void TestNullCheck_IsNull()
    {
        var parameters = new Dictionary<string, object?> { { "name", null } };
        var context = new DynamicContext(parameters);

        bool result = ExpressionEvaluator.Evaluate("name == null", context);

        Assert.True(result);
    }

    [Fact]
    public void TestNullCheck_ParameterNotExists()
    {
        var parameters = new Dictionary<string, object?>();
        var context = new DynamicContext(parameters);

        bool result = ExpressionEvaluator.Evaluate("name != null", context);

        Assert.False(result);
    }

    [Fact]
    public void TestComparison_GreaterThan()
    {
        var parameters = new Dictionary<string, object?> { { "age", 25 } };
        var context = new DynamicContext(parameters);

        bool result = ExpressionEvaluator.Evaluate("age > 18", context);

        Assert.True(result);
    }

    [Fact]
    public void TestComparison_LessThan()
    {
        var parameters = new Dictionary<string, object?> { { "age", 15 } };
        var context = new DynamicContext(parameters);

        bool result = ExpressionEvaluator.Evaluate("age < 18", context);

        Assert.True(result);
    }

    [Fact]
    public void TestComparison_GreaterOrEqual()
    {
        var parameters = new Dictionary<string, object?> { { "score", 90 } };
        var context = new DynamicContext(parameters);

        bool result1 = ExpressionEvaluator.Evaluate("score >= 90", context);
        bool result2 = ExpressionEvaluator.Evaluate("score >= 89", context);

        Assert.True(result1);
        Assert.True(result2);
    }

    [Fact]
    public void TestComparison_LessOrEqual()
    {
        var parameters = new Dictionary<string, object?> { { "score", 85 } };
        var context = new DynamicContext(parameters);

        bool result1 = ExpressionEvaluator.Evaluate("score <= 85", context);
        bool result2 = ExpressionEvaluator.Evaluate("score <= 90", context);

        Assert.True(result1);
        Assert.True(result2);
    }

    [Fact]
    public void TestEquality_Equal()
    {
        var parameters = new Dictionary<string, object?> { { "type", "admin" } };
        var context = new DynamicContext(parameters);

        bool result = ExpressionEvaluator.Evaluate("type == 'admin'", context);

        Assert.True(result);
    }

    [Fact]
    public void TestEquality_NotEqual()
    {
        var parameters = new Dictionary<string, object?> { { "type", "user" } };
        var context = new DynamicContext(parameters);

        bool result = ExpressionEvaluator.Evaluate("type != 'admin'", context);

        Assert.True(result);
    }

    [Fact]
    public void TestLogicalAnd_BothTrue()
    {
        var parameters = new Dictionary<string, object?>
        {
            { "name", "John" },
            { "age", 25 }
        };
        var context = new DynamicContext(parameters);

        bool result = ExpressionEvaluator.Evaluate("name != null and age > 18", context);

        Assert.True(result);
    }

    [Fact]
    public void TestLogicalAnd_OneFalse()
    {
        var parameters = new Dictionary<string, object?>
        {
            { "name", "John" },
            { "age", 15 }
        };
        var context = new DynamicContext(parameters);

        bool result = ExpressionEvaluator.Evaluate("name != null and age > 18", context);

        Assert.False(result);
    }

    [Fact]
    public void TestLogicalOr_OneTrue()
    {
        var parameters = new Dictionary<string, object?>
        {
            { "name", null },
            { "email", "test@example.com" }
        };
        var context = new DynamicContext(parameters);

        bool result = ExpressionEvaluator.Evaluate("name != null or email != null", context);

        Assert.True(result);
    }

    [Fact]
    public void TestLogicalOr_BothFalse()
    {
        var parameters = new Dictionary<string, object?>
        {
            { "name", null },
            { "email", null }
        };
        var context = new DynamicContext(parameters);

        bool result = ExpressionEvaluator.Evaluate("name != null or email != null", context);

        Assert.False(result);
    }

    [Fact]
    public void TestNegation()
    {
        var parameters = new Dictionary<string, object?> { { "isDeleted", false } };
        var context = new DynamicContext(parameters);

        bool result = ExpressionEvaluator.Evaluate("!isDeleted", context);

        Assert.True(result);
    }

    [Fact]
    public void TestSimpleCheck_Exists()
    {
        var parameters = new Dictionary<string, object?> { { "searchTerm", "test" } };
        var context = new DynamicContext(parameters);

        bool result = ExpressionEvaluator.Evaluate("searchTerm", context);

        Assert.True(result);
    }

    [Fact]
    public void TestSimpleCheck_Empty()
    {
        var parameters = new Dictionary<string, object?> { { "searchTerm", "" } };
        var context = new DynamicContext(parameters);

        bool result = ExpressionEvaluator.Evaluate("searchTerm", context);

        Assert.False(result);
    }

    [Fact]
    public void TestNumericComparison()
    {
        var parameters = new Dictionary<string, object?>
        {
            { "intValue", 100 },
            { "doubleValue", 99.5 }
        };
        var context = new DynamicContext(parameters);

        bool result = ExpressionEvaluator.Evaluate("intValue > doubleValue", context);

        Assert.True(result);
    }

    [Fact]
    public void TestNullCheck_WithNullValue()
    {
        var parameters = new Dictionary<string, object?>
        {
            { "name", "john" },
            { "email", null }
        };
        var context = new DynamicContext(parameters);

        bool nameNotNull = ExpressionEvaluator.Evaluate("name != null", context);
        bool emailNotNull = ExpressionEvaluator.Evaluate("email != null", context);

        Assert.True(nameNotNull, "name should not be null");
        Assert.False(emailNotNull, "email should be null");
    }
}

