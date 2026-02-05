using Xunit;
using MyBatis.NET.Core;
using MyBatis.NET.Mapper;
using MyBatis.NET.Tests.Models;
using MyBatis.NET.Tests.Mappers;

namespace MyBatis.NET.Tests.Integration;

/// <summary>
/// Integration tests simulating real user scenarios with Dynamic SQL
/// Prerequisites: Run TestDatabase.sql to create test database
/// </summary>
[Trait("Category", "Integration")]
public class DynamicSqlIntegrationTests : IDisposable
{
    private readonly SqlSession _session;
    private readonly IUserMapper _mapper;
    private const string ConnectionString = "Server=localhost;Database=MyBatisTestDB;Integrated Security=true;TrustServerCertificate=true;";

    public DynamicSqlIntegrationTests()
    {
        // Load mappers from XML
        MapperAutoLoader.AutoLoad("Mappers");

        // Create session
        _session = new SqlSession(ConnectionString);
        _mapper = _session.GetMapper<IUserMapper>();
    }

    [Fact]
    public void Test01_GetAll_ShouldReturnAllUsers()
    {
        // Act
        var users = _mapper.GetAll();

        // Assert
        Assert.NotEmpty(users);
        Assert.True(users.Count >= 10);
        Assert.All(users, user => Assert.NotNull(user.UserName));
    }

    [Fact]
    public void Test02_FindByNameOrEmail_OnlyName_ShouldFilterByName()
    {
        // Act
        var users = _mapper.FindByNameOrEmail(name: "john", email: null);

        // Assert
        Assert.NotEmpty(users);
        Assert.All(users, user => Assert.Contains("john", user.UserName.ToLower()));
    }

    [Fact]
    public void Test03_FindByNameOrEmail_OnlyEmail_ShouldFilterByEmail()
    {
        // Act
        var users = _mapper.FindByNameOrEmail(name: null, email: "example.com");

        // Assert
        Assert.NotEmpty(users);
        Assert.All(users, user =>
        {
            if (user.Email != null)
                Assert.Contains("example.com", user.Email);
        });
    }

    [Fact]
    public void Test04_FindByNameOrEmail_BothNull_ShouldReturnAll()
    {
        // Act
        var users = _mapper.FindByNameOrEmail(name: null, email: null);

        // Assert
        Assert.NotEmpty(users);
        Assert.True(users.Count >= 10);
    }

    [Fact]
    public void Test05_FindByAgeRange_MinAgeOnly_ShouldFilterCorrectly()
    {
        // Act
        var users = _mapper.FindByAgeRange(minAge: 30, maxAge: 0);

        // Assert
        Assert.NotEmpty(users);
        Assert.All(users, user => Assert.True(user.Age >= 30));
    }

    [Fact]
    public void Test06_FindByAgeRange_MaxAgeOnly_ShouldFilterCorrectly()
    {
        // Act
        var users = _mapper.FindByAgeRange(minAge: 0, maxAge: 25);

        // Assert
        Assert.NotEmpty(users);
        Assert.All(users, user => Assert.True(user.Age <= 25));
    }

    [Fact]
    public void Test07_FindByAgeRange_BothMinAndMax_ShouldFilterRange()
    {
        // Act
        var users = _mapper.FindByAgeRange(minAge: 20, maxAge: 35);

        // Assert
        Assert.NotEmpty(users);
        Assert.All(users, user =>
        {
            Assert.True(user.Age >= 20);
            Assert.True(user.Age <= 35);
        });
    }

    [Fact]
    public void Test08_FindByMultipleConditions_AllNull_ShouldReturnAll()
    {
        // Act
        var users = _mapper.FindByMultipleConditions(null, null, 0, null);

        // Assert
        Assert.NotEmpty(users);
    }

    [Fact]
    public void Test09_SearchUsers_WithName_ShouldFilter()
    {
        // Act
        var users = _mapper.SearchUsers(name: "john", email: null, isActive: null);

        // Assert
        Assert.NotEmpty(users);
        Assert.All(users, user => Assert.Contains("john", user.UserName.ToLower()));
    }

