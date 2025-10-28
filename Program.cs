using MyBatis.NET.Mapper;
using MyBatis.NET.Core;

public class User
{
    public int Id { get; set; }
    public string UserName { get; set; } = "";
    public string Email { get; set; } = "";
}

class Program
{
    static void Main()
    {
        MapperAutoLoader.AutoLoad();

        var connStr = "Server=localhost;Database=DemoDb;User Id=sa;Password=123456;TrustServerCertificate=True;";
        using var session = new SqlSession(connStr);

        var mapper = session.GetMapper<IUserMapper>();
        var users = mapper.GetAll();

        Console.WriteLine("== User List ==");
        foreach (var u in users)
            Console.WriteLine($"{u.Id} - {u.UserName} - {u.Email}");
    }
}
