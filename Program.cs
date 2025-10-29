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

        // Test delete
        if (users.Any(u => u.UserName == "TestUser"))
        {
            var testUser = users.First(u => u.UserName == "TestUser");
            int deleted = mapper.DeleteUser(testUser.Id);
            Console.WriteLine($"Deleted {deleted} row(s)");
        }

        // Test update with multiple parameters
        if (users.Any())
        {
            var userToUpdate = users.First();
            int updated = mapper.UpdateUser(userToUpdate.Id, "UpdatedName", "updated@example.com");
            Console.WriteLine($"Updated {updated} row(s)");
        }

        // Get all again to verify delete
        users = mapper.GetAll();
        Console.WriteLine("== User List After Delete ==");
        foreach (var u in users)
            Console.WriteLine($"{u.Id} - {u.UserName} - {u.Email}");
    }
}