    [Fact]
    public void Test10_SearchUsers_WithActiveStatus_ShouldFilter()
    {
        // Act
        var activeUsers = _mapper.SearchUsers(name: null, email: null, isActive: true);
        var inactiveUsers = _mapper.SearchUsers(name: null, email: null, isActive: false);

        // Assert
        Assert.NotEmpty(activeUsers);
        Assert.All(activeUsers, user => Assert.True(user.IsActive));

        Assert.NotEmpty(inactiveUsers);
        Assert.All(inactiveUsers, user => Assert.False(user.IsActive));
    }

    [Fact]
    public void Test11_FindByRole_Admin_ShouldReturnAdmins()
    {
        // Act
        var users = _mapper.FindByRole("admin");

        // Assert
        Assert.NotEmpty(users);
        Assert.All(users, user => Assert.Equal("Admin", user.Role));
    }

    [Fact]
    public void Test12_FindByRole_Manager_ShouldReturnManagers()
    {
        // Act
        var users = _mapper.FindByRole("manager");

        // Assert
        Assert.NotEmpty(users);
        Assert.All(users, user => Assert.Equal("Manager", user.Role));
    }

    [Fact]
    public void Test13_FindByRole_Unknown_ShouldUseOtherwise()
    {
        // Act
        var users = _mapper.FindByRole("unknown");

        // Assert
        Assert.NotEmpty(users);
        Assert.All(users, user => Assert.True(user.Role == "User" || user.Role == "Guest"));
    }

    [Fact]
    public void Test14_FindByStatus_Active_ShouldReturnActiveOnly()
    {
        // Act
        var users = _mapper.FindByStatus("active");

        // Assert
        Assert.NotEmpty(users);
        Assert.All(users, user =>
        {
            Assert.True(user.IsActive);
            Assert.Null(user.DeletedDate);
        });
    }

    [Fact]
    public void Test15_FindByStatus_Deleted_ShouldReturnDeletedOnly()
    {
        // Act
        var users = _mapper.FindByStatus("deleted");

        // Assert
        Assert.NotEmpty(users);
        Assert.All(users, user => Assert.NotNull(user.DeletedDate));
    }

    [Fact]
    public void Test16_UpdateUser_SingleField_ShouldUpdateOnlyThatField()
    {
        // Arrange
        var originalUser = _mapper.GetById(1);
        Assert.NotNull(originalUser);
        var originalEmail = originalUser.Email;

        // Act - Update only username
        var rowsAffected = _mapper.UpdateUser(
            id: 1,
            userName: "updated_john",
            email: null,
            age: 0,
            role: null
        );

        // Assert
        Assert.Equal(1, rowsAffected);
        var updatedUser = _mapper.GetById(1);
        Assert.NotNull(updatedUser);
        Assert.Equal("updated_john", updatedUser.UserName);
        Assert.Equal(originalEmail, updatedUser.Email); // Email should not change

        // Cleanup - restore original
        _mapper.UpdateUser(1, originalUser.UserName, null, 0, null);
    }

    [Fact]
    public void Test17_UpdateUser_MultipleFields_ShouldUpdateAll()
    {
        // Arrange
        var originalUser = _mapper.GetById(2);
        Assert.NotNull(originalUser);

        // Act
        var rowsAffected = _mapper.UpdateUser(
            id: 2,
            userName: "updated_jane",
            email: "updated@example.com",
            age: 31,
            role: "SuperAdmin"
        );

        // Assert
        Assert.Equal(1, rowsAffected);
        var updatedUser = _mapper.GetById(2);
        Assert.NotNull(updatedUser);
        Assert.Equal("updated_jane", updatedUser.UserName);
        Assert.Equal("updated@example.com", updatedUser.Email);
        Assert.Equal(31, updatedUser.Age);
        Assert.Equal("SuperAdmin", updatedUser.Role);

        // Cleanup
        _mapper.UpdateUser(2, originalUser.UserName, originalUser.Email, originalUser.Age, originalUser.Role);
    }

    [Fact]
    public void Test18_UpdateUserSelective_UsingDto_ShouldWork()
    {
        // Arrange
        var dto = new UserUpdateDto
        {
            Id = 3,
            UserName = "updated_bob",
            Email = null, // Should not update
            Age = null,   // Should not update
            Role = "Senior User"
        };

        var originalUser = _mapper.GetById(3);
        Assert.NotNull(originalUser);

        // Act
        var rowsAffected = _mapper.UpdateUserSelective(dto);

        // Assert
        Assert.Equal(1, rowsAffected);
        var updatedUser = _mapper.GetById(3);
        Assert.NotNull(updatedUser);
        Assert.Equal("updated_bob", updatedUser.UserName);
        Assert.Equal("Senior User", updatedUser.Role);
        Assert.Equal(originalUser.Email, updatedUser.Email); // Should not change

        // Cleanup
        _mapper.UpdateUser(3, originalUser.UserName, null, 0, originalUser.Role);
    }

