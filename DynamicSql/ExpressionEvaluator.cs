using System.Text.RegularExpressions;

namespace MyBatis.NET.DynamicSql;

/// <summary>
/// Simple OGNL-like expression evaluator for MyBatis conditions
/// Supports: ==, !=, >, <, >=, <=, null checks, and, or
/// </summary>
public static class ExpressionEvaluator
{
    public static bool Evaluate(string expression, DynamicContext context)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return true;

        expression = expression.Trim();

        // Handle 'and' / '||' operators
        if (expression.Contains(" and ", StringComparison.OrdinalIgnoreCase) || expression.Contains("&&"))
        {
            var separator = expression.Contains("&&") ? "&&" : " and ";
            var parts = expression.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);
            return parts.All(p => Evaluate(p.Trim(), context));
        }

        // Handle 'or' / '||' operators
        if (expression.Contains(" or ", StringComparison.OrdinalIgnoreCase) || expression.Contains("||"))
        {
            var separator = expression.Contains("||") ? "||" : " or ";
            var parts = expression.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);
            return parts.Any(p => Evaluate(p.Trim(), context));
        }

        // Handle negation (but not !=)
        if (expression.StartsWith("!") && !expression.StartsWith("!="))
        {
            return !Evaluate(expression.Substring(1).Trim(), context);
        }

        // Handle null checks: "name != null" or "name == null"
        if (expression.Contains(" != null") || expression.Contains(" == null"))
        {
            var isNotNull = expression.Contains(" != null");
            var varName = expression.Replace(" != null", "").Replace(" == null", "").Trim();
            var value = context.GetParameter(varName);

            return isNotNull ? value != null : value == null;
        }

        // Handle "name" - just check if parameter exists and is truthy
        if (!expression.Contains(" ") && !expression.Contains("=") && !expression.Contains(">") && !expression.Contains("<"))
        {
            var value = context.GetParameter(expression);

            // Handle boolean values
            if (value is bool boolValue)
                return boolValue;

            return value != null && !IsEmpty(value);
        }

        // Handle comparison operators: ==, !=, >, <, >=, <=
        var match = Regex.Match(expression, @"^(.+?)\s*(==|!=|>=|<=|>|<)\s*(.+?)$");
        if (match.Success)
        {
            var left = match.Groups[1].Value.Trim();
            var op = match.Groups[2].Value;
            var right = match.Groups[3].Value.Trim();

            var leftValue = GetValue(left, context);
            var rightValue = GetValue(right, context);

            return Compare(leftValue, op, rightValue);
        }

        return false;
    }

    private static object? GetValue(string expr, DynamicContext context)
    {
        expr = expr.Trim();

        // If it's a quoted string literal, return the string value
        if ((expr.StartsWith("'") && expr.EndsWith("'")) ||
            (expr.StartsWith("\"") && expr.EndsWith("\"")))
        {
            return expr.Substring(1, expr.Length - 2);
        }

        // If it's a number
        if (int.TryParse(expr, out var intVal))
            return intVal;
        if (double.TryParse(expr, out var doubleVal))
            return doubleVal;

        // If it's a boolean
        if (bool.TryParse(expr, out var boolVal))
            return boolVal;

        // If it's null keyword
        if (expr.Equals("null", StringComparison.OrdinalIgnoreCase))
            return null;

        // Otherwise, it's a parameter name
        return context.GetParameter(expr);
    }

    private static bool Compare(object? left, string op, object? right)
    {
        // Handle null comparisons
        if (left == null || right == null)
        {
            return op switch
            {
                "==" => left == right,
                "!=" => left != right,
                _ => false
            };
        }

        // Try numeric comparison
        if (IsNumeric(left) && IsNumeric(right))
        {
            var leftNum = Convert.ToDouble(left);
            var rightNum = Convert.ToDouble(right);

            return op switch
            {
                "==" => Math.Abs(leftNum - rightNum) < 0.0001,
                "!=" => Math.Abs(leftNum - rightNum) >= 0.0001,
                ">" => leftNum > rightNum,
                "<" => leftNum < rightNum,
                ">=" => leftNum >= rightNum,
                "<=" => leftNum <= rightNum,
                _ => false
            };
        }

        // String/object comparison - use case-insensitive for strings
        var leftStr = left.ToString() ?? "";
        var rightStr = right.ToString() ?? "";

        // For == and !=, use case-insensitive comparison
        if (op == "==" || op == "!=")
        {
            var equals = string.Equals(leftStr, rightStr, StringComparison.OrdinalIgnoreCase);
            return op == "==" ? equals : !equals;
        }

        // For ordering comparisons, use ordinal
        var comparison = string.Compare(leftStr, rightStr, StringComparison.Ordinal);
        return op switch
        {
            ">" => comparison > 0,
            "<" => comparison < 0,
            ">=" => comparison >= 0,
            "<=" => comparison <= 0,
            _ => false
        };
    }

    private static bool IsNumeric(object? value)
    {
        return value is int or long or float or double or decimal;
    }

    private static bool IsEmpty(object? value)
    {
        if (value == null)
            return true;

        if (value is string str)
            return string.IsNullOrWhiteSpace(str);

        if (value is System.Collections.ICollection collection)
            return collection.Count == 0;

        return false;
    }
}
