using System.Xml.Linq;
using System.IO;

namespace MyBatis.NET.Mapper;

public static class XmlMapperLoader
{
    public static List<SqlStatement> LoadFromFile(string filePath)
    {
        var doc = XDocument.Load(filePath);
        var ns = doc.Root?.Attribute("namespace")?.Value ?? "";
        var result = new List<SqlStatement>();

        foreach (var node in doc.Descendants())
        {
            var tag = node.Name.LocalName;
            if (tag is not ("select" or "insert" or "update" or "delete")) continue;

            result.Add(new SqlStatement
            {
                Id = $"{ns}.{node.Attribute("id")?.Value}",
                Sql = node.Value.Trim(),
                ParameterType = node.Attribute("parameterType")?.Value ?? "",
                ResultType = node.Attribute("resultType")?.Value ?? "",
                CommandType = tag
            });
        }

        return result;
    }

    public static List<SqlStatement> LoadFromStream(Stream stream)
    {
        var doc = XDocument.Load(stream);
        var ns = doc.Root?.Attribute("namespace")?.Value ?? "";
        var result = new List<SqlStatement>();

        foreach (var node in doc.Descendants())
        {
            var tag = node.Name.LocalName;
            if (tag is not ("select" or "insert" or "update" or "delete")) continue;

            result.Add(new SqlStatement
            {
                Id = $"{ns}.{node.Attribute("id")?.Value}",
                Sql = node.Value.Trim(),
                ParameterType = node.Attribute("parameterType")?.Value ?? "",
                ResultType = node.Attribute("resultType")?.Value ?? "",
                CommandType = tag
            });
        }

        return result;
    }
}