    [Fact]
    public void Test19_FindByIds_ShouldReturnSpecificUsers()
    {
        // Act
        var users = _mapper.FindByIds(new List<int> { 1, 2, 3, 4, 5 });

        // Assert
        Assert.Equal(5, users.Count);
        Assert.Contains(users, u => u.Id == 1);
        Assert.Contains(users, u => u.Id == 2);
        Assert.Contains(users, u => u.Id == 3);
        Assert.Contains(users, u => u.Id == 4);
        Assert.Contains(users, u => u.Id == 5);
    }

    [Fact]
    public void Test20_FindByRoles_ShouldReturnUsersWithSpecificRoles()
    {
        // Act
        var users = _mapper.FindByRoles(new List<string> { "Admin", "Manager" });

        // Assert
        Assert.NotEmpty(users);
        Assert.All(users, user =>
            Assert.True(user.Role == "Admin" || user.Role == "Manager")
        );
    }

    [Fact]
    public void Test21_ComplexSearch_AllFilters_ShouldCombineCorrectly()
    {
        // Arrange
        var criteria = new UserSearchCriteria
        {
            MinAge = 20,
            MaxAge = 40,
            Roles = new List<string> { "Admin", "User" },
            IsActive = true
        };

        // Act
        var users = _mapper.ComplexSearch(criteria);

        // Assert
        Assert.NotEmpty(users);
        Assert.All(users, user =>
        {
            Assert.True(user.Age >= 20);
            Assert.True(user.Age <= 40);
            Assert.True(user.Role == "Admin" || user.Role == "User");
            Assert.True(user.IsActive);
        });
    }

    [Fact]
    public void Test22_ComplexSearch_WithSearchType_ShouldFilterCorrectly()
    {
        // Arrange
        var criteria = new UserSearchCriteria
        {
            SearchType = "name",
            SearchValue = "john"
        };

        // Act
        var users = _mapper.ComplexSearch(criteria);

        // Assert
        Assert.NotEmpty(users);
        Assert.All(users, user =>
            Assert.Contains("john", user.UserName.ToLower())
        );
    }

    [Fact]
    public void Test23_ComplexSearch_NoFilters_ShouldReturnAll()
    {
        // Arrange
        var criteria = new UserSearchCriteria();

        // Act
        var users = _mapper.ComplexSearch(criteria);

        // Assert
        Assert.NotEmpty(users);
        Assert.True(users.Count >= 10);
    }

    [Fact]
    public void Test24_SearchWithTrim_ShouldHandlePrefixOverrides()
    {
        // Act - Test with AND prefix
        var users1 = _mapper.SearchWithTrim(name: "john", email: null);

        // Act - Test with OR prefix
        var users2 = _mapper.SearchWithTrim(name: null, email: "example.com");

        // Assert
        Assert.NotEmpty(users1);
        Assert.NotEmpty(users2);
    }

    [Fact]
    public void Test25_InsertAndDelete_ShouldWork()
    {
        // Arrange
        var newUser = new User
        {
            UserName = "test_user_temp",
            Email = "temp@test.com",
            Age = 99,
            Role = "Test",
            IsActive = true
        };

        // Act - Insert
        var insertResult = _mapper.Insert(newUser);
        Assert.Equal(1, insertResult);

        // Verify insertion
        var allUsers = _mapper.GetAll();
        var insertedUser = allUsers.FirstOrDefault(u => u.UserName == "test_user_temp");
        Assert.NotNull(insertedUser);

        // Act - Delete
        var deleteResult = _mapper.Delete(insertedUser.Id);
        Assert.Equal(1, deleteResult);

        // Verify deletion
        var userAfterDelete = _mapper.GetById(insertedUser.Id);
        Assert.Null(userAfterDelete);
    }

    public void Dispose()
    {
        _session?.Dispose();
    }
}
