namespace MyBatis.NET.Mapper;

public class SqlStatement
{
    public string Id { get; set; } = "";
    public string Sql { get; set; } = "";
    public string ParameterType { get; set; } = "";
    public string ResultType { get; set; } = "";
    public string CommandType { get; set; } = "Text";
}
