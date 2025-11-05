using MyBatis.NET.Tests.Models;
namespace MyBatis.NET.Tests.Mappers;

public interface IUserMapper
{
    // Basic CRUD
    List<User> GetAll();
    User? GetById(int id);
    int Insert(User user);
    int Delete(int id);

    // Dynamic SQL - IF conditions
    List<User> FindByNameOrEmail(string? name, string? email);
    List<User> FindByAgeRange(int? minAge, int? maxAge);
    List<User> FindByMultipleConditions(string? name, string? email, int? age, string? role);

    // Dynamic SQL - WHERE clause
    List<User> SearchUsers(string? name, string? email, bool? isActive);

    // Dynamic SQL - CHOOSE/WHEN/OTHERWISE
    List<User> FindByRole(string roleType);
    List<User> FindByStatus(string status);

    // Dynamic SQL - SET for UPDATE
    int UpdateUser(int id, string? userName, string? email, int? age, string? role);
    int UpdateUserSelective(UserUpdateDto user);

    // Dynamic SQL - FOREACH
    List<User> FindByIds(List<int> ids);
    List<User> FindByRoles(List<string> roles);

    // Dynamic SQL - Complex nested
    List<User> ComplexSearch(UserSearchCriteria criteria);

    // Dynamic SQL - TRIM
    List<User> SearchWithTrim(string? name, string? email);
}
