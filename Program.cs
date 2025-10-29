using MyBatis.NET.Mapper;
using MyBatis.NET.Core;

// Ví dụ sử dụng cho DDD với nhiều library
class Program
{
    static void Main()
    {
        // Load từ nhiều folder (cho nhiều library)
        MapperAutoLoader.AutoLoad("Mappers", "../OtherLibrary/Mappers", "../Domain/Mappers");

        // Hoặc load từ embedded resources trong assemblies
        // MapperAutoLoader.AutoLoadFromAssemblies(typeof(Program).Assembly, typeof(SomeOtherClass).Assembly);

        var connStr = "Server=localhost;Database=DemoDb;User Id=sa;Password=123456;TrustServerCertificate=True;";
        using var session = new SqlSession(connStr);

        var mapper = session.GetMapper<IUserMapper>();
        var users = mapper.GetAll();

        Console.WriteLine("== User List ==");
        foreach (var u in users)
            Console.WriteLine($"{u.Id} - {u.UserName} - {u.Email}");

        // Test insert
        var newUser = new User { UserName = "TestUser", Email = "test@example.com" };
        int inserted = mapper.InsertUser(newUser);
        Console.WriteLine($"Inserted {inserted} row(s)");

        // Get all again to verify
        users = mapper.GetAll();
        Console.WriteLine("== User List After Insert ==");
        foreach (var u in users)
            Console.WriteLine($"{u.Id} - {u.UserName} - {u.Email}");
    }
}
