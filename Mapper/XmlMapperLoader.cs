using System.Xml.Linq;
using System.IO;
using MyBatis.NET.DynamicSql;

namespace MyBatis.NET.Mapper;

public static class XmlMapperLoader
{
    public static List<SqlStatement> LoadFromFile(string filePath)
    {
        var doc = XDocument.Load(filePath);
        return ParseDocument(doc);
    }

    public static List<SqlStatement> LoadFromStream(Stream stream)
    {
        var doc = XDocument.Load(stream);
        return ParseDocument(doc);
    }

    private static List<SqlStatement> ParseDocument(XDocument doc)
    {
        var ns = doc.Root?.Attribute("namespace")?.Value ?? "";
        var result = new List<SqlStatement>();

        foreach (var node in doc.Root?.Elements() ?? Enumerable.Empty<XElement>())
        {
            var tag = node.Name.LocalName;
            if (tag is not ("select" or "insert" or "update" or "delete")) continue;

            var stmt = new SqlStatement
            {
                Id = $"{ns}.{node.Attribute("id")?.Value}",
                ParameterType = node.Attribute("parameterType")?.Value ?? "",
                ResultType = node.Attribute("resultType")?.Value ?? "",
                CommandType = tag
            };

            // Parse SQL content - check if it contains dynamic tags
            var sqlNode = ParseSqlNode(node);

            if (sqlNode is TextSqlNode textNode)
            {
                // Static SQL - just store the text
                stmt.Sql = GetTextContent(textNode);
                stmt.RootNode = null;
            }
            else
            {
                // Dynamic SQL - store the node tree
                stmt.RootNode = sqlNode;
                stmt.Sql = ""; // Will be built dynamically
            }

            result.Add(stmt);
        }

        return result;
    }

    private static SqlNode ParseSqlNode(XElement element)
    {
        var nodes = new List<SqlNode>();

        foreach (var node in element.Nodes())
        {
            if (node is XText textNode)
            {
                var text = textNode.Value;
                // Normalize whitespace but keep single spaces between words
                text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ");
                if (!string.IsNullOrWhiteSpace(text))
                {
                    nodes.Add(new TextSqlNode(text));
                }
            }
            else if (node is XElement childElement)
            {
                var dynamicNode = ParseDynamicElement(childElement);
                if (dynamicNode != null)
                {
                    nodes.Add(dynamicNode);
                }
            }
        }

        if (nodes.Count == 0)
        {
            return new TextSqlNode("");
        }
        else if (nodes.Count == 1)
        {
            return nodes[0];
        }
        else
        {
            return new MixedSqlNode(nodes);
        }
    }

    private static SqlNode? ParseDynamicElement(XElement element)
    {
        var tag = element.Name.LocalName.ToLower();

        return tag switch
        {
            "if" => ParseIf(element),
            "where" => ParseWhere(element),
            "set" => ParseSet(element),
            "choose" => ParseChoose(element),
            "foreach" => ParseForEach(element),
            "trim" => ParseTrim(element),
            _ => null
        };
    }

    private static IfSqlNode ParseIf(XElement element)
    {
        var test = element.Attribute("test")?.Value ?? "";
        var contents = ParseSqlNode(element);
        return new IfSqlNode(contents, test);
    }

    private static WhereSqlNode ParseWhere(XElement element)
    {
        var contents = ParseSqlNode(element);
        return new WhereSqlNode(contents);
    }

    private static SetSqlNode ParseSet(XElement element)
    {
        var contents = ParseSqlNode(element);
        return new SetSqlNode(contents);
    }

    private static ChooseSqlNode ParseChoose(XElement element)
    {
        var whenNodes = new List<IfSqlNode>();
        SqlNode? otherwiseNode = null;

        foreach (var child in element.Elements())
        {
            var tag = child.Name.LocalName.ToLower();
            if (tag == "when")
            {
                var test = child.Attribute("test")?.Value ?? "";
                var contents = ParseSqlNode(child);
                whenNodes.Add(new IfSqlNode(contents, test));
            }
            else if (tag == "otherwise")
            {
                otherwiseNode = ParseSqlNode(child);
            }
        }

        return new ChooseSqlNode(whenNodes, otherwiseNode);
    }

    private static ForEachSqlNode ParseForEach(XElement element)
    {
        var collection = element.Attribute("collection")?.Value ?? "list";
        var item = element.Attribute("item")?.Value ?? "item";
        var index = element.Attribute("index")?.Value ?? "index";
        var separator = element.Attribute("separator")?.Value ?? "";
        var open = element.Attribute("open")?.Value ?? "";
        var close = element.Attribute("close")?.Value ?? "";

        var contents = ParseSqlNode(element);
        return new ForEachSqlNode(contents, collection, item, index, separator, open, close);
    }

    private static TrimSqlNode ParseTrim(XElement element)
    {
        var prefix = element.Attribute("prefix")?.Value ?? "";
        var suffix = element.Attribute("suffix")?.Value ?? "";
        var prefixOverrides = element.Attribute("prefixOverrides")?.Value ?? "";
        var suffixOverrides = element.Attribute("suffixOverrides")?.Value ?? "";

        var contents = ParseSqlNode(element);
        return new TrimSqlNode(contents, prefix, suffix, prefixOverrides, suffixOverrides);
    }

    private static string GetTextContent(SqlNode node)
    {
        if (node is TextSqlNode textNode)
        {
            var context = new DynamicContext(new Dictionary<string, object?>());
            textNode.Apply(context);
            return context.GetSql();
        }
        return "";
    }
}
