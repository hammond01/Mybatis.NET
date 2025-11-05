using System.Text;
using System.Xml.Linq;

namespace MyBatis.NET.Tools;

/// <summary>
/// Tool to generate Mapper Interface from XML Mapper file
/// Ensures Interface and XML are always in sync
/// </summary>
public class MapperInterfaceGenerator
{
    public static string GenerateInterface(string xmlFilePath, string? namespaceOverride = null)
    {
        if (!File.Exists(xmlFilePath))
            throw new FileNotFoundException($"XML mapper file not found: {xmlFilePath}");

        var xml = XDocument.Load(xmlFilePath);
        var mapper = xml.Root ?? throw new InvalidOperationException("Invalid XML: missing root element");

        var mapperNamespace = mapper.Attribute("namespace")?.Value
            ?? throw new InvalidOperationException("Invalid XML: missing namespace attribute");

        var statements = mapper.Elements()
            .Where(e => e.Name.LocalName is "select" or "insert" or "update" or "delete")
            .ToList();

        var sb = new StringBuilder();

        // Using statements
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine();

        // Namespace
        var targetNamespace = namespaceOverride ?? ExtractNamespace(xmlFilePath);
        sb.AppendLine($"namespace {targetNamespace};");
        sb.AppendLine();

        // Interface definition
        sb.AppendLine($"/// <summary>");
        sb.AppendLine($"/// Auto-generated from {Path.GetFileName(xmlFilePath)}");
        sb.AppendLine($"/// Generated at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"/// </summary>");
        sb.AppendLine($"public interface {mapperNamespace}");
        sb.AppendLine("{");

        foreach (var stmt in statements)
        {
            var method = GenerateMethod(stmt);
            sb.AppendLine($"    {method}");
            sb.AppendLine();
        }

        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string GenerateMethod(XElement statement)
    {
        var statementType = statement.Name.LocalName;
        var id = statement.Attribute("id")?.Value ?? "Unknown";
        var paramType = statement.Attribute("parameterType")?.Value;
        var resultType = statement.Attribute("resultType")?.Value;

        // Analyze SQL to detect parameters
        var sql = statement.Value;
        var parameters = ExtractParameters(sql, statement);

        // Build method signature
        var returnType = statementType switch
        {
            "select" => DetermineSelectReturnType(statement, resultType),
            "insert" or "update" or "delete" => "int",
            _ => "int"
        };

        // Build parameter list
        var paramList = string.Join(", ", parameters.Select(p => $"{p.Type} {p.Name}"));

        return $"{returnType} {id}({paramList});";
    }

    private static string DetermineSelectReturnType(XElement statement, string? resultType)
    {
        var typeName = resultType ?? "dynamic";

        // Check for REQUIRED returnSingle attribute
        var returnSingleAttr = statement.Attribute("returnSingle");
        if (returnSingleAttr == null)
        {
            var id = statement.Attribute("id")?.Value ?? "Unknown";
            throw new InvalidOperationException(
                $"Missing REQUIRED attribute 'returnSingle' in <select id=\"{id}\">. " +
                $"Please add returnSingle=\"true\" for single object or returnSingle=\"false\" for list.");
        }

        var returnSingleValue = returnSingleAttr.Value.ToLower();

        if (returnSingleValue == "true")
        {
            // Single object - nullable
            return $"{typeName}?";
        }
        else if (returnSingleValue == "false")
        {
            // List of objects
            return $"List<{typeName}>";
        }
        else
        {
            var id = statement.Attribute("id")?.Value ?? "Unknown";
            throw new InvalidOperationException(
                $"Invalid value for 'returnSingle' in <select id=\"{id}\">. " +
                $"Expected 'true' or 'false', got '{returnSingleAttr.Value}'");
        }
    }

    private static List<(string Name, string Type)> ExtractParameters(string sql, XElement statement)
    {
        var parameters = new List<(string Name, string Type)>();
        var paramType = statement.Attribute("parameterType")?.Value;

        // If parameterType is specified and it's a complex type (not int, string, etc.)
        if (paramType != null && !IsPrimitiveType(paramType))
        {
            parameters.Add((ToCamelCase(paramType), paramType));
            return parameters;
        }

        // Extract @paramName from SQL
        var matches = System.Text.RegularExpressions.Regex.Matches(sql, @"@(\w+)");
        var uniqueParams = matches.Cast<System.Text.RegularExpressions.Match>()
            .Select(m => m.Groups[1].Value)
            .Distinct()
            .Where(p => !p.EndsWith("_0") && !p.EndsWith("_1")) // Exclude ForEach generated params
            .ToList();

        // Check for collections in foreach
        var foreachElements = statement.Descendants()
            .Where(e => e.Name.LocalName == "foreach")
            .ToList();

        foreach (var fe in foreachElements)
        {
            var collection = fe.Attribute("collection")?.Value;
            if (collection != null && !parameters.Any(p => p.Name == collection))
            {
                parameters.Add((collection, $"List<string>")); // Default to List<string>, can be improved
            }
        }

        // Extract parameters from if/where conditions
        var ifElements = statement.Descendants()
            .Where(e => e.Name.LocalName == "if")
            .ToList();

        foreach (var ifElem in ifElements)
        {
            var test = ifElem.Attribute("test")?.Value;
            if (test != null)
            {
                var condParams = ExtractParameterNamesFromCondition(test);
                foreach (var param in condParams)
                {
                    if (!parameters.Any(p => p.Name == param))
                    {
                        var type = GuessType(param);
                        parameters.Add((param, type));
                    }
                }
            }
        }

        // If no parameters found from analysis, check SQL @params
        if (parameters.Count == 0)
        {
            foreach (var param in uniqueParams)
            {
                var type = GuessType(param);
                parameters.Add((param, type));
            }
        }

        return parameters;
    }

    private static List<string> ExtractParameterNamesFromCondition(string condition)
    {
        var parameters = new List<string>();
        var matches = System.Text.RegularExpressions.Regex.Matches(condition, @"\b([a-zA-Z]\w*)\b(?!\s*==)");

        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            var name = match.Groups[1].Value;
            if (name != "null" && name != "and" && name != "or" && !name.StartsWith("'"))
            {
                parameters.Add(name);
            }
        }

        return parameters;
    }

    private static string GuessType(string paramName)
    {
        var lower = paramName.ToLower();

        if (lower.Contains("id")) return "int";
        if (lower.Contains("count") || lower.Contains("age") || lower.Contains("year")) return "int?";
        if (lower.Contains("price") || lower.Contains("amount")) return "decimal?";
        if (lower.Contains("date") || lower.Contains("time")) return "DateTime?";
        if (lower.Contains("active") || lower.Contains("enabled") || lower.Contains("is")) return "bool?";
        if (lower.Contains("name") || lower.Contains("email") || lower.Contains("role") || lower.Contains("type")) return "string?";

        return "string?"; // Default
    }

    private static bool IsPrimitiveType(string type)
    {
        return type switch
        {
            "int" or "long" or "short" or "byte" or
            "string" or "bool" or "decimal" or "double" or "float" or
            "DateTime" or "Guid" => true,
            _ => false
        };
    }

    private static string ToCamelCase(string str)
    {
        if (string.IsNullOrEmpty(str)) return str;
        return char.ToLower(str[0]) + str.Substring(1);
    }

    private static string ExtractNamespace(string xmlFilePath)
    {
        // Try to infer namespace from file path
        var dir = Path.GetDirectoryName(xmlFilePath) ?? "";
        var projectName = FindProjectName(dir);

        if (projectName != null)
        {
            return $"{projectName}.Mappers";
        }

        return "Mappers";
    }

    private static string? FindProjectName(string directory)
    {
        var current = new DirectoryInfo(directory);

        while (current != null)
        {
            var csprojFiles = current.GetFiles("*.csproj");
            if (csprojFiles.Length > 0)
            {
                return Path.GetFileNameWithoutExtension(csprojFiles[0].Name);
            }

            current = current.Parent;
        }

        return null;
    }

    public static void GenerateAndSave(string xmlFilePath, string? outputPath = null, string? namespaceOverride = null)
    {
        var interfaceCode = GenerateInterface(xmlFilePath, namespaceOverride);

        // Determine output path
        if (outputPath == null)
        {
            var xmlDir = Path.GetDirectoryName(xmlFilePath) ?? "";
            var xmlFileName = Path.GetFileNameWithoutExtension(xmlFilePath);

            // Generate IMapperName.cs from MapperName.xml
            var interfaceName = xmlFileName.StartsWith("I") ? xmlFileName : $"I{xmlFileName}";
            if (!interfaceName.EndsWith("Mapper"))
            {
                interfaceName += "Mapper";
            }

            outputPath = Path.Combine(xmlDir, $"{interfaceName}.cs");
        }

        File.WriteAllText(outputPath, interfaceCode);
        Console.WriteLine($"✅ Generated: {outputPath}");
    }
}
