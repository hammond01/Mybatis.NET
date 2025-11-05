namespace MyBatis.NET.Tests.Models;

public class User
{
    public int Id { get; set; }
    public string UserName { get; set; } = "";
    public string? Email { get; set; }
    public int Age { get; set; }
    public string Role { get; set; } = "";
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? DeletedDate { get; set; }

    public override string ToString()
    {
        return $"User(Id={Id}, UserName={UserName}, Email={Email}, Age={Age}, Role={Role}, IsActive={IsActive})";
    }
}

public class UserSearchCriteria
{
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
    public string? SearchType { get; set; }
    public string? SearchValue { get; set; }
    public List<string>? Roles { get; set; }
}

public class UserUpdateDto
{
    public int Id { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public int? Age { get; set; }
    public string? Role { get; set; }
}
