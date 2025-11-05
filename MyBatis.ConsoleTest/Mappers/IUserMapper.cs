using MyBatis.ConsoleTest.Models;

namespace MyBatis.ConsoleTest.Mappers;

public interface IUserMapper
{
    // Simple queries
    List<User> GetAll();
    User? GetById(int id);

    // Dynamic SQL
    List<User> SearchUsers(string? userName, string? role, int? minAge, int? maxAge, bool? isActive);

    // ForEach
    List<User> FindByRoles(List<string> roles);
    List<User> FindByIds(List<int> ids);

    // CRUD
    int InsertUser(User user);
    int UpdateUser(User user);
    int DeleteUser(int id);
    int SoftDeleteUser(int id);

    // Advanced
    List<User> SearchByType(string searchType, string searchValue);
    List<User> ComplexSearch(string? userName, string? email, List<string>? roles, int? minAge, int? maxAge, bool? isActive, DateTime? createdAfter, string? orderBy);
    int CountUsers(string? role, bool? isActive);
}
